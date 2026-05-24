using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Decide en el servidor qué slot (jugador) recibe cada parte del rompecabezas
// y sincroniza las decisiones a todos los clientes.
//
// Para el primer rompecabezas (código de colores):
//   - Un slot recibe la CONSOLA + NOTA DE ORDEN DE COLORES (el "solucionador")
//   - Los otros 3 slots reciben las 4 NOTAS NUMÉRICAS COLOREADAS, distribuidas
//
// Pon este componente en un GameObject de la escena GameMap con NetworkObject.
public class PuzzleAssignmentManager : NetworkBehaviour
{
    public static PuzzleAssignmentManager Instance;

    // Slot que recibe la consola del puzzle de código (0..3)
    public NetworkVariable<int> CodePuzzleSlot = new(-1,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Slot donde se ubica la nota numérica de cada color (0..3)
    public NetworkVariable<int> RojoSlot     = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> AzulSlot     = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> VerdeSlot    = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> AmarilloSlot = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Tooltip("Cantidad total de slots/mapas (normalmente 4).")]
    [SerializeField] private int totalSlots = 4;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            GenerarAsignacion();

        // Notificar a los map bindings cuando los valores se actualicen
        CodePuzzleSlot.OnValueChanged += (_, _) => NotifyMapBindings();
        RojoSlot.OnValueChanged       += (_, _) => NotifyMapBindings();
        AzulSlot.OnValueChanged       += (_, _) => NotifyMapBindings();
        VerdeSlot.OnValueChanged      += (_, _) => NotifyMapBindings();
        AmarilloSlot.OnValueChanged   += (_, _) => NotifyMapBindings();

        // Si ya hay valores cuando entramos, refrescar inmediatamente
        NotifyMapBindings();
    }

    private void GenerarAsignacion()
    {
        // 1) Slot del solucionador (la consola)
        int codeSlot = Random.Range(0, totalSlots);
        CodePuzzleSlot.Value = codeSlot;

        // 2) Distribuir las 4 notas numéricas entre los OTROS slots
        //    (no en el slot del solucionador — él no puede leer sus propias pistas)
        var otherSlots = new List<int>();
        for (int i = 0; i < totalSlots; i++)
            if (i != codeSlot) otherSlots.Add(i);

        // Asignamos un slot a cada color. Como tenemos 4 colores y 3 slots
        // disponibles (en una partida de 4 jugadores), al menos un slot tendrá
        // 2 notas. Hacemos round-robin barajado para distribuir equitativamente.
        Shuffle(otherSlots);
        RojoSlot.Value     = otherSlots[0 % otherSlots.Count];
        AzulSlot.Value     = otherSlots[1 % otherSlots.Count];
        VerdeSlot.Value    = otherSlots[2 % otherSlots.Count];
        AmarilloSlot.Value = otherSlots[3 % otherSlots.Count];

        Debug.Log($"[PuzzleAssignmentManager] Asignación: " +
                  $"Console→Slot {CodePuzzleSlot.Value}, " +
                  $"Rojo→{RojoSlot.Value}, Azul→{AzulSlot.Value}, " +
                  $"Verde→{VerdeSlot.Value}, Amarillo→{AmarilloSlot.Value}");
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void NotifyMapBindings()
    {
        foreach (var binding in FindObjectsOfType<PuzzleMapBinding>())
            binding.ApplyAssignment();
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
    }
}

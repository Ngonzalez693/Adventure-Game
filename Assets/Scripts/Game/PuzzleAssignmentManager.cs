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

    // Slot que recibe la consola del puzzle de patrón (palancas + luces)
    public NetworkVariable<int> PatternPuzzleSlot = new(-1,
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
        CodePuzzleSlot.OnValueChanged    += (_, _) => NotifyMapBindings();
        PatternPuzzleSlot.OnValueChanged += (_, _) => NotifyMapBindings();
        RojoSlot.OnValueChanged          += (_, _) => NotifyMapBindings();
        AzulSlot.OnValueChanged          += (_, _) => NotifyMapBindings();
        VerdeSlot.OnValueChanged         += (_, _) => NotifyMapBindings();
        AmarilloSlot.OnValueChanged      += (_, _) => NotifyMapBindings();

        // Si ya hay valores cuando entramos, refrescar inmediatamente
        NotifyMapBindings();
    }

    private void GenerarAsignacion()
    {
        // 1) Slot del solucionador del puzzle de código
        int codeSlot = Random.Range(0, totalSlots);
        CodePuzzleSlot.Value = codeSlot;

        // 2) Slot del solucionador del puzzle de patrón (debe ser distinto al del código)
        var availablePattern = new List<int>();
        for (int i = 0; i < totalSlots; i++)
            if (i != codeSlot) availablePattern.Add(i);
        int patternSlot = availablePattern[Random.Range(0, availablePattern.Count)];
        PatternPuzzleSlot.Value = patternSlot;

        // 3) Distribuir las 4 notas numéricas entre los OTROS slots del puzzle de código
        var otherSlotsForCode = new List<int>();
        for (int i = 0; i < totalSlots; i++)
            if (i != codeSlot) otherSlotsForCode.Add(i);

        Shuffle(otherSlotsForCode);
        RojoSlot.Value     = otherSlotsForCode[0 % otherSlotsForCode.Count];
        AzulSlot.Value     = otherSlotsForCode[1 % otherSlotsForCode.Count];
        VerdeSlot.Value    = otherSlotsForCode[2 % otherSlotsForCode.Count];
        AmarilloSlot.Value = otherSlotsForCode[3 % otherSlotsForCode.Count];

        Debug.Log($"[PuzzleAssignmentManager] Asignación: " +
                  $"Code→Slot {CodePuzzleSlot.Value}, Pattern→Slot {PatternPuzzleSlot.Value}, " +
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

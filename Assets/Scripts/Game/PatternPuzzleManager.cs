using Unity.Netcode;
using UnityEngine;

// Puzzle de patrón con palancas binarias:
//  - Cada jugador AYUDANTE tiene 3 palancas (cada una con 2 posiciones: 0/1).
//  - Las 3 palancas de un mismo jugador forman una "combinación" de 3 bits
//    (valor 0..7). Ej: palancas en [0,1,1] = combinación binaria 011 = 3.
//  - El host genera una combinación OBJETIVO aleatoria para cada ayudante.
//  - Cuando la combinación de un jugador coincide con su objetivo, su LUZ
//    correspondiente se enciende (verde).
//  - El jugador SOLUCIONADOR ve las 3 luces y solo da feedback verbal a los
//    ayudantes ("tu luz está encendida / apagada").
//  - Puzzle resuelto cuando las 3 luces de los ayudantes están encendidas.
//
// El estado de las 3 palancas de un slot se codifica como un entero de 3 bits:
//   bit 0 = palanca 0, bit 1 = palanca 1, bit 2 = palanca 2.
public class PatternPuzzleManager : NetworkBehaviour
{
    public static PatternPuzzleManager Instance;

    public const int SlotCount  = 4;  // un slot por mapa
    public const int LeversPerSlot = 3;
    public const int CombinationCount = 1 << LeversPerSlot; // 2^3 = 8

    // Estado actual de las 3 palancas por slot (codificado como int 0..7)
    public NetworkVariable<int> Slot0State = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Slot1State = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Slot2State = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Slot3State = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Combinaciones objetivo (servidor)
    public NetworkVariable<int> Slot0Target = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Slot1Target = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Slot2Target = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Slot3Target = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsSolved = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            GenerarObjetivos();

        IsSolved.OnValueChanged += OnSolvedChanged;
    }

    public override void OnNetworkDespawn()
    {
        IsSolved.OnValueChanged -= OnSolvedChanged;
        if (Instance == this) Instance = null;
    }

    private void OnSolvedChanged(bool previous, bool next)
    {
        if (next && !previous && SoundManager.Instance != null)
            SoundManager.Instance.PlayPuzzleSolved();
    }

    private void GenerarObjetivos()
    {
        Slot0Target.Value = Random.Range(0, CombinationCount);
        Slot1Target.Value = Random.Range(0, CombinationCount);
        Slot2Target.Value = Random.Range(0, CombinationCount);
        Slot3Target.Value = Random.Range(0, CombinationCount);

        Debug.Log($"[PatternPuzzleManager] Objetivos: " +
                  $"slot0={ToBinary(Slot0Target.Value)} ({Slot0Target.Value}), " +
                  $"slot1={ToBinary(Slot1Target.Value)} ({Slot1Target.Value}), " +
                  $"slot2={ToBinary(Slot2Target.Value)} ({Slot2Target.Value}), " +
                  $"slot3={ToBinary(Slot3Target.Value)} ({Slot3Target.Value})");
    }

    // Consultas
    public int GetSlotState(int slot) => slot switch
    {
        0 => Slot0State.Value, 1 => Slot1State.Value,
        2 => Slot2State.Value, 3 => Slot3State.Value, _ => 0
    };

    public int GetSlotTarget(int slot) => slot switch
    {
        0 => Slot0Target.Value, 1 => Slot1Target.Value,
        2 => Slot2Target.Value, 3 => Slot3Target.Value, _ => 0
    };

    // Posición individual de una palanca (0 o 1)
    public int GetLeverPosition(int slot, int leverIndex)
    {
        int state = GetSlotState(slot);
        return (state >> leverIndex) & 1;
    }

    // Verdadero si la combinación actual del slot coincide con su objetivo
    public bool IsSlotLightOn(int slot) => GetSlotState(slot) == GetSlotTarget(slot);

    // RPC: el cliente activa una palanca (toggle binario).
    [ServerRpc(RequireOwnership = false)]
    public void ToggleLeverServerRpc(int slot, int leverIndex)
    {
        if (leverIndex < 0 || leverIndex >= LeversPerSlot) return;

        int state = GetSlotState(slot);
        state ^= (1 << leverIndex); // toggle del bit
        SetSlotState(slot, state);
        CheckSolved();
    }

    private void SetSlotState(int slot, int state)
    {
        switch (slot)
        {
            case 0: Slot0State.Value = state; break;
            case 1: Slot1State.Value = state; break;
            case 2: Slot2State.Value = state; break;
            case 3: Slot3State.Value = state; break;
        }
    }

    private void CheckSolved()
    {
        var assign = PuzzleAssignmentManager.Instance;
        if (assign == null) return;

        int solverSlot = assign.PatternPuzzleSlot.Value;
        for (int i = 0; i < SlotCount; i++)
        {
            if (i == solverSlot) continue;
            if (!IsSlotLightOn(i)) return;
        }

        if (!IsSolved.Value)
        {
            IsSolved.Value = true;
            Debug.Log("[PatternPuzzleManager] Puzzle de patrón RESUELTO.");
        }
    }

    private static string ToBinary(int value)
    {
        return System.Convert.ToString(value, 2).PadLeft(LeversPerSlot, '0');
    }
}

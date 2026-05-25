using Unity.Netcode;
using UnityEngine;

// Puzzle de presión / válvulas:
//  - 8 válvulas en total (2 por mapa, 4 mapas), binarias (abierta/cerrada).
//  - Cada válvula tiene un "delta" aleatorio que aporta a la presión cuando
//    está abierta (rango: -25 a +25, sin cero).
//  - Presión actual = suma de los deltas de las válvulas abiertas.
//  - El servidor calcula un rango OBJETIVO garantizando que sea alcanzable
//    (usa la suma de un subconjunto aleatorio como centro del rango).
//  - El SOLUCIONADOR ve un manómetro con la presión + rango objetivo +
//    cuánto tiempo lleva estable.
//  - Cuando la presión está dentro del rango durante RequiredStableSeconds
//    segundos seguidos, el puzzle se marca como resuelto.
public class PressurePuzzleManager : NetworkBehaviour
{
    public static PressurePuzzleManager Instance;

    public const int SlotCount     = 4;
    public const int ValvesPerSlot = 2;
    public const int TotalValves   = SlotCount * ValvesPerSlot; // 8

    [Header("Tiempos / dificultad")]
    [Tooltip("Segundos en rango antes de marcar resuelto.")]
    public float RequiredStableSeconds = 3f;

    [Tooltip("Ancho del rango objetivo (+/- desde el centro).")]
    public float TargetRangeHalfWidth = 5f;

    // Estado de las 8 válvulas como bitmask (bit i = válvula i abierta).
    public NetworkVariable<int> ValveStates = new(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Deltas asignados a cada válvula (sincronizados).
    private NetworkList<int> _valveDeltas;

    public NetworkVariable<float> CurrentPressure = new(0f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<float> TargetMin = new(0f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<float> TargetMax = new(0f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<float> StableTime = new(0f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsSolved = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        Instance = this;
        _valveDeltas = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            GeneratePuzzle();
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
    }

    private void GeneratePuzzle()
    {
        _valveDeltas.Clear();

        // Generar 8 deltas aleatorios en [-25..25], evitando 0
        int[] deltas = new int[TotalValves];
        for (int i = 0; i < TotalValves; i++)
        {
            int d = Random.Range(-25, 26);
            if (d == 0) d = Random.value > 0.5f ? 10 : -10;
            deltas[i] = d;
            _valveDeltas.Add(d);
        }

        // Elegir un subconjunto aleatorio para garantizar solvencia
        int targetSum = 0;
        for (int i = 0; i < TotalValves; i++)
            if (Random.value > 0.5f) targetSum += deltas[i];

        TargetMin.Value = targetSum - TargetRangeHalfWidth;
        TargetMax.Value = targetSum + TargetRangeHalfWidth;

        Debug.Log($"[PressurePuzzleManager] Deltas válvulas: " +
                  $"[{string.Join(", ", deltas)}], target=[{TargetMin.Value:F0}..{TargetMax.Value:F0}]");
    }

    private void Update()
    {
        if (!IsServer || IsSolved.Value) return;

        // Recalcular presión actual a partir del bitmask y los deltas
        float total = 0f;
        int states = ValveStates.Value;
        for (int i = 0; i < TotalValves; i++)
            if ((states & (1 << i)) != 0) total += GetValveDelta(i);
        CurrentPressure.Value = total;

        // Acumular o resetear tiempo estable
        bool inRange = total >= TargetMin.Value && total <= TargetMax.Value;
        if (inRange)
        {
            StableTime.Value += Time.deltaTime;
            if (StableTime.Value >= RequiredStableSeconds && !IsSolved.Value)
            {
                IsSolved.Value = true;
                Debug.Log("[PressurePuzzleManager] Puzzle de presión RESUELTO.");
            }
        }
        else
        {
            if (StableTime.Value > 0f) StableTime.Value = 0f;
        }
    }

    public int GetValveDelta(int globalIndex)
    {
        if (_valveDeltas == null || globalIndex < 0 || globalIndex >= _valveDeltas.Count) return 0;
        return _valveDeltas[globalIndex];
    }

    public bool IsValveOpen(int slot, int valveIndex)
    {
        int globalIndex = slot * ValvesPerSlot + valveIndex;
        return (ValveStates.Value & (1 << globalIndex)) != 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleValveServerRpc(int slot, int valveIndex)
    {
        if (IsSolved.Value) return;
        int globalIndex = slot * ValvesPerSlot + valveIndex;
        if (globalIndex < 0 || globalIndex >= TotalValves) return;

        ValveStates.Value ^= (1 << globalIndex);
    }
}

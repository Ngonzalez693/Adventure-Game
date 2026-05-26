using Unity.Netcode;
using UnityEngine;

// Estado global de la partida en GameMap:
//   - Tiempo restante (cuenta atrás sincronizada por red).
//   - Estado de la partida (Playing / Won / Lost).
//   - El servidor decrementa el tiempo y comprueba condiciones de victoria/derrota.
//   - Los clientes leen el estado para mostrar HUD y paneles de fin.
//
// Pon este script en el GameObject "PuzzleManagers" (junto a los demás managers,
// usa el mismo NetworkObject).
public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance;

    public enum GameState { Playing = 0, Won = 1, Lost = 2 }

    [Header("Configuración")]
    [Tooltip("Tiempo inicial de la partida en segundos.")]
    public float StartingTime = 600f; // 10 minutos por defecto

    public NetworkVariable<float> TimeRemaining = new(0f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<GameState> CurrentState = new(GameState.Playing,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            TimeRemaining.Value = StartingTime;
            CurrentState.Value  = GameState.Playing;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (CurrentState.Value != GameState.Playing) return;

        // Decrementar tiempo
        TimeRemaining.Value -= Time.deltaTime;

        // Victoria: los 4 rompecabezas resueltos
        if (AreAllPuzzlesSolved())
        {
            CurrentState.Value = GameState.Won;
            Debug.Log("[GameStateManager] ¡Victoria! Todos los rompecabezas resueltos.");
            return;
        }

        // Derrota: se acabó el tiempo
        if (TimeRemaining.Value <= 0f)
        {
            TimeRemaining.Value = 0f;
            CurrentState.Value  = GameState.Lost;
            Debug.Log("[GameStateManager] Derrota. Se acabó el tiempo.");
        }
    }

    public bool IsGameOver => CurrentState.Value != GameState.Playing;

    private static bool AreAllPuzzlesSolved()
    {
        bool code     = PuzzleCodeManager.Instance     != null && PuzzleCodeManager.Instance.IsSolved.Value;
        bool pattern  = PatternPuzzleManager.Instance  != null && PatternPuzzleManager.Instance.IsSolved.Value;
        bool memory   = MemoryPuzzleManager.Instance   != null && MemoryPuzzleManager.Instance.IsSolved.Value;
        bool pressure = PressurePuzzleManager.Instance != null && PressurePuzzleManager.Instance.IsSolved.Value;
        return code && pattern && memory && pressure;
    }

    // Cuenta los puzzles resueltos (útil para mostrar en pantalla de derrota).
    public int CountSolvedPuzzles()
    {
        int n = 0;
        if (PuzzleCodeManager.Instance?.IsSolved.Value     == true) n++;
        if (PatternPuzzleManager.Instance?.IsSolved.Value  == true) n++;
        if (MemoryPuzzleManager.Instance?.IsSolved.Value   == true) n++;
        if (PressurePuzzleManager.Instance?.IsSolved.Value == true) n++;
        return n;
    }
}

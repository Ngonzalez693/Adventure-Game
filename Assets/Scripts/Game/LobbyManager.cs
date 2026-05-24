using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Gestor del Lobby: cuenta jugadores conectados y permite al host
// iniciar la partida cuando todos están listos.
//
// Pon este script en un GameObject de la escena Lobby que tenga
// NetworkObject (puedes ponerlo en el mismo "Multiplayer System").
public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [Header("Configuración")]
    [Tooltip("Cantidad de jugadores necesaria para poder iniciar la partida.")]
    [SerializeField] private int requiredPlayers = 4;

    [Tooltip("Nombre exacto de la escena que se carga al iniciar la partida " +
             "(luego puedes apuntar a la cinemática, después al GameMap).")]
    [SerializeField] private string nextSceneName = "GameMap";

    public int RequiredPlayers => requiredPlayers;

    // ── NetworkVariables: el servidor escribe, todos los clientes leen. ─────
    // Estos valores se sincronizan automáticamente a todos los jugadores.
    public NetworkVariable<int> PlayerCount = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> GameStarting = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public bool IsLobbyFull => PlayerCount.Value >= requiredPlayers;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Inicializar con los jugadores que ya estaban conectados
            // cuando el Lobby se cargó (incluye al host).
            PlayerCount.Value = NetworkManager.ConnectedClientsIds.Count;

            NetworkManager.OnClientConnectedCallback    += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback   += OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback    -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback   -= OnClientDisconnected;
        }

        if (Instance == this) Instance = null;
    }

    private void OnClientConnected(ulong clientId)
    {
        PlayerCount.Value = NetworkManager.ConnectedClientsIds.Count;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        PlayerCount.Value = NetworkManager.ConnectedClientsIds.Count;
    }

    // ────────────────────────────────────────────────
    //  Iniciar la partida (solo host, solo si lobby lleno)
    // ────────────────────────────────────────────────
    // Lo llama StartGameInteractable cuando el host interactúa con el objeto.
    // ServerRpc para que el cliente local pueda solicitar el cambio (aunque
    // en la práctica solo el host puede dispararlo).
    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc(ServerRpcParams rpcParams = default)
    {
        // Validación: solo el host puede iniciar
        if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId)
        {
            Debug.LogWarning("[LobbyManager] Un cliente intentó iniciar la partida.");
            return;
        }

        if (!IsLobbyFull)
        {
            Debug.LogWarning("[LobbyManager] Lobby no está lleno todavía.");
            return;
        }

        if (GameStarting.Value) return; // ya está iniciando

        GameStarting.Value = true;

        // Cargar la escena siguiente sincronizadamente para todos los jugadores.
        NetworkManager.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}

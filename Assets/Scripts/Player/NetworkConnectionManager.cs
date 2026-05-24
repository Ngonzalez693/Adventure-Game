using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Gestor de conexión LAN sin códigos de unión.
// HOST: inicia el servidor y se anuncia con broadcast UDP en la red local.
// CLIENT: escucha broadcasts hasta encontrar un host y se conecta automáticamente.
[RequireComponent(typeof(LanDiscovery))]
public class NetworkConnectionManager : MonoBehaviour
{
    public static NetworkConnectionManager Instance;

    [Header("UI")]
    public GameObject menuPanel;
    public TextMeshProUGUI statusText;

    [Header("Escenas")]
    [Tooltip("Nombre exacto de la escena Lobby (debe estar en Build Settings).")]
    public string lobbySceneName = "Lobby";

    [Header("Red")]
    [Tooltip("Puerto del juego (UnityTransport). Debe ser el mismo en host y cliente.")]
    public ushort gamePort = 7777;

    [Header("Escena al ser desconectado")]
    [Tooltip("Escena a la que se vuelve si perdemos al host (o cualquier desconexión).")]
    public string menuSceneName = "MenuScene";

    private LanDiscovery _discovery;
    private bool _subscribedToNetworkEvents;

    private void Awake()
    {
        // Si ya existe un Instance previo (del flujo anterior), lo destruimos
        // y nosotros (instancia de la escena nueva) tomamos su lugar.
        // Esto es importante cuando el usuario vuelve al menú desde el Lobby:
        // los botones de la nueva MenuScene están conectados a ESTA instancia
        // en su Inspector, no a la antigua persistente.
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _discovery = GetComponent<LanDiscovery>();

        SubscribeToNetworkEvents();
    }

    private void SubscribeToNetworkEvents()
    {
        if (_subscribedToNetworkEvents) return;
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        _subscribedToNetworkEvents = true;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        }
    }

    // ────────────────────────────────────────────────
    //  Eventos de red — detectar pérdida de host
    // ────────────────────────────────────────────────
    // Se dispara cuando NUESTRO cliente se detiene. Si fuimos host, ya manejamos
    // el regreso al menú en UiInGame, así que ignoramos ese caso aquí. Si fuimos
    // un cliente puro, significa que el host se cayó (o nos desconectamos) y
    // debemos volver al menú forzadamente.
    private void OnClientStopped(bool wasHost)
    {
        if (wasHost) return;
        ReturnToMenuFromDisconnect("Conexión perdida con el host.");
    }

    // Cuando el server se detiene (host hizo Shutdown). En el host fuerza la
    // limpieza incluso si UiInGame no lo hizo (ej: cierre desde otra vía).
    private void OnServerStopped(bool wasHost)
    {
        // Solo actuamos si NO estamos ya yendo al menú.
        // Si el host pulsó "Volver al menú" en UiInGame, ya se está cargando.
        if (SceneManager.GetActiveScene().name == menuSceneName) return;

        // El host puede limpiar su propio regreso desde UiInGame, pero si por
        // algún motivo nos quedamos sin server sin pasar por ese flujo,
        // garantizamos volver al menú.
        ReturnToMenuFromDisconnect(null);
    }

    private void ReturnToMenuFromDisconnect(string reason)
    {
        if (!string.IsNullOrEmpty(reason))
            Debug.Log($"[NetworkConnectionManager] {reason}");

        // Asegurar que Netcode esté limpio
        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
        {
            NetworkManager.Singleton.Shutdown();
        }

        _discovery?.Stop();

        // Limpiar el mapeo de slots para que la próxima partida empiece limpio.
        NetworkPlayerSpawner.ResetSlots();

        SceneManager.LoadScene(menuSceneName);
    }

    // ────────────────────────────────────────────────
    //  BOTÓN HOST: crea la sala y empieza a anunciarse en la LAN
    // ────────────────────────────────────────────────
    public void CrearSala()
    {
        if (statusText) statusText.text = "Creando sala...";

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // 0.0.0.0 = escuchar en todas las interfaces de red disponibles.
        transport.SetConnectionData("0.0.0.0", gamePort, "0.0.0.0");

        bool started = NetworkManager.Singleton.StartHost();
        if (!started)
        {
            if (statusText) statusText.text = "Error al iniciar Host";
            return;
        }

        // Anunciarse en la LAN para que los clientes nos encuentren
        _discovery.StartBroadcasting(gamePort);

        if (menuPanel) menuPanel.SetActive(false);
        if (statusText) statusText.text = "Esperando jugadores...";

        Debug.Log("Host iniciado, anunciando en la LAN.");

        // Cargar Lobby usando el SceneManager de Netcode
        // para que los clientes que se unan vayan automáticamente al Lobby.
        NetworkManager.Singleton.SceneManager.LoadScene(
            lobbySceneName,
            LoadSceneMode.Single);
    }

    // ────────────────────────────────────────────────
    //  BOTÓN CLIENT: busca un host en la LAN y se conecta
    // ────────────────────────────────────────────────
    public void UnirseASala()
    {
        if (statusText) statusText.text = "Buscando partida en la LAN...";

        // Cuando el discovery encuentre un host, conectarse a esa IP
        _discovery.OnHostFound = (ip, port) =>
        {
            if (statusText) statusText.text = $"Host encontrado ({ip}). Conectando...";

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(ip, port);

            bool started = NetworkManager.Singleton.StartClient();
            if (!started)
            {
                if (statusText) statusText.text = "Error al conectar";
                return;
            }

            if (menuPanel) menuPanel.SetActive(false);
            // No hace falta cargar la escena manualmente:
            // Netcode sincroniza al cliente con la escena del host.
        };

        _discovery.StartListening();
    }

    // Botón para cancelar la búsqueda (opcional)
    public void CancelarBusqueda()
    {
        _discovery.Stop();
        _discovery.OnHostFound = null;
        if (statusText) statusText.text = "Búsqueda cancelada";
    }
}

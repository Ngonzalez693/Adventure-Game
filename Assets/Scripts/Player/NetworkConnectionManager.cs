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

    private LanDiscovery _discovery;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _discovery = GetComponent<LanDiscovery>();
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

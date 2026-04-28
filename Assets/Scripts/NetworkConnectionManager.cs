using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;

public class NetworkConnectionManager : MonoBehaviour
{
    public static NetworkConnectionManager Instance;

    [Header("UI")]
    public GameObject menuPanel;
    public TMP_InputField joinCodeInput;
    public TextMeshProUGUI joinCodeDisplay;
    public TextMeshProUGUI statusText;

    private string joinCode;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    async void Start()
    {
        await InitializeServices();
    }

    async Task InitializeServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            statusText.text = "Conectado a los servicios ✓";
            Debug.Log("Autenticado: " + AuthenticationService.Instance.PlayerId);
        }
        catch (System.Exception e)
        {
            statusText.text = "Error al conectar";
            Debug.LogError("Error inicializando servicios: " + e.Message);
        }
    }

    // El jugador 1 crea la sala
    public async void CrearSala()
    {
        try
        {
            statusText.text = "Creando sala...";

            // Máximo 4 jugadores
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Mostrar código para compartir
            joinCodeDisplay.text = $"Código: {joinCode}";
            statusText.text = "¡Sala creada! Comparte el código";

            // Configurar transporte
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "udp"));

            NetworkManager.Singleton.StartHost();
            menuPanel.SetActive(false);

            Debug.Log("Host iniciado con código: " + joinCode);
        }
        catch (System.Exception e)
        {
            statusText.text = "Error creando sala";
            Debug.LogError(e.Message);
        }
    }

    // Los demás jugadores se unen con el código
    public async void UnirseASala()
    {
        try
        {
            string codigo = joinCodeInput.text.Trim().ToUpper();
            statusText.text = "Uniéndose a la sala...";

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(codigo);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(joinAllocation, "udp"));

            NetworkManager.Singleton.StartClient();
            menuPanel.SetActive(false);

            statusText.text = "¡Conectado!";
        }
        catch (System.Exception e)
        {
            statusText.text = "Código incorrecto";
            Debug.LogError(e.Message);
        }
    }
}
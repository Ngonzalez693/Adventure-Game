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
using UnityEngine.UI;

public class OnlineManager : MonoBehaviour
{
    public static OnlineManager Instance;

    [Header("UI")]
    public GameObject menuPanel;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI codigoText;
    public TMP_InputField codigoInput;
    public Button btnCrear;
    public Button btnUnirse;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    async void Start()
    {
        statusText.text = "Conectando a servicios...";
        btnCrear.interactable = false;
        btnUnirse.interactable = false;

        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            statusText.text = "✓ Listo";
            btnCrear.interactable = true;
            btnUnirse.interactable = true;
        }
        catch (System.Exception e)
        {
            statusText.text = "✗ Error de conexión";
            Debug.LogError(e.Message);
        }
    }

    public async void CrearSala()
    {
        btnCrear.interactable = false;
        statusText.text = "Creando sala...";

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string codigo = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "udp"));

            NetworkManager.Singleton.StartHost();

            codigoText.text = $"Código: {codigo}";
            statusText.text = "✓ Sala creada, comparte el código";
            menuPanel.SetActive(false);
        }
        catch (System.Exception e)
        {
            statusText.text = "✗ Error creando sala";
            btnCrear.interactable = true;
            Debug.LogError(e.Message);
        }
    }

    public async void UnirseASala()
    {
        string codigo = codigoInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(codigo))
        {
            statusText.text = "Introduce el código";
            return;
        }

        btnUnirse.interactable = false;
        statusText.text = "Uniéndose...";

        try
        {
            JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(codigo);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(join, "udp"));

            NetworkManager.Singleton.StartClient();

            statusText.text = "✓ Conectado";
            menuPanel.SetActive(false);
        }
        catch (System.Exception e)
        {
            statusText.text = "✗ Código incorrecto";
            btnUnirse.interactable = true;
            Debug.LogError(e.Message);
        }
    }
}
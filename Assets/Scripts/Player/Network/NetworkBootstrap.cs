using Unity.Netcode;
using UnityEngine;

// Este script se encarga de configurar el servidor cuando el juego inicia.
// Controla quién puede conectarse al servidor y cuántos jugadores son permitidos.
public class NetworkBootstrap : MonoBehaviour
{
    // Número máximo de jugadores permitidos en la partida
    // Puede configurarse desde el Inspector de Unity
    [SerializeField] private int maxPlayers = 4;

    // Start se ejecuta cuando la escena ya está cargada
    private void Start()
    {
        // Verifica que exista un NetworkManager en la escena
        // El NetworkManager es el sistema central de Netcode
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not present in the scene.");
            return;
        }

        // Solo registramos el callback si Connection Approval está activo
        // en el NetworkManager. Si no, Netcode emite un warning al iniciar.
        if (NetworkManager.Singleton.NetworkConfig.ConnectionApproval)
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    // Este método se ejecuta cuando un cliente intenta conectarse
    // Permite aceptar o rechazar la conexión
    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        // Si el número de jugadores conectados supera el máximo permitido
        if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            // Rechazamos la conexión
            response.Approved = false;

            // Mensaje que recibirá el cliente
            response.Reason = "Server Full";

            return;
        }

        // Si todavía hay espacio en el servidor, aceptamos la conexión
        response.Approved = true;

        // Esto indica que Netcode debe crear automáticamente
        // el prefab del jugador para el cliente que se conectó
        response.CreatePlayerObject = true;
    }
}
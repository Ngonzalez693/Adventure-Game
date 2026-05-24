using Unity.Netcode;
using UnityEngine;

// Objeto interactuable en el Lobby que inicia la partida.
// Solo responde si:
//   - El que interactúa es el HOST
//   - El lobby está lleno (PlayerCount >= RequiredPlayers)
//   - La partida no está iniciando ya
//
// Requisitos en la escena:
//   - El GameObject debe tener un Collider (puede ser trigger o no).
//   - PlayerInteraction debe poder encontrarlo via Physics.OverlapSphere.
public class StartGameInteractable : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [Tooltip("Nombre que se muestra en el prompt de interacción.")]
    public string objectName = "Consola de inicio";

    public void Interact()
    {
        var lobby = LobbyManager.Instance;
        if (lobby == null)
        {
            Debug.LogWarning("[StartGameInteractable] LobbyManager no encontrado.");
            return;
        }

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[StartGameInteractable] Solo el host puede iniciar.");
            return;
        }

        if (!lobby.IsLobbyFull)
        {
            int faltan = lobby.RequiredPlayers - lobby.PlayerCount.Value;
            string msg = faltan == 1 ? "Falta 1 jugador." : $"Faltan {faltan} jugadores.";
            Debug.Log($"[StartGameInteractable] {msg}");
            return;
        }

        if (lobby.GameStarting.Value) return;

        lobby.StartGameServerRpc();
    }

    public string GetPromptText()
    {
        var lobby = LobbyManager.Instance;
        if (lobby == null) return objectName;

        bool isHost = NetworkManager.Singleton != null &&
                      NetworkManager.Singleton.IsHost;

        if (lobby.GameStarting.Value)
            return "Iniciando...";

        if (!isHost)
            return $"{objectName} (solo el host puede iniciar)";

        if (!lobby.IsLobbyFull)
        {
            int faltan = lobby.RequiredPlayers - lobby.PlayerCount.Value;
            string plural = faltan == 1 ? "falta 1 jugador" : $"faltan {faltan} jugadores";
            return $"{objectName} ({plural})";
        }

        return "Iniciar partida";
    }
}

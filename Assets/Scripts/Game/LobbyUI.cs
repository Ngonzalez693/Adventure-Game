using TMPro;
using Unity.Netcode;
using UnityEngine;

// Actualiza los textos del Canvas del Lobby según el estado de LobbyManager.
// Pon este script en el Canvas (o en cualquier GameObject de la escena Lobby)
// y arrastra los TextMeshPro correspondientes en el Inspector.
public class LobbyUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Texto que muestra el conteo de jugadores (ej: '2/4').")]
    public TextMeshProUGUI numberPlayerText;

    [Tooltip("Texto de estado del lobby ('Esperando...', '¡Listo!', etc.).")]
    public TextMeshProUGUI waitingText;

    [Header("Textos de estado (puedes personalizar)")]
    public string waitingMessage   = "Esperando jugadores...";
    public string readyMessageHost = "¡Todos listos! Interactúa con la consola.";
    public string readyMessageClient = "¡Todos listos! Esperando al host.";
    public string startingMessage  = "¡Iniciando partida!";

    private void Update()
    {
        var lobby = LobbyManager.Instance;
        if (lobby == null || !lobby.IsSpawned) return;

        int count    = lobby.PlayerCount.Value;
        int required = lobby.RequiredPlayers;

        // ── Conteo "X/4" ──────────────────────────────────────────────────
        if (numberPlayerText != null)
            numberPlayerText.text = $"{count} / {required}";

        // ── Texto de estado ──────────────────────────────────────────────
        if (waitingText != null)
        {
            if (lobby.GameStarting.Value)
            {
                waitingText.text = startingMessage;
            }
            else if (lobby.IsLobbyFull)
            {
                // Mensaje distinto para host (puede iniciar) y cliente (debe esperar)
                bool isHost = NetworkManager.Singleton != null &&
                              NetworkManager.Singleton.IsHost;
                waitingText.text = isHost ? readyMessageHost : readyMessageClient;
            }
            else
            {
                waitingText.text = waitingMessage;
            }
        }
    }
}

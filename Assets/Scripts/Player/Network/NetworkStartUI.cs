using UnityEngine;
using Unity.Netcode;

namespace Net
{
    // Este script crea una interfaz simple usando OnGUI
    // para iniciar el sistema de red como Host, Client o Server.
    public class NetworkStartUI : MonoBehaviour
    {
        // Ancho del botón en la interfaz
        private const float ButtonWidth = 200f;

        // Alto del botón
        private const float ButtonHeight = 40f;

        // OnGUI se ejecuta cada frame para dibujar elementos GUI
        private void OnGUI()
        {
            // Verifica que exista un NetworkManager en la escena
            if (NetworkManager.Singleton == null)
            {
                // Si no existe mostramos un mensaje de error en pantalla
                GUI.Label(new Rect(10, 10, 300, 25), "NetworkManager not found in scene.");
                return;
            }

            // Posición inicial de los botones en pantalla
            float x = 10f;
            float y = 10f;

            // Si aún no se inició la red
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                // Botón para iniciar el juego como HOST
                // Host = Cliente + Servidor al mismo tiempo
                if (GUI.Button(new Rect(x, y, ButtonWidth, ButtonHeight), "Start Host"))
                {
                    bool result = NetworkManager.Singleton.StartHost();

                    // Si falla mostramos error en consola
                    if (!result)
                        Debug.LogError("Failed to start Host.");
                }

                // Botón para iniciar como CLIENTE
                if (GUI.Button(new Rect(x, y + ButtonHeight + 5, ButtonWidth, ButtonHeight), "Start Client"))
                {
                    bool result = NetworkManager.Singleton.StartClient();

                    if (!result)
                        Debug.LogError("Failed to start Client.");
                }

                // Botón para iniciar solo como SERVIDOR
                // No controla jugador, solo ejecuta la simulación
                if (GUI.Button(new Rect(x, y + (ButtonHeight + 5) * 2, ButtonWidth, ButtonHeight), "Start Server"))
                {
                    bool result = NetworkManager.Singleton.StartServer();

                    if (!result)
                        Debug.LogError("Failed to start Server.");
                }
            }
            else
            {
                // Si la red ya está activa mostramos el modo actual

                string mode = NetworkManager.Singleton.IsHost ? "HOST"
                            : NetworkManager.Singleton.IsServer ? "SERVER"
                            : "CLIENT";

                // Texto que muestra el modo actual
                GUI.Label(new Rect(x, y, 300, 25), $"Running as: {mode}");

                // Botón para cerrar la conexión de red
                if (GUI.Button(new Rect(x, y + 30, ButtonWidth, ButtonHeight), "Shutdown"))
                {
                    NetworkManager.Singleton.Shutdown();
                }
            }
        }
    }
}
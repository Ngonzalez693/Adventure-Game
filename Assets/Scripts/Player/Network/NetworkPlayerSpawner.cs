using Unity.Netcode;
using UnityEngine;

// Este script se encarga de posicionar a cada jugador
// en un punto de spawn cuando se conecta al servidor.
public class NetworkPlayerSpawner : NetworkBehaviour
{
    // Lista de puntos de spawn definidos en la escena
    // Se asignan desde el Inspector de Unity
    [SerializeField] private Transform[] spawnPoints;

    // Índice que indica cuál spawn usar
    private int spawnIndex;

    // Este método se ejecuta cuando el objeto de red
    // aparece en la red (cuando el servidor inicia)
    public override void OnNetworkSpawn()
    {
        // Solo el servidor debe controlar el spawn de jugadores
        if (!IsServer)
            return;

        // Registramos una función que se ejecutará
        // cada vez que un cliente se conecte
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }

    // Este método se ejecuta cuando un nuevo cliente se conecta
    private void SpawnPlayer(ulong clientId)
    {
        // Verifica que existan puntos de spawn configurados
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn points missing.");
            return;
        }

        // Selecciona el spawn correspondiente
        Transform spawn = spawnPoints[spawnIndex];

        // Obtiene el objeto del jugador que Netcode creó automáticamente
        NetworkObject playerObject =
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        // Mueve el jugador a la posición del spawn
        playerObject.transform.position = spawn.position;

        // Ajusta también la rotación del jugador
        playerObject.transform.rotation = spawn.rotation;

        // Avanza al siguiente spawn
        // El operador % hace que vuelva al inicio si se acaban los spawns
        spawnIndex = (spawnIndex + 1) % spawnPoints.Length;
    }
}

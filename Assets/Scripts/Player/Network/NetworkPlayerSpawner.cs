using Unity.Netcode;
using UnityEngine;

// Administra los puntos de spawn de la escena.
// Los jugadores se auto-posicionan llamando GetNextSpawnPoint()
// desde su propio OnNetworkSpawn, evitando problemas de timing
// con callbacks que disparan antes de que PlayerObject esté listo.
public class NetworkPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private int _spawnIndex;

    // Singleton simple para que el prefab del jugador pueda encontrarlo
    public static NetworkPlayerSpawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Devuelve el siguiente spawn point disponible (solo llamar desde el servidor)
    public Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[NetworkPlayerSpawner] No hay spawn points asignados.");
            return null;
        }

        Transform point = spawnPoints[_spawnIndex];
        _spawnIndex = (_spawnIndex + 1) % spawnPoints.Length;
        return point;
    }
}

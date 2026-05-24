using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Player; // NetworkCharacterMotor

// Administra los puntos de spawn de la escena.
// Cada jugador es asignado a un slot determinístico según el orden en que
// se conectaron al host (host = slot 0, primer cliente = slot 1, etc.).
// Así, el mismo jugador siempre cae en el mismo spawn point al cambiar
// de escena (Lobby → GameMap).
public class NetworkPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    public int SpawnPointCount => spawnPoints != null ? spawnPoints.Length : 0;

    // Singleton para que NetworkCharacterMotor pueda pedir su spawn point
    public static NetworkPlayerSpawner Instance { get; private set; }

    // Mapeo persistente clientId → slot. Se asigna en el servidor cuando un
    // cliente se conecta por primera vez y se mantiene aunque cambien las
    // escenas.
    private static readonly Dictionary<ulong, int> _slotByClientId = new();
    private static int _nextSlot;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Cuando se carga GameMap, este spawner es nuevo, pero los
            // jugadores ya tienen sus slots asignados desde el Lobby.
            // Solo asignamos slot a los que aún no lo tengan.
            foreach (var id in NetworkManager.ConnectedClientsIds)
                EnsureSlotForClient(id);

            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;

            // Re-posicionar a TODOS los jugadores existentes en esta nueva escena.
            // Esto es necesario porque los player objects PERSISTEN entre escenas
            // (no se vuelven a crear), por lo que su OnNetworkSpawn no se dispara
            // de nuevo al cambiar de Lobby a GameMap.
            StartCoroutine(RepositionAllPlayersNextFrame());
        }
    }

    private IEnumerator RepositionAllPlayersNextFrame()
    {
        // Esperar un par de frames para que toda la escena (incluyendo posiciones
        // finales de los transforms) esté completamente lista.
        yield return null;
        yield return null;

        Debug.Log($"[NetworkPlayerSpawner] Reposicionando jugadores en escena '{gameObject.scene.name}'. " +
                  $"Clientes conectados: {NetworkManager.ConnectedClientsIds.Count}, " +
                  $"spawnPoints: {(spawnPoints?.Length ?? 0)}");

        foreach (var clientId in NetworkManager.ConnectedClientsIds)
        {
            if (!NetworkManager.ConnectedClients.TryGetValue(clientId, out var client))
            {
                Debug.LogWarning($"[NetworkPlayerSpawner] No se encontró client {clientId}.");
                continue;
            }

            var playerObj = client.PlayerObject;
            if (playerObj == null)
            {
                Debug.LogWarning($"[NetworkPlayerSpawner] PlayerObject null para client {clientId}.");
                continue;
            }

            var motor = playerObj.GetComponent<NetworkCharacterMotor>();
            if (motor == null)
            {
                Debug.LogWarning($"[NetworkPlayerSpawner] No NetworkCharacterMotor en player de client {clientId}.");
                continue;
            }

            Transform spawn = GetSpawnPointForClient(clientId);
            if (spawn == null)
            {
                Debug.LogWarning($"[NetworkPlayerSpawner] Spawn point null para client {clientId}.");
                continue;
            }

            Debug.Log($"[NetworkPlayerSpawner] Teleporting client {clientId} a slot " +
                      $"{GetSlotForClient(clientId)} → {spawn.name} @ {spawn.position}");

            motor.TeleportTo(spawn.position, spawn.rotation);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        if (Instance == this) Instance = null;
    }

    private void OnClientConnected(ulong clientId)    => EnsureSlotForClient(clientId);
    private void OnClientDisconnected(ulong clientId) => _slotByClientId.Remove(clientId);

    private void EnsureSlotForClient(ulong clientId)
    {
        if (_slotByClientId.ContainsKey(clientId)) return;
        _slotByClientId[clientId] = _nextSlot++;
    }

    // Devuelve el spawn point asignado a este clientId.
    // Solo debe llamarse desde el servidor.
    public Transform GetSpawnPointForClient(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[NetworkPlayerSpawner] No hay spawn points asignados.");
            return null;
        }

        EnsureSlotForClient(clientId);
        int slot = _slotByClientId[clientId] % spawnPoints.Length;
        return spawnPoints[slot];
    }

    // Devuelve el slot (0..N-1) del cliente. Útil para asignar puzzles, mapas,
    // visibilidad por capas, etc.
    public int GetSlotForClient(ulong clientId)
    {
        EnsureSlotForClient(clientId);
        return _slotByClientId[clientId];
    }

    // Reset estático del mapeo de slots. Llamar al volver al menú si quieres
    // que en una sesión nueva los slots empiecen desde 0 otra vez.
    public static void ResetSlots()
    {
        _slotByClientId.Clear();
        _nextSlot = 0;
    }
}

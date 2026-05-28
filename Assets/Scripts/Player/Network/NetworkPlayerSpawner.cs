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

    // Mapeo persistente clientId → slot (SOLO SERVIDOR). Se asigna cuando un
    // cliente se conecta por primera vez y se mantiene aunque cambien las
    // escenas.
    private static readonly Dictionary<ulong, int> _slotByClientId = new();

    // Mapeo sincronizado: índice = slot, valor = clientId. Lo escribe el
    // servidor en OnNetworkSpawn y todos los clientes lo leen para saber qué
    // slot tiene cada jugador.
    private NetworkList<ulong> _clientBySlot;

    private void Awake()
    {
        Instance = this;
        _clientBySlot = new NetworkList<ulong>();
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

            // Publicar el mapeo a todos los clientes
            SyncSlotsToNetwork();

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

    private void OnClientConnected(ulong clientId)
    {
        EnsureSlotForClient(clientId);
        SyncSlotsToNetwork();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        _slotByClientId.Remove(clientId);
        SyncSlotsToNetwork();
    }

    private void EnsureSlotForClient(ulong clientId)
    {
        if (_slotByClientId.ContainsKey(clientId)) return;

        // Buscar el slot LIBRE más bajo (0..maxSlots-1). De este modo, cuando
        // un jugador se desconecta y otro se une, el nuevo reusa el slot
        // liberado en lugar de obtener un índice creciente que colisionaría
        // con el módulo de spawnPoints.Length.
        int maxSlots = spawnPoints != null && spawnPoints.Length > 0 ? spawnPoints.Length : 4;
        var usedSlots = new HashSet<int>(_slotByClientId.Values);

        int slot = 0;
        while (slot < maxSlots && usedSlots.Contains(slot)) slot++;

        if (slot >= maxSlots)
        {
            Debug.LogWarning($"[NetworkPlayerSpawner] Todos los {maxSlots} slots ocupados; " +
                             $"cliente {clientId} comparte slot por módulo.");
            slot = _slotByClientId.Count % maxSlots;
        }

        _slotByClientId[clientId] = slot;
    }

    // SOLO SERVIDOR. Refresca la NetworkList con el mapeo slot → clientId
    // para que todos los clientes lo puedan leer.
    private void SyncSlotsToNetwork()
    {
        if (!IsServer || _clientBySlot == null) return;

        // Ordenar por slot ascendente y construir la lista
        var ordered = new List<KeyValuePair<ulong, int>>(_slotByClientId);
        ordered.Sort((a, b) => a.Value.CompareTo(b.Value));

        _clientBySlot.Clear();
        foreach (var kvp in ordered)
            _clientBySlot.Add(kvp.Key);
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

    // Devuelve el slot (0..N-1) del cliente. Funciona tanto en servidor como
    // en clientes (los clientes usan la NetworkList sincronizada; el servidor
    // tiene la información autoritativa en el dict estático).
    public int GetSlotForClient(ulong clientId)
    {
        if (IsServer)
        {
            EnsureSlotForClient(clientId);
            return _slotByClientId[clientId];
        }

        // Cliente: buscar en la lista sincronizada por la red
        if (_clientBySlot != null)
        {
            for (int i = 0; i < _clientBySlot.Count; i++)
                if (_clientBySlot[i] == clientId) return i;
        }
        return -1; // todavía no se sincronizó
    }

    // Reset estático del mapeo de slots. Llamar al volver al menú si quieres
    // que en una sesión nueva los slots empiecen desde 0 otra vez.
    public static void ResetSlots()
    {
        _slotByClientId.Clear();
    }
}

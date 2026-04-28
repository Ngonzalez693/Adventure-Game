// PuzzleCodeManager.cs
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
using TMPro;

public class PuzzleCodeManager : NetworkBehaviour
{
    public static PuzzleCodeManager Instance;

    // Códigos sincronizados en red - cada índice es un jugador
    private NetworkList<int> codigosJugadores;

    [Header("UI")]
    public TextMeshProUGUI miCodigoText;  // El código que yo le doy al otro
    public TextMeshProUGUI statusText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        codigosJugadores = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            // El host genera todos los códigos
            GenerarCodigos();
        }

        // Suscribirse a cambios en la lista
        codigosJugadores.OnListChanged += OnCodigosActualizados;
        MostrarMiCodigo();
    }

    void GenerarCodigos()
    {
        // Generar un código de 4 dígitos por jugador
        // Código 1234 por defecto para pruebas, luego aleatorio
        codigosJugadores.Clear();

        // Por ahora generamos 2 códigos (para 2 jugadores)
        for (int i = 0; i < 4; i++)
            codigosJugadores.Add(Random.Range(0, 10));  // Jugador 1

        for (int i = 0; i < 4; i++)
            codigosJugadores.Add(Random.Range(0, 10));  // Jugador 2
    }

    void OnCodigosActualizados(NetworkListEvent<int> changeEvent)
    {
        MostrarMiCodigo();
    }

    void MostrarMiCodigo()
    {
        if (codigosJugadores.Count < 4) return;

        // Obtener mi índice de jugador
        int miIndice = (int)NetworkManager.Singleton.LocalClientId;

        // Mi código = los 4 dígitos que empiezan en mi índice * 4
        int inicio = miIndice * 4;
        if (inicio + 3 >= codigosJugadores.Count) return;

        string miCodigo = "";
        for (int i = inicio; i < inicio + 4; i++)
            miCodigo += codigosJugadores[i].ToString();

        // Mostrar MI código en pantalla para dárselo al otro jugador
        if (miCodigoText != null)
            miCodigoText.text = $"Tu código: {miCodigo}";

        Debug.Log($"Mi código (para dar al otro): {miCodigo}");
    }

    // Obtener el código del OTRO jugador (el que yo debo resolver)
    public int[] ObtenerCodigoAResolver()
    {
        int miIndice = (int)NetworkManager.Singleton.LocalClientId;

        // El código a resolver es el del otro jugador
        // Si soy jugador 0, resuelvo el código del jugador 1 y viceversa
        int otroIndice = miIndice == 0 ? 1 : 0;
        int inicio = otroIndice * 4;

        if (inicio + 3 >= codigosJugadores.Count)
        {
            Debug.LogWarning("Códigos no generados aún");
            return new int[] { 1, 2, 3, 4 }; // Fallback
        }

        int[] codigo = new int[4];
        for (int i = 0; i < 4; i++)
            codigo[i] = codigosJugadores[inicio + i];

        return codigo;
    }

    // Notificar a todos que un jugador completó su puzzle
    [ServerRpc(RequireOwnership = false)]
    public void PuzzleCompletadoServerRpc(ulong clientId)
    {
        PuzzleCompletadoClientRpc(clientId);
    }

    [ClientRpc]
    void PuzzleCompletadoClientRpc(ulong clientId)
    {
        Debug.Log($"Jugador {clientId} completó su puzzle");
        // Aquí puedes agregar lógica de victoria
    }
}
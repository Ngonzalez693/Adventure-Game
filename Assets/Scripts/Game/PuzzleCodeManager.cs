using Unity.Netcode;
using UnityEngine;

// Genera y sincroniza el rompecabezas de código de colores.
// - El host genera 4 colores en un orden aleatorio (la "clave" que ve el
//   jugador A en su nota de orden).
// - El host asigna un dígito (0-9) a cada color (las "pistas" que aparecen
//   en las notas con números coloreados distribuidas por los otros mapas).
// - El código correcto = orden de colores × dígito asignado a cada color.
//
// Ejemplo:
//   colorOrder    = [Rojo, Azul, Verde, Amarillo]
//   digitsByColor = { Rojo:7, Azul:3, Verde:9, Amarillo:1 }
//   código correcto = 7391
public class PuzzleCodeManager : NetworkBehaviour
{
    public static PuzzleCodeManager Instance;

    // Los 4 colores que usaremos. Si quieres cambiar el set, edita esto.
    public enum PuzzleColor { Rojo, Azul, Verde, Amarillo }

    // Cantidad de colores/dígitos. Para 4 dígitos, debe ser 4.
    public const int DigitCount = 4;

    // ── Estado sincronizado ──────────────────────────────────────────────
    // Orden de colores (índices 0..3 corresponden a los 4 colores).
    // Se sincroniza como cuatro NetworkVariables sencillos para simplicidad.
    private NetworkVariable<int> _colorOrder0 = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _colorOrder1 = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _colorOrder2 = new(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _colorOrder3 = new(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Dígito asignado a cada color (índice = (int)PuzzleColor).
    private NetworkVariable<int> _digitRojo     = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _digitAzul     = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _digitVerde    = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _digitAmarillo = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Estado: si el puzzle ya fue resuelto.
    public NetworkVariable<bool> IsSolved = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            GenerarRompecabezas();
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
    }

    // ────────────────────────────────────────────────
    //  Generación (solo servidor)
    // ────────────────────────────────────────────────
    private void GenerarRompecabezas()
    {
        // 1) Generar un orden aleatorio de colores (shuffle de [0,1,2,3])
        int[] order = { 0, 1, 2, 3 };
        for (int i = order.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        _colorOrder0.Value = order[0];
        _colorOrder1.Value = order[1];
        _colorOrder2.Value = order[2];
        _colorOrder3.Value = order[3];

        // 2) Asignar un dígito aleatorio a cada color (0-9)
        _digitRojo.Value     = Random.Range(0, 10);
        _digitAzul.Value     = Random.Range(0, 10);
        _digitVerde.Value    = Random.Range(0, 10);
        _digitAmarillo.Value = Random.Range(0, 10);

        Debug.Log($"[PuzzleCodeManager] Código generado: " +
                  $"{GetColorOrderString()} → {string.Join("", GetCorrectCode())}");
    }

    // ────────────────────────────────────────────────
    //  Consultas públicas (cualquier cliente)
    // ────────────────────────────────────────────────
    public PuzzleColor[] GetColorOrder()
    {
        return new[]
        {
            (PuzzleColor)_colorOrder0.Value,
            (PuzzleColor)_colorOrder1.Value,
            (PuzzleColor)_colorOrder2.Value,
            (PuzzleColor)_colorOrder3.Value,
        };
    }

    public int GetDigitForColor(PuzzleColor color)
    {
        return color switch
        {
            PuzzleColor.Rojo     => _digitRojo.Value,
            PuzzleColor.Azul     => _digitAzul.Value,
            PuzzleColor.Verde    => _digitVerde.Value,
            PuzzleColor.Amarillo => _digitAmarillo.Value,
            _ => 0
        };
    }

    public int[] GetCorrectCode()
    {
        var order = GetColorOrder();
        int[] code = new int[DigitCount];
        for (int i = 0; i < DigitCount; i++)
            code[i] = GetDigitForColor(order[i]);
        return code;
    }

    // Devuelve el color como Unity Color (para teñir texto en UI).
    public static Color ToUnityColor(PuzzleColor c) => c switch
    {
        PuzzleColor.Rojo     => new Color(1f, 0.2f, 0.2f),
        PuzzleColor.Azul     => new Color(0.3f, 0.5f, 1f),
        PuzzleColor.Verde    => new Color(0.3f, 1f, 0.3f),
        PuzzleColor.Amarillo => new Color(1f, 0.9f, 0.2f),
        _ => Color.white,
    };

    public string GetColorOrderString()
    {
        var order = GetColorOrder();
        return string.Join(" → ", order);
    }

    // ────────────────────────────────────────────────
    //  Marcar el puzzle como resuelto
    // ────────────────────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    public void MarcarResueltoServerRpc()
    {
        if (!IsSolved.Value) IsSolved.Value = true;
        Debug.Log("[PuzzleCodeManager] Puzzle de código RESUELTO.");
    }
}

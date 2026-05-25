using Unity.Netcode;
using UnityEngine;

// Puzzle de memoria (estilo Simon Says cooperativo):
//  - El host genera 3 SECUENCIAS de colores aleatorias (3 colores cada una).
//  - Cada secuencia se asigna a una "posición" (1, 2, 3).
//  - PuzzleAssignmentManager asigna esas posiciones a los 3 slots ayudantes.
//  - El SOLUCIONADOR tiene 3 botones de colores (Rojo, Verde, Azul).
//  - Para resolver: el solucionador debe pulsar la secuencia 1 completa,
//    luego la 2, luego la 3 (9 pulsaciones en total, en orden).
//  - Una pulsación incorrecta resetea el progreso a 0.
//  - El feedback es la NetworkVariable Progress (0..9) que todos pueden leer.
//
// Códigos de color: 0 = Rojo, 1 = Verde, 2 = Azul
public class MemoryPuzzleManager : NetworkBehaviour
{
    public static MemoryPuzzleManager Instance;

    public const int SequenceCount  = 3; // 3 secuencias = 3 ayudantes
    public const int LengthPerSeq   = 3; // 3 colores por secuencia
    public const int TotalSteps     = SequenceCount * LengthPerSeq; // 9
    public const int ColorCount     = 3; // R, G, B

    // Lista plana con las 9 entradas de las 3 secuencias.
    // _sequences[posición * LengthPerSeq + index] = color
    private NetworkList<int> _sequences;

    public NetworkVariable<int> Progress = new(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsSolved = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        Instance = this;
        _sequences = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            GenerarSecuencias();
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
    }

    private void GenerarSecuencias()
    {
        _sequences.Clear();
        for (int i = 0; i < TotalSteps; i++)
            _sequences.Add(Random.Range(0, ColorCount));

        // Log las 3 secuencias para depuración
        for (int pos = 0; pos < SequenceCount; pos++)
        {
            string s = "";
            for (int i = 0; i < LengthPerSeq; i++)
                s += ColorName(_sequences[pos * LengthPerSeq + i]) + " ";
            Debug.Log($"[MemoryPuzzleManager] Secuencia {pos + 1}: {s.Trim()}");
        }
    }

    // Devuelve la secuencia (3 colores) para una posición (0, 1 o 2).
    public int[] GetSequenceForPosition(int position)
    {
        if (_sequences == null || _sequences.Count < TotalSteps)
            return new int[LengthPerSeq];

        var seq = new int[LengthPerSeq];
        for (int i = 0; i < LengthPerSeq; i++)
            seq[i] = _sequences[position * LengthPerSeq + i];
        return seq;
    }

    public int GetExpectedColor(int step)
    {
        if (_sequences == null || step < 0 || step >= _sequences.Count) return -1;
        return _sequences[step];
    }

    // RPC: el solucionador pulsa un botón de color.
    [ServerRpc(RequireOwnership = false)]
    public void PressColorServerRpc(int color)
    {
        if (IsSolved.Value) return;

        int expected = GetExpectedColor(Progress.Value);
        if (color == expected)
        {
            Progress.Value++;
            if (Progress.Value >= TotalSteps)
            {
                IsSolved.Value = true;
                Debug.Log("[MemoryPuzzleManager] Puzzle de memoria RESUELTO.");
            }
        }
        else
        {
            // Error: reset
            Debug.Log($"[MemoryPuzzleManager] Color incorrecto en paso {Progress.Value}. " +
                      $"Esperado={ColorName(expected)}, recibido={ColorName(color)}. Reset.");
            Progress.Value = 0;
        }
    }

    public static string ColorName(int color) => color switch
    {
        0 => "Rojo", 1 => "Verde", 2 => "Azul", _ => "?"
    };

    public static Color ToUnityColor(int color) => color switch
    {
        0 => new Color(1f, 0.3f, 0.3f),
        1 => new Color(0.3f, 1f, 0.3f),
        2 => new Color(0.3f, 0.5f, 1f),
        _ => Color.white,
    };

    public static string ToHexColor(int color)
    {
        return ColorUtility.ToHtmlStringRGB(ToUnityColor(color));
    }
}

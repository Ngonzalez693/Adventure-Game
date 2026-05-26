using System.Collections;
using UnityEngine;

// Consola del ayudante para ver su secuencia. En lugar de abrir un panel
// de UI, el propio MESH del objeto cambia de color para mostrar la secuencia:
//
//   default → color 1 → default → color 2 → default → color 3 → default
//
// El número de POSICIÓN se muestra en el prompt de interacción
// ("[E] Ver secuencia (posición 2)") para que el jugador sepa cuál es.
//
// Mientras la animación está corriendo, el objeto no es re-interactuable.
public class MemorySequenceViewer : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    public string objectName = "Consola de secuencia";

    [Tooltip("Slot del mapa donde está este objeto (0..3).")]
    public int mapSlot;

    [Header("Visual")]
    [Tooltip("Renderer del mesh que cambia de color. Si está vacío usa el de este GameObject.")]
    public Renderer paintRenderer;

    [Tooltip("Color por defecto (entre flashes y al iniciar). Si lo dejas en " +
             "negro puro, se usará el color actual del material al iniciar.")]
    public Color defaultColor = new Color(0.4f, 0.4f, 0.4f);

    [Header("Tiempos (segundos)")]
    [Tooltip("Cuánto dura cada color encendido.")]
    public float colorDuration = 1f;

    [Tooltip("Pausa en gris entre cada color.")]
    public float gapDuration = 0.4f;

    private bool _playing;
    private Color _resolvedDefault;

    private void Awake()
    {
        if (paintRenderer == null) paintRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        // Si defaultColor es casi negro, asumimos que el usuario quería el
        // color actual del material — lo guardamos como default.
        _resolvedDefault = (defaultColor.r + defaultColor.g + defaultColor.b > 0.01f)
            ? defaultColor
            : (paintRenderer != null ? paintRenderer.material.color : Color.gray);

        ApplyColor(_resolvedDefault);
    }

    public void Interact()
    {
        if (_playing) return;

        var assign = PuzzleAssignmentManager.Instance;
        var puzzle = MemoryPuzzleManager.Instance;
        if (assign == null || puzzle == null)
        {
            Debug.LogWarning("[MemorySequenceViewer] Faltan managers en la escena.");
            return;
        }

        int myPos = GetMemoryPositionForSlot(mapSlot, assign);
        if (myPos < 0)
        {
            // Soy el solucionador → no tengo secuencia. Ignorar.
            return;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMenuButton();

        int[] seq = puzzle.GetSequenceForPosition(myPos);
        StartCoroutine(PlaySequenceCoroutine(seq));
    }

    private IEnumerator PlaySequenceCoroutine(int[] sequence)
    {
        _playing = true;

        // Empezar en gris
        ApplyColor(_resolvedDefault);
        yield return new WaitForSeconds(gapDuration);

        for (int i = 0; i < sequence.Length; i++)
        {
            // Mostrar color i
            ApplyColor(MemoryPuzzleManager.ToUnityColor(sequence[i]));
            yield return new WaitForSeconds(colorDuration);

            // Volver al gris entre fases
            ApplyColor(_resolvedDefault);
            yield return new WaitForSeconds(gapDuration);
        }

        // Asegurar gris al terminar
        ApplyColor(_resolvedDefault);
        _playing = false;
    }

    private void ApplyColor(Color c)
    {
        if (paintRenderer == null) return;

        // Cubrimos varios shaders: built-in usa _Color, URP/HDRP usan _BaseColor
        var mat = paintRenderer.material;
        mat.color = c;
        if (mat.HasProperty("_BaseColor"))     mat.SetColor("_BaseColor", c);
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", c * 0.6f);
    }

    public string GetPromptText()
    {
        var assign = PuzzleAssignmentManager.Instance;
        if (assign == null) return $"Usar {objectName}";

        int myPos = GetMemoryPositionForSlot(mapSlot, assign);
        if (myPos < 0) return $"{objectName} (sin secuencia)";

        if (_playing) return "Reproduciendo...";

        return $"Ver secuencia (posición {myPos + 1})";
    }

    private static int GetMemoryPositionForSlot(int slot, PuzzleAssignmentManager mgr) => slot switch
    {
        0 => mgr.Slot0MemoryPos.Value,
        1 => mgr.Slot1MemoryPos.Value,
        2 => mgr.Slot2MemoryPos.Value,
        3 => mgr.Slot3MemoryPos.Value,
        _ => -1
    };
}

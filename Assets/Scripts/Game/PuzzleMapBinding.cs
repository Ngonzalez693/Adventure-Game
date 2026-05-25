using UnityEngine;

// Pon este script en cada _Map N. Activa/desactiva los objetos físicos del
// puzzle que solo deben existir según el rol asignado.
//
// Las cajas (MysteryBox) NO se manejan aquí — siempre están visibles.
public class PuzzleMapBinding : MonoBehaviour
{
    [Header("Slot de este mapa")]
    [Tooltip("Slot del jugador asociado a este mapa (0..3). Map1=0, Map2=1, etc.")]
    public int slotIndex;

    [Header("Puzzle de código")]
    [Tooltip("Consola del puzzle de código. Activa solo en el mapa del solucionador.")]
    public GameObject codeConsole;

    [Header("Puzzle de patrón")]
    [Tooltip("Panel de luces. Activo solo en el mapa del solucionador del patrón.")]
    public GameObject patternLightPanel;

    [Tooltip("Las 3 palancas binarias de este mapa. Tamaño esperado: 3.")]
    public GameObject[] levers;

    [Header("Puzzle de memoria")]
    [Tooltip("Panel con los 3 botones de colores + display de progreso. " +
             "Activo solo en el mapa del solucionador del puzzle de memoria.")]
    public GameObject memoryButtonPanel;

    [Tooltip("Consola que el ayudante usa para ver su secuencia. Activa en TODOS " +
             "los mapas EXCEPTO el del solucionador del memoria.")]
    public GameObject memorySequenceViewer;

    private void Start()
    {
        SetActiveSafe(codeConsole, false);
        SetActiveSafe(patternLightPanel, false);
        SetLeversActive(false);
        SetActiveSafe(memoryButtonPanel, false);
        SetActiveSafe(memorySequenceViewer, false);

        if (PuzzleAssignmentManager.Instance != null)
            ApplyAssignment();
    }

    public void ApplyAssignment()
    {
        var mgr = PuzzleAssignmentManager.Instance;
        if (mgr == null) return;

        // Puzzle de código
        bool isCodeSolver = mgr.CodePuzzleSlot.Value == slotIndex;
        SetActiveSafe(codeConsole, isCodeSolver);

        // Puzzle de patrón
        bool isPatternSolver = mgr.PatternPuzzleSlot.Value == slotIndex;
        SetActiveSafe(patternLightPanel, isPatternSolver);
        SetLeversActive(!isPatternSolver);

        // Puzzle de memoria
        bool isMemorySolver = mgr.MemoryPuzzleSlot.Value == slotIndex;
        SetActiveSafe(memoryButtonPanel, isMemorySolver);
        SetActiveSafe(memorySequenceViewer, !isMemorySolver);
    }

    private void SetLeversActive(bool active)
    {
        if (levers == null) return;
        foreach (var l in levers)
            SetActiveSafe(l, active);
    }

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}

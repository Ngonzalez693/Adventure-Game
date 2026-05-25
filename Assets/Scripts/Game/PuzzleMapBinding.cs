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
    [Tooltip("Panel de luces (3 esferas LightIndicator). Activo solo en el mapa " +
             "del solucionador del puzzle de patrón.")]
    public GameObject patternLightPanel;

    [Tooltip("Las 3 palancas binarias de este mapa. Activas para todos los slots " +
             "EXCEPTO el solucionador del patrón. Tamaño esperado: 3.")]
    public GameObject[] levers;

    private void Start()
    {
        SetActiveSafe(codeConsole, false);
        SetActiveSafe(patternLightPanel, false);
        SetLeversActive(false);

        if (PuzzleAssignmentManager.Instance != null)
            ApplyAssignment();
    }

    public void ApplyAssignment()
    {
        var mgr = PuzzleAssignmentManager.Instance;
        if (mgr == null) return;

        // Puzzle de código: solo la consola en el mapa del solucionador
        bool isCodeSolver = mgr.CodePuzzleSlot.Value == slotIndex;
        SetActiveSafe(codeConsole, isCodeSolver);

        // Puzzle de patrón:
        //  - El solucionador ve el panel de luces (no tiene palancas)
        //  - Los demás tienen 3 palancas (no tienen luces)
        bool isPatternSolver = mgr.PatternPuzzleSlot.Value == slotIndex;
        SetActiveSafe(patternLightPanel, isPatternSolver);
        SetLeversActive(!isPatternSolver);
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

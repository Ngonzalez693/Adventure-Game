using UnityEngine;

// Biblioteca central de iconos por rol. Pon una sola instancia en la escena
// (en Canvas, HUD o UIManager) y configura los 4 sprites de los roles.
// Después, los componentes PlayerRoleIcon leen de aquí.
public class RoleIconLibrary : MonoBehaviour
{
    public static RoleIconLibrary Instance;

    [Header("Iconos por rol")]
    [Tooltip("Icono del solucionador del puzzle de código.")]
    public Sprite codeIcon;

    [Tooltip("Icono del solucionador del puzzle de patrón (palancas + luces).")]
    public Sprite patternIcon;

    [Tooltip("Icono del solucionador del puzzle de memoria (3 botones).")]
    public Sprite memoryIcon;

    [Tooltip("Icono del solucionador del puzzle de presión (válvulas).")]
    public Sprite pressureIcon;

    [Header("Fallback")]
    [Tooltip("Icono cuando no hay asignación todavía o el slot está fuera de rango.")]
    public Sprite defaultIcon;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Devuelve el icono que corresponde al rol del slot dado.
    public Sprite GetIconForSlot(int slot)
    {
        var assign = PuzzleAssignmentManager.Instance;
        if (assign == null || slot < 0) return defaultIcon;

        if (assign.CodePuzzleSlot.Value     == slot) return codeIcon     != null ? codeIcon     : defaultIcon;
        if (assign.PatternPuzzleSlot.Value  == slot) return patternIcon  != null ? patternIcon  : defaultIcon;
        if (assign.MemoryPuzzleSlot.Value   == slot) return memoryIcon   != null ? memoryIcon   : defaultIcon;
        if (assign.PressurePuzzleSlot.Value == slot) return pressureIcon != null ? pressureIcon : defaultIcon;
        return defaultIcon;
    }
}

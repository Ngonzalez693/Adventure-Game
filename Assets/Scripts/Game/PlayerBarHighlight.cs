using TMPro;
using UnityEngine;

// Pon este script en cada barra "JUGADOR 1..4" del HUD. Cambia el color del
// texto del nombre cuando el jugador del slot correspondiente ha completado
// su rompecabezas.
public class PlayerBarHighlight : MonoBehaviour
{
    [Header("Referencia")]
    [Tooltip("Texto 'JUGADOR #' que cambia de color.")]
    public TextMeshProUGUI nameText;

    [Header("Slot que representa esta barra")]
    [Range(0, 3)] public int slot;

    [Header("Colores")]
    public Color colorPending  = Color.white;
    public Color colorComplete = new Color(0.3f, 1f, 0.3f);

    private void Update()
    {
        if (nameText == null) return;
        nameText.color = IsSlotPuzzleSolved(slot) ? colorComplete : colorPending;
    }

    private static bool IsSlotPuzzleSolved(int s)
    {
        var assign = PuzzleAssignmentManager.Instance;
        if (assign == null) return false;

        if (assign.CodePuzzleSlot.Value == s)
            return PuzzleCodeManager.Instance != null && PuzzleCodeManager.Instance.IsSolved.Value;
        if (assign.PatternPuzzleSlot.Value == s)
            return PatternPuzzleManager.Instance != null && PatternPuzzleManager.Instance.IsSolved.Value;
        if (assign.MemoryPuzzleSlot.Value == s)
            return MemoryPuzzleManager.Instance != null && MemoryPuzzleManager.Instance.IsSolved.Value;
        if (assign.PressurePuzzleSlot.Value == s)
            return PressurePuzzleManager.Instance != null && PressurePuzzleManager.Instance.IsSolved.Value;

        return false;
    }
}

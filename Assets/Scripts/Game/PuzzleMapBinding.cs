using UnityEngine;

// Pon este script en cada _Map N. Su única responsabilidad ahora es activar/
// desactivar la CONSOLA del puzzle de código, que solo existe físicamente en
// el mapa del jugador solucionador (al ser una pieza grande de "hardware").
//
// Las cajas (notas con orden de colores o números coloreados) NO se manejan
// aquí — siempre están visibles y deciden su contenido al interactuar
// (ver MysteryBox.cs).
public class PuzzleMapBinding : MonoBehaviour
{
    [Header("Slot de este mapa")]
    [Tooltip("Slot del jugador asociado a este mapa (0..3). Map1=0, Map2=1, etc.")]
    public int slotIndex;

    [Header("Consola del puzzle de código")]
    [Tooltip("El terminal donde el jugador ingresa el código. Solo se activa " +
             "en el mapa del slot elegido como solucionador.")]
    public GameObject codeConsole;

    private void Start()
    {
        // Por defecto la consola está apagada hasta que la asignación decida
        // quién la recibe.
        if (codeConsole != null) codeConsole.SetActive(false);

        if (PuzzleAssignmentManager.Instance != null)
            ApplyAssignment();
    }

    // Llamado por PuzzleAssignmentManager cuando cambia la asignación.
    public void ApplyAssignment()
    {
        var mgr = PuzzleAssignmentManager.Instance;
        if (mgr == null || codeConsole == null) return;

        bool isCodeSolver = mgr.CodePuzzleSlot.Value == slotIndex;
        if (codeConsole.activeSelf != isCodeSolver)
            codeConsole.SetActive(isCodeSolver);
    }
}

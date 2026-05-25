using TMPro;
using UnityEngine;

// Texto 3D que muestra el progreso del puzzle de memoria al solucionador.
// Pon este script en un GameObject con TextMeshPro (3D) cerca del panel
// de botones. Solo se ve en el mapa del solucionador.
public class MemoryProgressDisplay : MonoBehaviour
{
    [Header("Referencia (TextMeshPro 3D)")]
    public TextMeshPro progressText;

    [Header("Mensajes")]
    public string format        = "Progreso: {0}/{1}";
    public string solvedMessage = "¡SECUENCIA COMPLETA!";

    [Header("Colores")]
    public Color colorProgress = Color.white;
    public Color colorSolved   = Color.green;

    private void Update()
    {
        if (progressText == null || MemoryPuzzleManager.Instance == null) return;

        if (MemoryPuzzleManager.Instance.IsSolved.Value)
        {
            progressText.text  = solvedMessage;
            progressText.color = colorSolved;
        }
        else
        {
            int progress = MemoryPuzzleManager.Instance.Progress.Value;
            progressText.text  = string.Format(format, progress, MemoryPuzzleManager.TotalSteps);
            progressText.color = colorProgress;
        }
    }
}

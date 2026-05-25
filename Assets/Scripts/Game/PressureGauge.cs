using TMPro;
using UnityEngine;

// Manómetro 3D para el solucionador del puzzle de presión.
// Muestra:
//   - Presión actual (numérica)
//   - Rango objetivo
//   - Indicador "BAJO / EN RANGO / ALTO"
//   - Barra de tiempo estable (cuánto queda para resolver)
//
// Configura los TextMeshPro 3D en los slots correspondientes. Cualquiera
// puede dejarse vacío si no se quiere mostrar.
public class PressureGauge : MonoBehaviour
{
    [Header("Textos 3D (TextMeshPro)")]
    public TextMeshPro pressureText;     // ej: "PSI: 35"
    public TextMeshPro targetText;       // ej: "Objetivo: 40-50"
    public TextMeshPro statusText;       // ej: "BAJO", "EN RANGO", "ALTO"
    public TextMeshPro stableText;       // ej: "Estable: 2.1 / 3 s"

    [Header("Renderer opcional para colorear el fondo")]
    [Tooltip("Si se asigna, el material toma color rojo/verde según estado.")]
    public Renderer backgroundRenderer;

    [Header("Colores de estado")]
    public Color colorLow      = new Color(0.3f, 0.5f, 1f);  // azul
    public Color colorInRange  = new Color(0.3f, 1f, 0.3f);  // verde
    public Color colorHigh     = new Color(1f, 0.3f, 0.3f);  // rojo
    public Color colorSolved   = new Color(0.3f, 1f, 0.3f);
    public Color colorWaiting  = Color.gray;

    private void Update()
    {
        var mgr = PressurePuzzleManager.Instance;
        if (mgr == null) { ShowWaiting(); return; }

        float pressure = mgr.CurrentPressure.Value;
        float min      = mgr.TargetMin.Value;
        float max      = mgr.TargetMax.Value;
        bool solved    = mgr.IsSolved.Value;

        if (pressureText != null) pressureText.text = $"PSI: {pressure:F0}";
        if (targetText   != null) targetText.text   = $"Objetivo: {min:F0} a {max:F0}";

        if (solved)
        {
            if (statusText != null) { statusText.text = "ESTABILIZADO"; statusText.color = colorSolved; }
            if (stableText != null) stableText.text = "¡Sistema OK!";
            if (backgroundRenderer != null) PaintBackground(colorSolved);
            return;
        }

        // Estado de presión
        string status;
        Color statusColor;
        if (pressure < min)        { status = "BAJO";     statusColor = colorLow; }
        else if (pressure > max)   { status = "ALTO";     statusColor = colorHigh; }
        else                       { status = "EN RANGO"; statusColor = colorInRange; }

        if (statusText != null) { statusText.text = status; statusText.color = statusColor; }
        if (backgroundRenderer != null) PaintBackground(statusColor);

        // Tiempo estable
        if (stableText != null)
        {
            float t = mgr.StableTime.Value;
            float need = mgr.RequiredStableSeconds;
            stableText.text = $"Estable: {t:F1} / {need:F0} s";
        }
    }

    private void ShowWaiting()
    {
        if (pressureText != null) pressureText.text = "PSI: --";
        if (targetText   != null) targetText.text   = "Objetivo: --";
        if (statusText   != null) { statusText.text = "..."; statusText.color = colorWaiting; }
        if (stableText   != null) stableText.text = "";
        if (backgroundRenderer != null) PaintBackground(colorWaiting);
    }

    private void PaintBackground(Color c)
    {
        var mat = backgroundRenderer.material;
        mat.color = c;
        if (mat.HasProperty("_BaseColor"))     mat.SetColor("_BaseColor", c);
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", c * 0.5f);
    }
}

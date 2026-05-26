using TMPro;
using UnityEngine;

// Muestra el tiempo restante en el HUD con formato MM:SS.
// Cambia de color cuando el tiempo es bajo o crítico.
//
// Pon este script en el GameObject "HUD" (o donde quieras), y arrastra
// el TextMeshPro UI al campo timerText.
public class HUDTimer : MonoBehaviour
{
    [Header("Referencia")]
    public TextMeshProUGUI timerText;

    [Header("Colores por estado")]
    public Color colorNormal   = Color.white;
    public Color colorWarning  = new Color(1f, 0.85f, 0.2f);  // amarillo
    public Color colorCritical = new Color(1f, 0.3f, 0.3f);   // rojo
    public Color colorWon      = new Color(0.3f, 1f, 0.3f);   // verde
    public Color colorLost     = new Color(1f, 0.2f, 0.2f);

    [Header("Umbrales (segundos)")]
    public float warningThreshold  = 60f;
    public float criticalThreshold = 30f;

    [Header("Parpadeo en estado crítico")]
    public float blinkFrequency = 3f; // Hz

    private void Update()
    {
        var mgr = GameStateManager.Instance;
        if (mgr == null || timerText == null) return;

        float t = Mathf.Max(0f, mgr.TimeRemaining.Value);

        // Formato MM:SS
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // Color según el estado del juego
        switch (mgr.CurrentState.Value)
        {
            case GameStateManager.GameState.Won:
                timerText.color = colorWon;
                break;
            case GameStateManager.GameState.Lost:
                timerText.color = colorLost;
                break;
            default:
                ApplyPlayingColor(t);
                break;
        }
    }

    private void ApplyPlayingColor(float t)
    {
        if (t <= criticalThreshold)
        {
            // Parpadeo entre crítico y blanco
            float blink = Mathf.PingPong(Time.unscaledTime * blinkFrequency, 1f);
            timerText.color = Color.Lerp(colorCritical, Color.white, blink * 0.4f);
        }
        else if (t <= warningThreshold)
        {
            timerText.color = colorWarning;
        }
        else
        {
            timerText.color = colorNormal;
        }
    }
}

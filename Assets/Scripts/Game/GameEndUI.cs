using TMPro;
using UnityEngine;

// Muestra el panel correspondiente al final de la partida (victoria o derrota)
// y desbloquea el cursor para que se pueda hacer click en los botones de
// "Volver al menú".
//
// Pon en el Canvas (o UIManager) y arrastra los dos paneles + textos de stats.
// Los paneles deben estar DESACTIVADOS por defecto en el inspector.
public class GameEndUI : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Tooltip("HUD que se OCULTA cuando aparece la pantalla de fin (timer, " +
             "tarea, etc.). Arrastra el GameObject 'HUD'.")]
    public GameObject hudRoot;

    [Header("Textos de estadísticas (opcional)")]
    public TextMeshProUGUI victoryStatsText;
    public TextMeshProUGUI defeatStatsText;

    [Header("Mensajes")]
    public string victoryFormat = "¡Misión completada!\nTiempo usado: {0}\nTiempo restante: {1}";
    public string defeatFormat  = "Tiempo agotado.\nRompecabezas resueltos: {0} / 4";

    private GameStateManager.GameState _lastSeenState = GameStateManager.GameState.Playing;

    private void Start()
    {
        SetActiveSafe(victoryPanel, false);
        SetActiveSafe(defeatPanel, false);
    }

    private void Update()
    {
        var mgr = GameStateManager.Instance;
        if (mgr == null) return;

        var state = mgr.CurrentState.Value;
        if (state == _lastSeenState) return;
        _lastSeenState = state;

        switch (state)
        {
            case GameStateManager.GameState.Won:
                SetActiveSafe(defeatPanel, false);
                SetActiveSafe(victoryPanel, true);
                SetActiveSafe(hudRoot, false);
                FillVictoryStats(mgr);
                UnlockCursor();
                break;

            case GameStateManager.GameState.Lost:
                SetActiveSafe(victoryPanel, false);
                SetActiveSafe(defeatPanel, true);
                SetActiveSafe(hudRoot, false);
                FillDefeatStats(mgr);
                UnlockCursor();
                break;

            default:
                SetActiveSafe(victoryPanel, false);
                SetActiveSafe(defeatPanel, false);
                SetActiveSafe(hudRoot, true);
                break;
        }
    }

    private void FillVictoryStats(GameStateManager mgr)
    {
        if (victoryStatsText == null) return;
        float used      = mgr.StartingTime - mgr.TimeRemaining.Value;
        float remaining = mgr.TimeRemaining.Value;
        victoryStatsText.text = string.Format(victoryFormat,
            FormatTime(used), FormatTime(remaining));
    }

    private void FillDefeatStats(GameStateManager mgr)
    {
        if (defeatStatsText == null) return;
        defeatStatsText.text = string.Format(defeatFormat, mgr.CountSolvedPuzzles());
    }

    private static string FormatTime(float seconds)
    {
        int m = Mathf.Max(0, (int)(seconds / 60f));
        int s = Mathf.Max(0, (int)(seconds % 60f));
        return $"{m:00}:{s:00}";
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}

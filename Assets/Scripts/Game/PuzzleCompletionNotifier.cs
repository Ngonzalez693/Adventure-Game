using System.Collections;
using TMPro;
using UnityEngine;

// Muestra un mensaje en pantalla cuando cualquier rompecabezas se completa.
// El mensaje aparece para TODOS los jugadores (porque IsSolved es un
// NetworkVariable sincronizado).
//
// Setup:
//  - Crea un panel/texto en el Canvas (ej. "PuzzleCompleteBanner")
//  - Añade un CanvasGroup al panel
//  - Pon este script en cualquier GameObject del HUD
//  - Arrastra el CanvasGroup y el TextMeshProUGUI al inspector
public class PuzzleCompletionNotifier : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("CanvasGroup del banner (para fade in/out).")]
    public CanvasGroup messageGroup;

    [Tooltip("Texto del mensaje.")]
    public TextMeshProUGUI messageText;

    [Header("Tiempos")]
    [Tooltip("Duración total visible del mensaje.")]
    public float displayDuration = 3.5f;
    [Tooltip("Tiempo de fade in.")]
    public float fadeInTime  = 0.25f;
    [Tooltip("Tiempo de fade out.")]
    public float fadeOutTime = 0.5f;

    [Header("Formato")]
    [Tooltip("Mensaje. {0} = número de jugador (1..4), {1} = nombre del puzzle.")]
    public string messageFormat = "Jugador {0} completó: {1}";

    [Header("Nombres mostrados de cada puzzle")]
    public string codeName     = "Código";
    public string patternName  = "Patrón";
    public string memoryName   = "Memoria";
    public string pressureName = "Presión";

    private bool _codeWasSolved, _patternWasSolved, _memoryWasSolved, _pressureWasSolved;
    private Coroutine _displayRoutine;

    private void Awake()
    {
        if (messageGroup != null) messageGroup.alpha = 0f;
    }

    private void Update()
    {
        var assign = PuzzleAssignmentManager.Instance;
        if (assign == null) return;

        TryNotify(
            PuzzleCodeManager.Instance != null && PuzzleCodeManager.Instance.IsSolved.Value,
            ref _codeWasSolved, codeName, assign.CodePuzzleSlot.Value);

        TryNotify(
            PatternPuzzleManager.Instance != null && PatternPuzzleManager.Instance.IsSolved.Value,
            ref _patternWasSolved, patternName, assign.PatternPuzzleSlot.Value);

        TryNotify(
            MemoryPuzzleManager.Instance != null && MemoryPuzzleManager.Instance.IsSolved.Value,
            ref _memoryWasSolved, memoryName, assign.MemoryPuzzleSlot.Value);

        TryNotify(
            PressurePuzzleManager.Instance != null && PressurePuzzleManager.Instance.IsSolved.Value,
            ref _pressureWasSolved, pressureName, assign.PressurePuzzleSlot.Value);
    }

    private void TryNotify(bool nowSolved, ref bool wasSolved, string puzzleName, int slot)
    {
        if (nowSolved && !wasSolved && slot >= 0)
        {
            string msg = string.Format(messageFormat, slot + 1, puzzleName);
            ShowMessage(msg);
        }
        wasSolved = nowSolved;
    }

    public void ShowMessage(string text)
    {
        if (messageGroup == null || messageText == null) return;

        messageText.text = text;
        if (_displayRoutine != null) StopCoroutine(_displayRoutine);
        _displayRoutine = StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        // Fade in
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            messageGroup.alpha = Mathf.Clamp01(t / fadeInTime);
            yield return null;
        }
        messageGroup.alpha = 1f;

        // Hold
        yield return new WaitForSeconds(displayDuration - fadeInTime - fadeOutTime);

        // Fade out
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            messageGroup.alpha = Mathf.Clamp01(1f - (t / fadeOutTime));
            yield return null;
        }
        messageGroup.alpha = 0f;
        _displayRoutine = null;
    }
}

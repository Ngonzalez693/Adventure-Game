// PuzzleManager.cs
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    public static bool IsAnyPuzzleOpen { get; private set; }

    [Header("Paneles - arrastra desde Hierarchy")]
    public GameObject numberPuzzlePanel;

    [Header("Controladores")]
    public NumberPuzzleUI numberPuzzleUI;

    private InteractableObject currentObject;
    private bool _puzzleOpen;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CerrarTodo();
    }

    // Mientras un puzzle esté abierto, forzamos el cursor visible/libre en
    // CADA frame. Esto cubre el caso donde otro script (cámara, controller,
    // etc.) intenta re-lockearlo y bloquea los clics en los botones.
    void LateUpdate()
    {
        if (!_puzzleOpen) return;

        if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
        if (!Cursor.visible)                          Cursor.visible   = true;
    }

    public void AbrirPuzzle(InteractableObject.PuzzleType tipo, InteractableObject objeto)
    {
        currentObject = objeto;
        CerrarTodo();

        // NO usamos Time.timeScale = 0 porque en multijugador rompe la
        // recepción de input UI en algunos clientes (sigue funcionando solo
        // en el host). En su lugar marcamos un flag global "puzzle abierto"
        // y dejamos que otros sistemas (movimiento del jugador, prompt de
        // interacción) lo consulten para auto-pausarse localmente.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        _puzzleOpen      = true;
        IsAnyPuzzleOpen  = true;

        switch (tipo)
        {
            case InteractableObject.PuzzleType.NumberPuzzle:
                numberPuzzlePanel.SetActive(true);
                numberPuzzleUI.Inicializar();
                break;
        }
    }

    public void PuzzleCompletado()
    {
        currentObject?.SetCompleted();
        CerrarPuzzle();
    }

    public void CerrarPuzzle()
    {
        CerrarTodo();
        _puzzleOpen      = false;
        IsAnyPuzzleOpen  = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void CerrarTodo()
    {
        if (numberPuzzlePanel != null) numberPuzzlePanel.SetActive(false);
    }
}
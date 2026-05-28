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

    public void AbrirPuzzle(InteractableObject.PuzzleType tipo, InteractableObject objeto)
    {
        currentObject = objeto;
        CerrarTodo();

        // El cursor lo mantiene libre el script CursorAlwaysFree (a nivel global).
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
        // No tocamos el cursor.
    }

    void CerrarTodo()
    {
        if (numberPuzzlePanel != null) numberPuzzlePanel.SetActive(false);
    }
}
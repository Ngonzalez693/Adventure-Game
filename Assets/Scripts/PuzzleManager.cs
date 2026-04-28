// PuzzleManager.cs
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Paneles - arrastra desde Hierarchy")]
    public GameObject numberPuzzlePanel;

    [Header("Controladores")]
    public NumberPuzzleUI numberPuzzleUI;

    private InteractableObject currentObject;

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

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void CerrarTodo()
    {
        if (numberPuzzlePanel != null) numberPuzzlePanel.SetActive(false);
    }
}
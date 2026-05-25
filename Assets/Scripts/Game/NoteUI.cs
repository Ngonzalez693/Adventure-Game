using TMPro;
using UnityEngine;

// Popup simple para mostrar el contenido de una nota.
// Pon este script en el Canvas y crea un panel hijo "NotePanel" con:
//   - Imagen de fondo
//   - TextMeshPro para el contenido
//   - Botón "Cerrar" que llame a NoteUI.Cerrar()
public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance;

    [Header("Referencias")]
    public GameObject notePanel;
    public TextMeshProUGUI noteText;

    private void Awake()
    {
        Instance = this;
        if (notePanel != null) notePanel.SetActive(false);
    }

    public void Mostrar(string content)
    {
        if (notePanel == null || noteText == null) return;

        noteText.text = content;
        notePanel.SetActive(true);

        // NO Time.timeScale = 0: rompe input UI en clientes multijugador.
        // El "pausado" se hace solo localmente (cursor + PlayerInteraction
        // detecta esto via PuzzleManager.IsAnyPuzzleOpen/IsAnyNoteOpen).
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Cerrar()
    {
        if (notePanel != null) notePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

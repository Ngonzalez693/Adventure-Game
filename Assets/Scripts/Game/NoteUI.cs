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
        // El cursor lo mantiene libre CursorAlwaysFree (global).
    }

    public void Cerrar()
    {
        if (notePanel != null) notePanel.SetActive(false);
        // No tocamos el cursor.
    }
}

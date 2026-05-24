using UnityEngine;

// Nota que muestra UN dígito en un color específico.
// Cada instancia representa "el número asignado a este color".
// Distribuye estas notas en los mapas de los otros jugadores; en el inspector,
// elige el color que esta nota representa.
//
// Ejemplo: pongo una ColoredNumberNote con color=Rojo en el mapa del jugador B.
// En runtime, mostrará el dígito que el host le asignó al color Rojo.
//
// Requiere:
//   - Collider en el GameObject
//   - NoteUI y PuzzleCodeManager presentes en la escena
public class ColoredNumberNote : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    public string objectName = "Papel";
    public PuzzleCodeManager.PuzzleColor color;

    public void Interact()
    {
        if (PuzzleCodeManager.Instance == null || NoteUI.Instance == null)
        {
            Debug.LogWarning("[ColoredNumberNote] Faltan PuzzleCodeManager o NoteUI.");
            return;
        }

        int digit = PuzzleCodeManager.Instance.GetDigitForColor(color);
        Color c = PuzzleCodeManager.ToUnityColor(color);
        string hex = ColorUtility.ToHtmlStringRGB(c);

        string body = $"<size=200><color=#{hex}>{digit}</color></size>";
        NoteUI.Instance.Mostrar(body);
    }

    public string GetPromptText() => $"Leer {objectName}";
}

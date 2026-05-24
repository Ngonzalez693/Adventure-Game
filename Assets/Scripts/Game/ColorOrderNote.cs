using UnityEngine;

// Nota que muestra el ORDEN de colores que el jugador A debe ingresar
// en la consola. Pon esta nota dentro de una caja en el mapa del jugador A.
//
// Requiere:
//   - Collider en el GameObject (PlayerInteraction lo detecta por OverlapSphere)
//   - NoteUI presente en la escena (en el Canvas)
//   - PuzzleCodeManager presente en la escena
public class ColorOrderNote : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    public string objectName = "Nota de colores";

    public void Interact()
    {
        if (PuzzleCodeManager.Instance == null)
        {
            Debug.LogWarning("[ColorOrderNote] PuzzleCodeManager no encontrado.");
            return;
        }

        if (NoteUI.Instance == null)
        {
            Debug.LogWarning("[ColorOrderNote] NoteUI no encontrado en la escena.");
            return;
        }

        // Construir el texto con cada color tinted con su color real (usando
        // rich-text tags de TextMeshPro).
        var order = PuzzleCodeManager.Instance.GetColorOrder();
        string body = "Orden:\n\n";
        for (int i = 0; i < order.Length; i++)
        {
            Color c = PuzzleCodeManager.ToUnityColor(order[i]);
            string hex = ColorUtility.ToHtmlStringRGB(c);
            body += $"{i + 1}. <color=#{hex}>{order[i]}</color>\n";
        }

        NoteUI.Instance.Mostrar(body);
    }

    public string GetPromptText() => $"Leer {objectName}";
}

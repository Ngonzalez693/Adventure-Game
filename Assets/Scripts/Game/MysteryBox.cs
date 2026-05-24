using UnityEngine;

// Caja interactuable genérica. La caja siempre está visible en el mundo.
// Al interactuar, decide en tiempo real si revelar contenido o mostrar el
// panel de "caja vacía", consultando a PuzzleAssignmentManager.
//
// En el inspector configuras qué PODRÍA contener (color order note,
// colored number note de cierto color, o nada). Si el asignamiento del
// rompecabezas no asigna esa pista a este mapa, la caja queda vacía.
//
// Reemplaza a los scripts antiguos ColorOrderNote, ColoredNumberNote y EmptyBox.
public class MysteryBox : MonoBehaviour, IInteractable
{
    public enum BoxContentType { Empty, ColorOrderNote, ColoredNumberNote }

    [Header("Configuración de la caja")]
    public string objectName = "Caja";

    [Tooltip("Tipo de contenido POTENCIAL. La caja revelará contenido solo " +
             "si la asignación del puzzle pone esta pista en este mapa.")]
    public BoxContentType contentType = BoxContentType.Empty;

    [Tooltip("Solo se usa si contentType = ColoredNumberNote.")]
    public PuzzleCodeManager.PuzzleColor color;

    [Tooltip("Slot del mapa donde está esta caja (0..3). Map1=0, Map2=1, etc.")]
    public int mapSlot;

    public void Interact()
    {
        if (HasContentNow())
            ShowContent();
        else
            ShowEmpty();
    }

    private bool HasContentNow()
    {
        var mgr = PuzzleAssignmentManager.Instance;
        if (mgr == null) return false;

        return contentType switch
        {
            BoxContentType.Empty             => false,
            BoxContentType.ColorOrderNote    => mgr.CodePuzzleSlot.Value == mapSlot,
            BoxContentType.ColoredNumberNote => GetSlotForColor(color) == mapSlot,
            _                                => false,
        };
    }

    private int GetSlotForColor(PuzzleCodeManager.PuzzleColor c)
    {
        var mgr = PuzzleAssignmentManager.Instance;
        return c switch
        {
            PuzzleCodeManager.PuzzleColor.Rojo     => mgr.RojoSlot.Value,
            PuzzleCodeManager.PuzzleColor.Azul     => mgr.AzulSlot.Value,
            PuzzleCodeManager.PuzzleColor.Verde    => mgr.VerdeSlot.Value,
            PuzzleCodeManager.PuzzleColor.Amarillo => mgr.AmarilloSlot.Value,
            _ => -1,
        };
    }

    private void ShowContent()
    {
        if (NoteUI.Instance == null || PuzzleCodeManager.Instance == null) return;

        switch (contentType)
        {
            case BoxContentType.ColorOrderNote:
            {
                var order = PuzzleCodeManager.Instance.GetColorOrder();
                string body = "Orden:\n\n";
                for (int i = 0; i < order.Length; i++)
                {
                    Color c = PuzzleCodeManager.ToUnityColor(order[i]);
                    string hex = ColorUtility.ToHtmlStringRGB(c);
                    body += $"{i + 1}. <color=#{hex}>{order[i]}</color>\n";
                }
                NoteUI.Instance.Mostrar(body);
                break;
            }
            case BoxContentType.ColoredNumberNote:
            {
                int digit = PuzzleCodeManager.Instance.GetDigitForColor(color);
                Color c   = PuzzleCodeManager.ToUnityColor(color);
                string hex = ColorUtility.ToHtmlStringRGB(c);
                string body = $"<size=200><color=#{hex}>{digit}</color></size>";
                NoteUI.Instance.Mostrar(body);
                break;
            }
        }
    }

    private void ShowEmpty()
    {
        if (EmptyBoxUI.Instance != null)
            EmptyBoxUI.Instance.Mostrar();
        else
            Debug.LogWarning("[MysteryBox] EmptyBoxUI no encontrado en la escena.");
    }

    public string GetPromptText() => $"Abrir {objectName}";
}

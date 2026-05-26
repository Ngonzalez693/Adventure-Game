using UnityEngine;
using UnityEngine.EventSystems;

// Reproduce un sonido cuando el cursor entra al área del botón.
// Añádelo a cualquier botón UI (Host/Client/Options/Exit en el menú, etc).
public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMenuButton();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

// Reproduce el sonido de botón cuando el usuario hace click.
// Útil para botones que no son menús pero queremos feedback de press.
public class ButtonPressSound : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMenuButton();
    }
}

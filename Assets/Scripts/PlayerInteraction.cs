// PlayerInteraction.cs
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración")]
    public float interactionRange = 2.5f;

    [Header("UI - Opcional")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;

    private IInteractable currentInteractable;

    void Update()
    {
        CheckForInteractable();

        if (Keyboard.current.eKey.wasPressedThisFrame && currentInteractable != null)
            currentInteractable.Interact();
    }

    void CheckForInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position, interactionRange
        );

        IInteractable closest = null;
        float minDist = float.MaxValue;

        foreach (var col in colliders)
        {
            IInteractable inter = col.GetComponent<IInteractable>();
            if (inter != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = inter;
                }
            }
        }

        currentInteractable = closest;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(closest != null);
            if (promptText != null && closest != null)
                promptText.text = $"[E] {closest.GetPromptText()}";
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
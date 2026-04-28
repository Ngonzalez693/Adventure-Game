// InteractableObject.cs
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    public string objectName = "Caja";
    public PuzzleType puzzleType;
    public bool isCompleted = false;

    public enum PuzzleType { NumberPuzzle, ColorPuzzle }

    public void Interact()
    {
        if (isCompleted)
        {
            Debug.Log("Ya está completado");
            return;
        }

        PuzzleManager.Instance.AbrirPuzzle(puzzleType, this);
    }

    public string GetPromptText()
    {
        return isCompleted ? $"{objectName} ✓" : $"Inspeccionar {objectName}";
    }

    public void SetCompleted()
    {
        isCompleted = true;
        Debug.Log($"¡{objectName} completado!");
    }
}
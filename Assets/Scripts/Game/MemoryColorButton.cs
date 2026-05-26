using UnityEngine;

// Botón de color del puzzle de memoria. Hay 3 (Rojo, Verde, Azul) en el
// mapa del solucionador. Al interactuar, envía el color al servidor para
// que verifique si coincide con el siguiente paso de la secuencia.
//
// Setup:
//  - Pon este script en un GameObject 3D con Collider (un cubo coloreado).
//  - Configura "Color Index": 0 = Rojo, 1 = Verde, 2 = Azul.
public class MemoryColorButton : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [Tooltip("0 = Rojo, 1 = Verde, 2 = Azul")]
    [Range(0, 2)]
    public int colorIndex;

    public string objectName = "Botón";

    [Header("Visual (opcional)")]
    [Tooltip("Renderer que se tinta con el color asignado al iniciar.")]
    public Renderer paintRenderer;

    private void Start()
    {
        if (paintRenderer != null)
        {
            // Pintar el botón con su color (usa material instance)
            paintRenderer.material.color = MemoryPuzzleManager.ToUnityColor(colorIndex);
        }
    }

    public void Interact()
    {
        if (MemoryPuzzleManager.Instance == null)
        {
            Debug.LogWarning("[MemoryColorButton] MemoryPuzzleManager no encontrado.");
            return;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMemoryButton();

        MemoryPuzzleManager.Instance.PressColorServerRpc(colorIndex);
    }

    public string GetPromptText()
    {
        return $"Pulsar {MemoryPuzzleManager.ColorName(colorIndex)}";
    }
}

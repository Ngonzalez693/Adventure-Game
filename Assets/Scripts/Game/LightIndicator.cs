using UnityEngine;

// Esfera con material emisivo que cambia de color según si la palanca de
// su slot asignado está en la posición correcta.
//
// Setup:
//  - Pon este script en la Sphere (o cualquier MeshRenderer).
//  - El material debe tener Emission habilitada (en URP/HDRP también).
//  - Slot Index = el slot cuya palanca controla esta luz.
public class LightIndicator : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Slot cuya palanca controla esta luz (0..3).")]
    public int slotIndex;

    [Header("Colores")]
    public Color colorCorrecto   = Color.green;
    public Color colorIncorrecto = Color.red;

    [Tooltip("Multiplicador de la emisión (más alto = más brillante).")]
    public float emissionIntensity = 1.5f;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (PatternPuzzleManager.Instance == null || _renderer == null) return;

        // Si el puzzle entero está resuelto, todas las luces se ponen en verde
        // (incluyendo la del propio slot del solucionador, que normalmente
        // permanecería en rojo porque él no tiene palancas que mover).
        bool correct = PatternPuzzleManager.Instance.IsSolved.Value
                    || PatternPuzzleManager.Instance.IsSlotLightOn(slotIndex);
        Color c = correct ? colorCorrecto : colorIncorrecto;

        // Usar PropertyBlock para no instanciar nuevos materials por luz
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_BaseColor", c);
        _propBlock.SetColor("_Color", c);            // por si es Standard Shader
        _propBlock.SetColor("_EmissionColor", c * emissionIntensity);
        _renderer.SetPropertyBlock(_propBlock);
    }
}

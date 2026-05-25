using UnityEngine;

// Palanca BINARIA. Cada vez que interactúas, toggle entre dos posiciones (0 / 1).
// 3 palancas en un mapa forman una combinación de 3 bits. Cuando la
// combinación coincide con el objetivo del slot, su luz se enciende.
//
// Setup:
//  - Pon este script en el GameObject de la palanca (con Collider).
//  - Asigna Handle Transform al hijo "Handle" (Cube) que rota visualmente.
//  - Map Slot = slot del mapa donde está esta palanca.
//  - Lever Index = 0, 1 o 2 (cuál de las 3 palancas del mapa es esta).
public class LeverInteractable : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    public string objectName = "Palanca";

    [Tooltip("Slot del mapa donde está esta palanca (0..3). Map1=0, Map2=1, etc.")]
    public int mapSlot;

    [Tooltip("Cuál de las 3 palancas del mapa es esta (0, 1 o 2).")]
    [Range(0, 2)]
    public int leverIndex;

    [Header("Visual")]
    [Tooltip("Transform del handle que rota. Si está vacío, usa este transform.")]
    public Transform handleTransform;

    [Tooltip("Ángulos del handle para posición 0 y posición 1.")]
    public float angleOff = -45f;
    public float angleOn  =  45f;

    [Tooltip("Velocidad de la animación de rotación.")]
    public float rotationSpeed = 10f;

    private void Awake()
    {
        if (handleTransform == null) handleTransform = transform;
    }

    private void Update()
    {
        if (PatternPuzzleManager.Instance == null || handleTransform == null) return;

        int pos = PatternPuzzleManager.Instance.GetLeverPosition(mapSlot, leverIndex);
        float targetAngle = pos == 1 ? angleOn : angleOff;

        Quaternion target = Quaternion.Euler(0f, 0f, targetAngle);
        handleTransform.localRotation = Quaternion.Slerp(
            handleTransform.localRotation, target, Time.deltaTime * rotationSpeed);
    }

    public void Interact()
    {
        if (PatternPuzzleManager.Instance == null)
        {
            Debug.LogWarning("[LeverInteractable] PatternPuzzleManager no encontrado.");
            return;
        }
        PatternPuzzleManager.Instance.ToggleLeverServerRpc(mapSlot, leverIndex);
    }

    public string GetPromptText()
    {
        if (PatternPuzzleManager.Instance == null) return $"Mover {objectName}";
        int pos = PatternPuzzleManager.Instance.GetLeverPosition(mapSlot, leverIndex);
        return $"Mover {objectName} (actual: {pos})";
    }
}

using UnityEngine;

// Válvula binaria. Al interactuar, alterna entre cerrada (0) y abierta (1).
// Cuando está abierta, aporta su delta a la presión.
//
// Setup:
//  - GameObject con Collider.
//  - "Handle Transform" = el hijo que rota (ej: una cruz/rueda).
//  - "Map Slot" = slot del mapa (0..3).
//  - "Valve Index" = 0 o 1 (cuál de las dos válvulas del mapa es esta).
public class ValveInteractable : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    public string objectName = "Válvula";

    [Tooltip("Slot del mapa (0..3).")]
    public int mapSlot;

    [Tooltip("0 o 1 (cuál de las dos válvulas del mapa es esta).")]
    [Range(0, 1)]
    public int valveIndex;

    [Header("Visual")]
    [Tooltip("Transform del handle/rueda que rota. Si está vacío usa este transform.")]
    public Transform handleTransform;

    [Tooltip("Ángulos del handle (Z local) para válvula cerrada / abierta.")]
    public float angleClosed = 0f;
    public float angleOpen   = 90f;

    [Tooltip("Velocidad de la animación de rotación.")]
    public float rotationSpeed = 8f;

    private void Awake()
    {
        if (handleTransform == null) handleTransform = transform;
    }

    private void Update()
    {
        if (PressurePuzzleManager.Instance == null || handleTransform == null) return;

        bool open = PressurePuzzleManager.Instance.IsValveOpen(mapSlot, valveIndex);
        float targetAngle = open ? angleOpen : angleClosed;

        Quaternion target = Quaternion.Euler(0f, 0f, targetAngle);
        handleTransform.localRotation = Quaternion.Slerp(
            handleTransform.localRotation, target, Time.deltaTime * rotationSpeed);
    }

    public void Interact()
    {
        if (PressurePuzzleManager.Instance == null)
        {
            Debug.LogWarning("[ValveInteractable] PressurePuzzleManager no encontrado.");
            return;
        }
        PressurePuzzleManager.Instance.ToggleValveServerRpc(mapSlot, valveIndex);
    }

    public string GetPromptText()
    {
        if (PressurePuzzleManager.Instance == null) return $"Girar {objectName}";

        int globalIndex = mapSlot * PressurePuzzleManager.ValvesPerSlot + valveIndex;
        int delta = PressurePuzzleManager.Instance.GetValveDelta(globalIndex);
        bool open = PressurePuzzleManager.Instance.IsValveOpen(mapSlot, valveIndex);

        string sign  = delta > 0 ? "+" : "";
        string state = open ? "abierta" : "cerrada";
        return $"Girar {objectName} ({sign}{delta}, {state})";
    }
}

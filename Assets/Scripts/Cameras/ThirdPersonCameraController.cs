using UnityEngine;
using UnityEngine.InputSystem;

// Cámara de tercera persona clásica (siempre detrás del jugador).
//
//  Mouse X  →  gira al JUGADOR  (la cámara sigue su espalda)
//  Mouse Y  →  inclina la cámara arriba/abajo únicamente
//
// Coloca este script en la Main Camera de la escena.
// NetworkCameraAssigner llama a SetTarget() al spawnear el jugador local.
public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Asignado automáticamente por NetworkCameraAssigner.")]
    public Transform target;           // root del jugador

    [Header("Posición")]
    public float distance       = 5f;
    public float heightOffset   = 1.6f;
    public float shoulderOffset = 0.6f; // > 0 = hombro derecho

    [Header("Sensibilidad")]
    public float sensitivityX = 180f;   // giro del personaje
    public float sensitivityY = 150f;   // inclinación de cámara

    [Header("Ángulo vertical (grados)")]
    public float minPitch = -20f;
    public float maxPitch =  60f;

    [Header("Suavizado")]
    [Range(1f, 30f)]
    public float followSpeed = 15f;

    // ── Estado ──────────────────────────────────────────────────────────────
    private float _pitch  = 15f;
    private bool  _active = false;

    // ── Loop ─────────────────────────────────────────────────────────────────
    private void LateUpdate()
    {
        if (!_active || target == null) return;

        HandleCursorToggle();

        // Leer delta del mouse
        Vector2 delta = Vector2.zero;
        if (Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
            delta = Mouse.current.delta.ReadValue();

        // ── Mouse X: girar al jugador ──────────────────────────────────────
        // La cámara nunca orbita sola; sigue la espalda del jugador.
        if (delta.x != 0f)
            target.Rotate(0f, delta.x * sensitivityX * Time.deltaTime, 0f, Space.World);

        // ── Mouse Y: solo inclinar la cámara ──────────────────────────────
        _pitch -= delta.y * sensitivityY * Time.deltaTime;
        _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // ── Posición: detrás del jugador usando sus vectores locales ───────
        // Usamos target.forward / target.right directamente para evitar
        // la dependencia circular que causaba eulerAngles.y al mezclar
        // Rotate() y LookAt() en el mismo frame.

        Vector3 pivot = target.position + Vector3.up * heightOffset;

        // Dirección "detrás y un poco arriba" según el pitch
        Quaternion pitchRot  = Quaternion.AngleAxis(_pitch, target.right);
        Vector3    backward  = pitchRot * (-target.forward);

        Vector3 desiredPos = pivot
                           + backward          * distance
                           + target.right      * shoulderOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPos,
                                          Time.deltaTime * followSpeed);
        // Apuntar siempre al jugador
        transform.LookAt(pivot);
    }

    // ── Cursor ───────────────────────────────────────────────────────────────
    private void HandleCursorToggle()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetCursorLocked(false);

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
            && Cursor.lockState == CursorLockMode.None)
            SetCursorLocked(true);
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }

    // ── API ──────────────────────────────────────────────────────────────────
    /// <summary>Llamado por NetworkCameraAssigner al spawnear el jugador.</summary>
    public void SetTarget(Transform playerRoot)
    {
        target  = playerRoot;   // siempre el root del jugador
        _active = true;
        SetCursorLocked(true);
    }
}

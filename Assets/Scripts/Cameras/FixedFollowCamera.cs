using UnityEngine;

// Cámara de tercera persona fija detrás del jugador.
//
// Comportamiento:
//   - No recibe input del mouse.
//   - Siempre intenta estar detrás del jugador.
//   - Cuando el jugador gira (con WASD), la cámara lo sigue suavemente.
//
// Setup:
//   1. Agrega este script a la Main Camera de la escena.
//   2. El jugador tiene NetworkCameraAssigner, que llama SetTarget() al spawnear.
//   3. Cada cliente solo mueve SU cámara (la del jugador local).
public class FixedFollowCamera : MonoBehaviour
{
    [Header("Posición")]
    [Tooltip("Distancia detrás del jugador")]
    public float distance       = 5f;
    [Tooltip("Altura sobre la que apunta la cámara")]
    public float heightOffset   = 1.6f;
    [Tooltip("Offset lateral (0 = centrado, >0 = hombro derecho)")]
    public float shoulderOffset = 0f;

    [Header("Suavizado")]
    [Tooltip("Velocidad con la que la cámara sigue al jugador al girar")]
    [Range(1f, 20f)]
    public float followSpeed = 6f;

    // ── Estado interno ─────────────────────────────────────────────────────
    private Transform _target;
    private bool      _active = false;

    // ── Loop ──────────────────────────────────────────────────────────────
    private void LateUpdate()
    {
        if (!_active || _target == null) return;

        // Pivote: posición del jugador + altura
        Vector3 pivot = _target.position + Vector3.up * heightOffset;

        // Posición deseada: detrás del jugador usando sus vectores locales.
        // -_target.forward siempre apunta a la espalda del jugador.
        // Cuando el jugador gira, forward cambia y desiredPos se actualiza.
        Vector3 desiredPos = pivot
                           - _target.forward * distance
                           + _target.right   * shoulderOffset;

        // Seguimiento suavizado: la cámara llega gradualmente a desiredPos.
        // Cuanto más bajo sea followSpeed, más "lag" de cámara hay.
        transform.position = Vector3.Lerp(transform.position, desiredPos,
                                          Time.deltaTime * followSpeed);

        // Siempre apuntar al jugador
        transform.LookAt(pivot);
    }

    // ── API pública ────────────────────────────────────────────────────────
    /// <summary>
    /// Llamado por NetworkCameraAssigner cuando el jugador local spawnea.
    /// Solo el jugador dueño (owner) llama esto, por eso cada cliente
    /// mueve únicamente la cámara de su propio personaje.
    /// </summary>
    public void SetTarget(Transform playerRoot)
    {
        _target = playerRoot;
        _active = true;
    }
}

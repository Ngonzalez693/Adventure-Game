using UnityEngine;

public class CameraRootFollower : MonoBehaviour
{
    [Tooltip("Se asigna automáticamente por NetworkCameraAssigner al spawnear. " +
             "También puedes arrastrarlo manualmente aquí como alternativa.")]
    public Transform target;

    // Llamado por NetworkCameraAssigner en tiempo de ejecución
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + Vector3.up * 0.9f;
    }
}

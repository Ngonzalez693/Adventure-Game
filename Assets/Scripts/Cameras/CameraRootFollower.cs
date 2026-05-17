using UnityEngine;

public class CameraRootFollower : MonoBehaviour
{
    public Transform playerRoot;

    public void SetTarget(Transform player)
    {
        playerRoot = player;
    }

    private void LateUpdate()
    {
        if (playerRoot == null) return;

        // Posición sigue al jugador normalmente (con Y incluida)
        transform.position = playerRoot.position;

        // Solo copia rotación Y, congela X y Z
        transform.rotation = Quaternion.Euler(
            0f,
            playerRoot.eulerAngles.y,
            0f
        );
    }
}
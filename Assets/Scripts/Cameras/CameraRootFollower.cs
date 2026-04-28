using UnityEngine;

public class CameraRootFollower : MonoBehaviour
{
    public Transform target; // arrastra Player1 aquí

    void LateUpdate()
    {
        if (target != null)
        {
            // Solo sigue la posición, ignora la rotación del Player
            transform.position = target.position + Vector3.up * 0.9f;
        }
    }
}
// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Arrastra aquí tu script de movimiento del jugador")]
    public MonoBehaviour playerMovementScript;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetPlayerMovement(bool active)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = active;
    }
}
using UnityEngine;

// Asegura que el cursor del ratón nunca esté bloqueado ni invisible.
// Pon este script en cualquier GameObject persistente (ej. el mismo
// donde está SoundManager). Sobrescribe cualquier intento de cualquier
// otro script de bloquear el cursor.
public class CursorAlwaysFree : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        UnlockNow();
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
        if (!Cursor.visible)                          Cursor.visible   = true;
    }

    private static void UnlockNow()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }
}

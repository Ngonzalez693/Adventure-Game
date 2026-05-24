using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class UiInGame : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [Tooltip("Nombre exacto de la escena del juego (debe estar en Build Settings)")]
    public string nameEscenaMenu = "MenuScene";

    [Header("Paneles")]
    [Tooltip("Arrastra aquí los GameObject de los paneles")]
    public GameObject optionsPanel;

    // ────────────────────────────────────────────────
    //  UPDATE — ESC toggles the options panel
    // ────────────────────────────────────────────────
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            OnPressOptions();
    }

    // ────────────────────────────────────────────────
    //  OPTIONS BUTTON
    // ────────────────────────────────────────────────
    public void OnPressOptions()
    {
        if (optionsPanel == null)
        {
            Debug.LogWarning("UiInGame: no se asignó el Panel de Opciones en el Inspector.");
            return;
        }

        bool open = !optionsPanel.activeSelf;
        optionsPanel.SetActive(open);

        // Cuando se abre el panel, liberamos y mostramos el cursor para que
        // se puedan clickear los botones. Al cerrarlo, lo volvemos a bloquear.
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = open;
    }

    // Auxiliary method to close the options panel from a "Back" button inside the panel
    public void OnClickCloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // Restaurar el cursor bloqueado al estilo de juego
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    // ────────────────────────────────────────────────
    //  BACK TO MENU BUTTON
    // ────────────────────────────────────────────────
    public void OnClickExitToMenu()
    {
        // Si hay conexión de red activa, la cerramos primero.
        // Esto desconecta a los clientes (si somos host) o nos desconecta a
        // nosotros (si somos cliente). Sin esto, Netcode queda corriendo en
        // background y la próxima sesión empieza en estado inconsistente.
        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene(nameEscenaMenu);
    }

    // ────────────────────────────────────────────────
    //  EXIT BUTTON
    // ────────────────────────────────────────────────
    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
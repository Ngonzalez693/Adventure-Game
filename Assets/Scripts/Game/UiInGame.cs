using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class UiInGame : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [Tooltip("Nombre exacto de la escena del menú (debe estar en Build Settings)")]
    public string nameEscenaMenu  = "MenuScene";

    [Tooltip("Nombre exacto de la escena del lobby (debe estar en Build Settings)")]
    public string nameEscenaLobby = "Lobby";

    [Header("Paneles")]
    [Tooltip("Arrastra aquí los GameObject de los paneles")]
    public GameObject optionsPanel;

    // ────────────────────────────────────────────────
    //  UPDATE — ESC toggles the options panel
    // ────────────────────────────────────────────────
    // Guarda estática para evitar dobles toggles si por algún motivo hay
    // más de un componente UiInGame escuchando la tecla P en el mismo frame.
    private static int _lastPHandledFrame = -1;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (Time.frameCount == _lastPHandledFrame) return;
            _lastPHandledFrame = Time.frameCount;
            OnPressOptions();
        }
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

        Debug.Log($"[UiInGame] OptionsPanel '{optionsPanel.name}' (id={optionsPanel.GetInstanceID()}) " +
                  $"→ active={optionsPanel.activeSelf}, activeInHierarchy={optionsPanel.activeInHierarchy}");

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
    //  BACK TO LOBBY BUTTON (desde panel de victoria/derrota)
    // ────────────────────────────────────────────────
    // Solo el HOST puede ejecutar este cambio de escena, porque NGO requiere
    // que SceneManager.LoadScene se llame desde el servidor. Los clientes que
    // pulsen el botón no harán nada; lo ideal es ocultarles el botón en el
    // inspector (o reemplazarlo por un texto "Esperando al host...").
    public void OnClickBackToLobby()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("[UiInGame] NetworkManager no encontrado.");
            return;
        }

        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[UiInGame] Solo el host puede regresar al lobby.");
            return;
        }

        // Cambio de escena SINCRONIZADO con todos los clientes vía Netcode.
        // El NetworkManager y los player objects persisten — los managers y
        // estado del rompecabezas (que están en GameMap) se destruyen y luego
        // se vuelven a generar la próxima vez que se cargue GameMap.
        NetworkManager.Singleton.SceneManager.LoadScene(
            nameEscenaLobby, UnityEngine.SceneManagement.LoadSceneMode.Single);
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
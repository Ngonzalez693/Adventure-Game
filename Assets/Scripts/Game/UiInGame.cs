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

        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    // Auxiliary method to close the options panel from a "Back" button inside the panel
    public void OnClickCloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    // ────────────────────────────────────────────────
    //  BACK TO MENU BUTTON
    // ────────────────────────────────────────────────
    public void OnClickExitToMenu()
    {
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
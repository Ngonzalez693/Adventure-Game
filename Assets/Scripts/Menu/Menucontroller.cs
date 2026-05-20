using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [Tooltip("Nombre exacto de la escena del juego (debe estar en Build Settings)")]
    public string nombreEscenaJuego = "GameScene";

    [Header("Paneles")]
    [Tooltip("Arrastra aquí los GameObject de los paneles")]
    public GameObject panelOpciones;
    public GameObject panelCrearPartida;

    // ────────────────────────────────────────────────
    //  BOTÓN JUGAR
    // ────────────────────────────────────────────────
    public void OnClickJugar()
    {
        panelCrearPartida.SetActive(true);
    }

    public void OnClickCrearPartida()
    {
        // Carga la escena indicada en 'nombreEscenaJuego'
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void OnClickCerrarJugar()
    {
        if (panelCrearPartida != null)
            panelCrearPartida.SetActive(false);
    }

    // ────────────────────────────────────────────────
    //  BOTÓN OPCIONES / CONTROLES
    // ────────────────────────────────────────────────
    public void OnClickOpciones()
    {
        if (panelOpciones == null)
        {
            Debug.LogWarning("MenuController: no se asignó el Panel de Opciones en el Inspector.");
            return;
        }

        // Alterna la visibilidad del panel
        bool estaActivo = panelOpciones.activeSelf;
        panelOpciones.SetActive(!estaActivo);
    }

    // Método auxiliar para cerrar el panel desde un botón "Volver" dentro del panel
    public void OnClickCerrarOpciones()
    {
        if (panelOpciones != null)
            panelOpciones.SetActive(false);
    }

    // ────────────────────────────────────────────────
    //  BOTÓN SALIR
    // ────────────────────────────────────────────────
    public void OnClickSalir()
    {
        #if UNITY_EDITOR
                // En el editor detiene el Play Mode
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                // En la build real cierra la aplicación
                Application.Quit();
        #endif
    }
}
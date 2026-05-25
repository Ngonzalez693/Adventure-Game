using UnityEngine;

// Panel que se muestra cuando una caja está vacía.
// El panel típicamente contiene una imagen de una caja vacía, un texto
// opcional ("Esta caja está vacía") y un botón cerrar.
//
// Pon este script en el Canvas (o en cualquier GameObject de la escena) y
// arrastra el panel hijo "EmptyBoxPanel" al campo correspondiente.
public class EmptyBoxUI : MonoBehaviour
{
    public static EmptyBoxUI Instance;

    [Header("Referencias")]
    public GameObject emptyBoxPanel;

    private void Awake()
    {
        Instance = this;
        if (emptyBoxPanel != null) emptyBoxPanel.SetActive(false);
    }

    public void Mostrar()
    {
        if (emptyBoxPanel == null) return;
        emptyBoxPanel.SetActive(true);

        // NO Time.timeScale = 0 (rompe input UI en clientes multijugador).
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Cerrar()
    {
        if (emptyBoxPanel != null) emptyBoxPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

using UnityEngine;

// Caja "decoy" sin contenido útil. Sirve para que los jugadores tengan que
// buscar y no encuentren la pista de inmediato.
public class EmptyBox : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    public string objectName = "Caja";
    [Tooltip("Mensajes aleatorios al abrir una caja vacía.")]
    public string[] mensajesVacios = new[]
    {
        "Está vacía.",
        "Solo polvo y herramientas viejas.",
        "Nada útil aquí.",
        "Tornillos sueltos y un trapo."
    };

    public void Interact()
    {
        if (NoteUI.Instance == null) return;
        string msg = mensajesVacios.Length > 0
            ? mensajesVacios[Random.Range(0, mensajesVacios.Length)]
            : "Vacía.";
        NoteUI.Instance.Mostrar(msg);
    }

    public string GetPromptText() => $"Abrir {objectName}";
}

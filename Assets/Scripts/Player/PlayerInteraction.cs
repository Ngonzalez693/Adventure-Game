// PlayerInteraction.cs
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

// Detecta objetos interactuables cercanos y muestra un prompt en pantalla.
// Solo el jugador local (owner) ejecuta esta lógica, así varios jugadores
// no pelean por el mismo texto de UI.
public class PlayerInteraction : NetworkBehaviour
{
    [Header("Configuración")]
    public float interactionRange = 2.5f;

    [Header("UI (opcional — se busca automáticamente si está vacío)")]
    [Tooltip("Si está vacío, se buscará por tag 'InteractionPrompt' en la escena.")]
    public GameObject interactionPrompt;
    [Tooltip("Si está vacío, se toma el primer TMP_Text hijo de interactionPrompt.")]
    public TextMeshProUGUI promptText;

    private IInteractable currentInteractable;
    private bool _uiResolved;

    public override void OnNetworkSpawn()
    {
        // Solo el dueño del personaje muestra/usa el prompt de interacción.
        // Los demás desactivan este componente para no procesar Update.
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        ResolveUiReferences();

        // Re-resolver la UI al cambiar de escena (Lobby → GameMap, etc.).
        // El player object persiste entre escenas pero el Canvas no.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsOwner) return;
        // Invalidamos referencias viejas y forzamos re-búsqueda
        _uiResolved = false;
        interactionPrompt = null;
        promptText = null;
        ResolveUiReferences();
    }

    private void ResolveUiReferences()
    {
        // Buscar el GameObject del prompt en la escena si no se asignó manualmente.
        // Lo buscamos por TAG para que funcione en cualquier escena (Lobby, GameMap).
        if (interactionPrompt == null)
        {
            var go = GameObject.FindGameObjectWithTag("InteractionPrompt");
            if (go != null) interactionPrompt = go;
        }

        // Auto-resolver el TextMeshPro hijo si no se asignó.
        if (promptText == null && interactionPrompt != null)
            promptText = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>(true);

        // Solo marcamos como "resuelto" si REALMENTE encontramos la UI.
        // Si no, seguimos intentando cada frame — esto cubre el caso de
        // arrancar como Host desde MenuScene (el player spawnea antes de
        // que la escena Lobby cargue y su InteractionPrompt exista).
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
            _uiResolved = true;
        }
    }

    void Update()
    {
        // Solo el owner ejecuta interacción (defensa extra además de OnNetworkSpawn).
        if (!IsOwner) return;

        // Si la UI no se ha resuelto todavía (ej: la escena cambió y aún no
        // existían los objetos), intentamos resolver de nuevo.
        if (!_uiResolved) ResolveUiReferences();

        // Si hay un modal abierto (puzzle, nota, caja vacía), o el juego ya
        // terminó (victoria/derrota), pausamos la detección de interactuables:
        // el prompt no debe mostrarse y E no debe disparar otra interacción.
        bool gameOver = GameStateManager.Instance != null && GameStateManager.Instance.IsGameOver;
        if (PuzzleManager.IsAnyPuzzleOpen || IsAnyNoteOpen() || gameOver)
        {
            if (interactionPrompt != null && interactionPrompt.activeSelf)
                interactionPrompt.SetActive(false);
            return;
        }

        CheckForInteractable();

        if (Keyboard.current.eKey.wasPressedThisFrame && currentInteractable != null)
            currentInteractable.Interact();
    }

    void CheckForInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position, interactionRange
        );

        IInteractable closest = null;
        float minDist = float.MaxValue;

        foreach (var col in colliders)
        {
            IInteractable inter = col.GetComponent<IInteractable>();
            if (inter != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = inter;
                }
            }
        }

        currentInteractable = closest;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(closest != null);
            if (promptText != null && closest != null)
                promptText.text = $"[E] {closest.GetPromptText()}";
        }
    }

    // Cuando cambiamos de escena (Lobby -> Cinematic -> GameMap), las referencias
    // de UI quedan inválidas. Forzamos re-resolución.
    private void OnEnable()
    {
        _uiResolved = false;
        interactionPrompt = null;
        promptText = null;
    }

    private static bool IsAnyNoteOpen()
    {
        return (NoteUI.Instance     != null && NoteUI.Instance.notePanel       != null && NoteUI.Instance.notePanel.activeSelf)
            || (EmptyBoxUI.Instance != null && EmptyBoxUI.Instance.emptyBoxPanel != null && EmptyBoxUI.Instance.emptyBoxPanel.activeSelf);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

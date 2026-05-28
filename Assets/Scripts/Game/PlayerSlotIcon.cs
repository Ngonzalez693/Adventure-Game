using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// Muestra el icono POR SLOT (no por rol) en una UI Image.
// Usar para el avatar/portrait del jugador local en la esquina del HUD,
// o para mostrar "qué jugador es" cuando queremos distinguir personas
// más allá del rol que les tocó.
[RequireComponent(typeof(Image))]
public class PlayerSlotIcon : MonoBehaviour
{
    public enum SlotSource { Specific, LocalPlayer }

    [Header("Fuente del slot")]
    [Tooltip("Specific = slot fijo. LocalPlayer = el slot del jugador local.")]
    public SlotSource source = SlotSource.LocalPlayer;

    [Tooltip("Solo se usa si source = Specific.")]
    [Range(0, 3)]
    public int specificSlot;

    [Header("Comportamiento")]
    public bool hideWhenNoIcon;

    private Image _image;

    private void Awake() => _image = GetComponent<Image>();

    private float _logTimer;

    private void Update()
    {
        if (_image == null)
        {
            DebugThrottled("Image null");
            return;
        }
        if (PlayerSlotIconLibrary.Instance == null)
        {
            DebugThrottled("PlayerSlotIconLibrary.Instance == null — falta el componente en la escena.");
            return;
        }

        int slot = ResolveSlot();
        Sprite sprite = PlayerSlotIconLibrary.Instance.GetIconForSlot(slot);

        DebugThrottled($"slot={slot}, sprite={(sprite != null ? sprite.name : "NULL")}");

        if (sprite == null)
        {
            if (hideWhenNoIcon) _image.enabled = false;
            return;
        }

        _image.enabled = true;
        if (_image.sprite != sprite)
            _image.sprite = sprite;
    }

    private int ResolveSlot()
    {
        if (source == SlotSource.LocalPlayer)
        {
            var spawner = NetworkPlayerSpawner.Instance;
            if (spawner == null || NetworkManager.Singleton == null) return -1;
            return spawner.GetSlotForClient(NetworkManager.Singleton.LocalClientId);
        }
        return specificSlot;
    }

    private void DebugThrottled(string msg)
    {
        _logTimer -= Time.deltaTime;
        if (_logTimer <= 0f)
        {
            Debug.Log($"[PlayerSlotIcon] {name}: {msg}");
            _logTimer = 2f; // log cada 2 segundos
        }
    }
}

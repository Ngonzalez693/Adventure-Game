using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// Pon este script en una UI Image que debe mostrar el icono de rol de un
// jugador específico (por slot) o del jugador local. Se actualiza automáticamente
// cuando se conoce la asignación del puzzle.
//
// Uso típico:
//  - En los 4 chips "JUGADOR 1..4" del HUD: source = Specific, specificSlot = 0/1/2/3
//  - En el icono del jugador local (esquina inferior derecha): source = LocalPlayer
[RequireComponent(typeof(Image))]
public class PlayerRoleIcon : MonoBehaviour
{
    public enum SlotSource { Specific, LocalPlayer }

    [Header("Fuente del slot")]
    [Tooltip("Specific = un slot fijo (0..3). LocalPlayer = el slot del jugador local.")]
    public SlotSource source = SlotSource.Specific;

    [Tooltip("Solo se usa si source = Specific. Slot fijo (0=Jugador 1, 1=Jugador 2...).")]
    [Range(0, 3)]
    public int specificSlot;

    [Header("Comportamiento")]
    [Tooltip("Si el icono no está disponible, ocultar la Image en lugar de mostrar el default.")]
    public bool hideWhenNoIcon;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void Update()
    {
        if (_image == null || RoleIconLibrary.Instance == null) return;

        int slot = ResolveSlot();
        Sprite sprite = RoleIconLibrary.Instance.GetIconForSlot(slot);

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
}

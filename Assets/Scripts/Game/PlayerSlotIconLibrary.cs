using UnityEngine;

// Biblioteca de iconos POR SLOT (jugador 1..4). Independiente del rol.
// Útil para mostrar "tu icono de jugador" (avatar/color) en la esquina del HUD,
// que se mantiene aunque cambie tu rol entre partidas.
//
// RoleIconLibrary  → iconos según el ROL (code/pattern/memory/pressure)
// PlayerSlotIconLibrary → iconos según el SLOT del jugador (0/1/2/3)
public class PlayerSlotIconLibrary : MonoBehaviour
{
    public static PlayerSlotIconLibrary Instance;

    [Header("Iconos por slot")]
    [Tooltip("Icono del Jugador 1 (slot 0).")]
    public Sprite slot0Icon;

    [Tooltip("Icono del Jugador 2 (slot 1).")]
    public Sprite slot1Icon;

    [Tooltip("Icono del Jugador 3 (slot 2).")]
    public Sprite slot2Icon;

    [Tooltip("Icono del Jugador 4 (slot 3).")]
    public Sprite slot3Icon;

    [Header("Fallback")]
    public Sprite defaultIcon;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public Sprite GetIconForSlot(int slot) => slot switch
    {
        0 => slot0Icon != null ? slot0Icon : defaultIcon,
        1 => slot1Icon != null ? slot1Icon : defaultIcon,
        2 => slot2Icon != null ? slot2Icon : defaultIcon,
        3 => slot3Icon != null ? slot3Icon : defaultIcon,
        _ => defaultIcon
    };
}

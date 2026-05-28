using Unity.Netcode;
using UnityEngine;

// Pon este script en el prefab del jugador. Aplica una textura distinta al
// cuerpo según el SLOT del jugador (0..3). Como el mapeo de slots se
// sincroniza por red (NetworkPlayerSpawner), todos los clientes ven a cada
// jugador con su textura correspondiente.
//
// Setup:
//  - Arrastra las 4 texturas en orden (slot 0, 1, 2, 3).
//  - Arrastra los Renderer(s) del cuerpo que deben recibir la textura
//    (puede ser SkinnedMeshRenderer o MeshRenderer).
//  - Si tu material usa otra propiedad de textura, cámbiala (URP = _BaseMap,
//    Built-in = _MainTex, HDRP = _BaseColorMap).
public class PlayerBodyTexture : NetworkBehaviour
{
    [Header("Texturas por slot (0..3)")]
    [Tooltip("Textura 0 = Jugador 1, 1 = Jugador 2, etc.")]
    public Texture[] slotTextures = new Texture[4];

    [Header("Renderers a teñir")]
    [Tooltip("Renderer(s) del cuerpo. Puede ser SkinnedMeshRenderer o MeshRenderer.")]
    public Renderer[] bodyRenderers;

    [Header("Material")]
    [Tooltip("Nombre de la propiedad de textura del shader. " +
             "URP = _BaseMap, Built-in = _MainTex, HDRP = _BaseColorMap.")]
    public string texturePropertyName = "_BaseMap";

    // Recordamos el último slot aplicado para no actualizar el material cada
    // frame innecesariamente.
    private int _lastAppliedSlot = -1;

    private void Update()
    {
        if (slotTextures == null || slotTextures.Length == 0) return;
        if (bodyRenderers == null || bodyRenderers.Length == 0) return;

        var spawner = NetworkPlayerSpawner.Instance;
        if (spawner == null) return;

        int slot = spawner.GetSlotForClient(OwnerClientId);
        if (slot < 0 || slot >= slotTextures.Length) return;
        if (slot == _lastAppliedSlot) return;

        ApplyTexture(slotTextures[slot]);
        _lastAppliedSlot = slot;
    }

    private void ApplyTexture(Texture tex)
    {
        if (tex == null) return;

        foreach (var rend in bodyRenderers)
        {
            if (rend == null) continue;

            // rend.material crea una instancia local del material para no
            // modificar el asset compartido. OK porque solo hay 1 por jugador.
            var mat = rend.material;

            if (mat.HasProperty(texturePropertyName))
            {
                mat.SetTexture(texturePropertyName, tex);
            }
            else
            {
                // Fallback: probar las propiedades más comunes
                if (mat.HasProperty("_MainTex"))         mat.SetTexture("_MainTex", tex);
                else if (mat.HasProperty("_BaseMap"))    mat.SetTexture("_BaseMap", tex);
                else if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", tex);
            }
        }
    }
}

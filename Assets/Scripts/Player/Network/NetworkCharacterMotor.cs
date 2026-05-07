using Unity.Netcode;
using UnityEngine;
using StarterAssets;

namespace Player
{
    public class NetworkCharacterMotor : NetworkBehaviour
    {
        private ThirdPersonController _thirdPerson;

        private void Awake()
        {
            _thirdPerson = GetComponent<ThirdPersonController>();
        }

        public override void OnNetworkSpawn()
        {
            // ── Posicionar en spawn point ─────────────────────────────────────
            // Se hace aquí porque en OnNetworkSpawn el objeto YA existe en la red,
            // a diferencia del callback OnClientConnectedCallback donde
            // PlayerObject puede ser null para clientes remotos.
            if (IsServer)
            {
                var spawner = NetworkPlayerSpawner.Instance;
                if (spawner != null)
                {
                    Transform spawnPoint = spawner.GetNextSpawnPoint();
                    if (spawnPoint != null)
                    {
                        transform.position = spawnPoint.position;
                        transform.rotation = spawnPoint.rotation;
                    }
                }
            }

            // ── Activar/desactivar ThirdPersonController ──────────────────────
            // Solo el dueño mueve su personaje localmente.
            // Los demás reciben posición y animaciones por red.
            if (!IsOwner && _thirdPerson != null)
                _thirdPerson.enabled = false;
        }
    }
}

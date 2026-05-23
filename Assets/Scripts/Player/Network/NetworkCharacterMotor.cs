using System.Collections;
using Unity.Netcode;
using UnityEngine;
using StarterAssets;

namespace Player
{
    public class NetworkCharacterMotor : NetworkBehaviour
    {
        private ThirdPersonController _thirdPerson;
        private CharacterController _characterController;

        private void Awake()
        {
            _thirdPerson = GetComponent<ThirdPersonController>();
            _characterController = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            // ── Posicionar en spawn point (solo servidor) ─────────────────────
            // Esperamos un frame para asegurar que la escena (y por tanto el
            // NetworkPlayerSpawner) estén completamente cargados.
            // Esto resuelve el caso del HOST, cuyo player se crea ANTES de
            // que termine de cargar la escena Lobby.
            if (IsServer)
                StartCoroutine(SpawnAtPointWhenReady());

            // ── Activar/desactivar ThirdPersonController ──────────────────────
            // Solo el dueño mueve su personaje localmente.
            // Los demás reciben posición y animaciones por red.
            if (!IsOwner && _thirdPerson != null)
                _thirdPerson.enabled = false;
        }

        private IEnumerator SpawnAtPointWhenReady()
        {
            // Esperar hasta que el spawner exista en la escena cargada.
            // Para clientes remotos esto pasa inmediatamente; para el host
            // puede tomar uno o dos frames mientras carga el Lobby.
            float timeout = 5f;
            while (NetworkPlayerSpawner.Instance == null && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            var spawner = NetworkPlayerSpawner.Instance;
            if (spawner == null)
            {
                Debug.LogError("[NetworkCharacterMotor] NetworkPlayerSpawner no encontrado tras timeout.");
                yield break;
            }

            Transform spawnPoint = spawner.GetNextSpawnPoint();
            if (spawnPoint == null) yield break;

            // CharacterController bloquea cambios directos a transform.position.
            // Hay que desactivarlo, mover, y reactivarlo.
            bool wasEnabled = _characterController != null && _characterController.enabled;
            if (_characterController != null) _characterController.enabled = false;

            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            if (_characterController != null && wasEnabled)
                _characterController.enabled = true;
        }
    }
}

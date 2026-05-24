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

            // Usamos el clientId (OwnerClientId del player) para que el slot
            // de spawn sea determinístico y persistente entre escenas.
            Transform spawnPoint = spawner.GetSpawnPointForClient(OwnerClientId);
            if (spawnPoint == null) yield break;

            TeleportTo(spawnPoint.position, spawnPoint.rotation);
        }

        // Mueve al jugador a una posición/rotación dadas.
        // Como el prefab usa ClientNetworkTransform (autoridad del cliente),
        // el servidor NO puede reposicionar al jugador directamente: solo el
        // dueño tiene autoridad sobre su propio transform. Por eso enviamos
        // un ClientRpc al owner para que se teleporte a sí mismo.
        // Llamar desde el servidor.
        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            if (!IsServer) return;

            // Aplicar también en el servidor para que tenga la posición correcta
            // de inmediato (por si necesitamos leerla antes del RPC).
            ApplyTeleportLocal(position, rotation);

            // Si el dueño es el propio host (server = client 0), el ApplyLocal
            // ya hizo el trabajo. Si es un cliente remoto, mandamos RPC.
            if (OwnerClientId != NetworkManager.ServerClientId)
            {
                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { OwnerClientId }
                    }
                };
                TeleportClientRpc(position, rotation, rpcParams);
            }
        }

        [ClientRpc]
        private void TeleportClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams rpcParams = default)
        {
            // Solo el dueño aplica el teleport (defensa extra, el RPC ya está
            // dirigido al owner via TargetClientIds).
            if (!IsOwner) return;
            StartCoroutine(ApplyTeleportLocalRoutine(position, rotation));
        }

        // Aplica el teleport en la instancia local. Lo hace en una corrutina
        // para esperar un tick de física entre el cambio de posición y la
        // reactivación del CharacterController. Sin esa espera, el siguiente
        // GroundedCheck devuelve false (los colliders aún no se actualizaron),
        // la gravedad acumula velocidad y el jugador cae a través del mapa.
        private void ApplyTeleportLocal(Vector3 position, Quaternion rotation)
        {
            StartCoroutine(ApplyTeleportLocalRoutine(position, rotation));
        }

        // Distancia máxima hacia abajo para buscar el suelo desde el spawn point.
        private const float GroundSearchDistance = 50f;

        // Cuánto se desplaza el jugador hacia arriba del suelo encontrado,
        // para evitar que el CC se quede pegado/atravesando.
        private const float GroundOffset = 0.05f;

        // Si después del teleport el jugador cae por debajo de esta distancia
        // respecto al spawn original, lo volvemos a teleportar.
        private const float FallThreshold = 5f;

        private IEnumerator ApplyTeleportLocalRoutine(Vector3 position, Quaternion rotation)
        {
            // Pausar el ThirdPersonController para que su Update no llame a
            // _controller.Move() mientras el CharacterController está apagado.
            bool tpcWasEnabled = _thirdPerson != null && _thirdPerson.enabled;
            if (_thirdPerson != null) _thirdPerson.enabled = false;

            bool wasEnabled = _characterController != null && _characterController.enabled;
            if (_characterController != null) _characterController.enabled = false;

            // Aplicar posición/rotación inicial
            transform.position = position;
            transform.rotation = rotation;
            Physics.SyncTransforms();

            // Esperar varios frames para que los colliders de la nueva escena
            // estén listos en el cliente (especialmente importante con latencia).
            for (int i = 0; i < 5; i++)
            {
                transform.position = position;
                transform.rotation = rotation;
                yield return new WaitForFixedUpdate();
            }

            // Buscar el suelo real bajo el spawn point con un raycast.
            // Esto garantiza que el jugador caiga sobre geometría real,
            // sin importar la altura exacta del spawn point.
            Vector3 finalPosition = position;
            Vector3 rayStart = position + Vector3.up * 1f;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit,
                                GroundSearchDistance + 1f,
                                ~0, QueryTriggerInteraction.Ignore))
            {
                finalPosition = hit.point + Vector3.up * GroundOffset;
            }

            transform.position = finalPosition;
            Physics.SyncTransforms();

            // Resetear velocidad vertical antes de reactivar el CC
            ResetVerticalVelocityIfPossible();

            if (_characterController != null && wasEnabled)
                _characterController.enabled = true;

            if (_thirdPerson != null && tpcWasEnabled && IsOwner)
                _thirdPerson.enabled = true;

            // Watchdog: si el jugador cae anormalmente bajo el spawn point
            // (porque el raycast falló o algo salió mal), lo re-posicionamos.
            // Esto cubre el caso de latencia donde los colliders aún no estaban
            // listos al momento del primer teleport.
            StartCoroutine(FallSafetyNet(finalPosition, rotation));
        }

        // Vigila al jugador durante unos segundos tras el teleport.
        // Si cae por debajo del threshold, lo re-teleporta hasta que se estabilice.
        private IEnumerator FallSafetyNet(Vector3 targetPosition, Quaternion targetRotation)
        {
            float watchTime = 3f;
            float elapsed = 0f;
            int retries = 0;
            const int MaxRetries = 3;

            while (elapsed < watchTime && retries < MaxRetries)
            {
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;

                if (transform.position.y < targetPosition.y - FallThreshold)
                {
                    Debug.LogWarning($"[NetworkCharacterMotor] Jugador {OwnerClientId} cayó " +
                                     $"(Y={transform.position.y:F2}, target={targetPosition.y:F2}). Re-teleportando.");
                    retries++;

                    // Re-aplicar sin pasar por la corrutina completa
                    bool tpcWas = _thirdPerson != null && _thirdPerson.enabled;
                    if (_thirdPerson != null) _thirdPerson.enabled = false;
                    if (_characterController != null) _characterController.enabled = false;

                    transform.position = targetPosition;
                    transform.rotation = targetRotation;
                    Physics.SyncTransforms();

                    ResetVerticalVelocityIfPossible();

                    if (_characterController != null) _characterController.enabled = true;
                    if (_thirdPerson != null && tpcWas && IsOwner) _thirdPerson.enabled = true;
                }
            }
        }

        // Llama a ThirdPersonController para resetear su velocidad vertical
        // (que es privada). Si añadimos un setter público en TPC esto será más
        // limpio; por ahora se hace por reflexión solo en este caso de teleport.
        private void ResetVerticalVelocityIfPossible()
        {
            if (_thirdPerson == null) return;
            var field = typeof(ThirdPersonController).GetField(
                "_verticalVelocity",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(_thirdPerson, 0f);
        }
    }
}

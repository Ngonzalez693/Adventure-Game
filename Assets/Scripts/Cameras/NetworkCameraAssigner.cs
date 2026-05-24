using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Componente del prefab del jugador.
// Al spawnear el jugador local (owner), busca el sistema de cámara
// activo en la escena y le asigna este jugador como target.
//
// Sistemas de cámara soportados (pon UNO en la escena):
//   · FixedFollowCamera          → fija detrás, sigue cuando el jugador gira
//   · ThirdPersonCameraController → fija detrás, mouse X gira al jugador
//   · CameraRootFollower         → sigue posición solamente (básica)
public class NetworkCameraAssigner : NetworkBehaviour
{
    [Tooltip("Tiempo máximo (segundos) a esperar a que aparezca un script de " +
             "cámara en la escena. Cubre el caso del HOST cuyo player se crea " +
             "antes de que la escena Lobby termine de cargarse.")]
    public float waitTimeout = 5f;

    public override void OnNetworkSpawn()
    {
        // Solo el jugador local configura SU cámara.
        // Los otros jugadores no tocan la cámara de la escena.
        if (!IsOwner) return;

        // Iniciamos una corrutina porque, al spawnear el host antes de cargar
        // el Lobby, la cámara todavía no existe. Esperamos hasta que aparezca.
        StartCoroutine(AssignCameraWhenReady());

        // Re-asignar la cámara cada vez que se carga una nueva escena
        // (Lobby → GameMap, etc.). El player object persiste entre escenas,
        // pero la cámara y CameraRoot son distintos en cada una.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsOwner) return;
        StartCoroutine(AssignCameraWhenReady());
    }

    private IEnumerator AssignCameraWhenReady()
    {
        float elapsed = 0f;

        while (elapsed < waitTimeout)
        {
            // ── Opción 1: FixedFollowCamera ──────────────────────────────────
            var fixedCam = FindObjectOfType<FixedFollowCamera>();
            if (fixedCam != null)
            {
                fixedCam.SetTarget(transform);
                Debug.Log("[NetworkCameraAssigner] FixedFollowCamera asignada.");
                yield break;
            }

            // ── Opción 2: ThirdPersonCameraController ────────────────────────
            var tpCam = FindObjectOfType<ThirdPersonCameraController>();
            if (tpCam != null)
            {
                tpCam.SetTarget(transform);
                Debug.Log("[NetworkCameraAssigner] ThirdPersonCameraController asignada.");
                yield break;
            }

            // ── Opción 3: CameraRootFollower (básica) ────────────────────────
            var follower = FindObjectOfType<CameraRootFollower>();
            if (follower != null)
            {
                follower.SetTarget(transform);
                Debug.Log("[NetworkCameraAssigner] CameraRootFollower asignada.");
                yield break;
            }

            // Ninguno todavía — esperar un frame y reintentar.
            // Normalmente la escena termina de cargar en 1-2 frames.
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.LogError("[NetworkCameraAssigner] Timeout: no se encontró ningún " +
                       "script de cámara en la escena tras " + waitTimeout + "s.\n" +
                       "Agrega uno a la escena (en el GameObject CameraRoot):\n" +
                       " · FixedFollowCamera\n" +
                       " · ThirdPersonCameraController\n" +
                       " · CameraRootFollower");
    }
}

using Unity.Netcode;
using UnityEngine;

// Componente del prefab del jugador.
// Al spawnear el jugador local (owner), busca el sistema de cámara
// activo en la escena y le asigna este jugador como target.
//
// Sistemas de cámara soportados (pon UNO en la Main Camera):
//   · FixedFollowCamera         → fija detrás, sigue cuando el jugador gira
//   · ThirdPersonCameraController → fija detrás, mouse X gira al jugador
//   · CameraRootFollower         → sigue posición solamente (básica)
public class NetworkCameraAssigner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Solo el jugador local configura SU cámara.
        // Los otros jugadores no tocan la cámara de la escena.
        if (!IsOwner) return;

        // ── Opción 1: FixedFollowCamera ──────────────────────────────────
        var fixedCam = FindObjectOfType<FixedFollowCamera>();
        if (fixedCam != null)
        {
            fixedCam.SetTarget(transform);
            Debug.Log("[NetworkCameraAssigner] FixedFollowCamera asignada.");
            return;
        }

        // ── Opción 2: ThirdPersonCameraController ────────────────────────
        var tpCam = FindObjectOfType<ThirdPersonCameraController>();
        if (tpCam != null)
        {
            tpCam.SetTarget(transform);
            Debug.Log("[NetworkCameraAssigner] ThirdPersonCameraController asignada.");
            return;
        }

        // ── Opción 3: CameraRootFollower (básica) ────────────────────────
        var follower = FindObjectOfType<CameraRootFollower>();
        if (follower != null)
        {
            follower.SetTarget(transform);
            Debug.Log("[NetworkCameraAssigner] CameraRootFollower asignada.");
            return;
        }

        Debug.LogError("[NetworkCameraAssigner] No se encontró ningún script " +
                       "de cámara en la escena. Agrega uno a la Main Camera:\n" +
                       " · FixedFollowCamera\n" +
                       " · ThirdPersonCameraController\n" +
                       " · CameraRootFollower");
    }
}

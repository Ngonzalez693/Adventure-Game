using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    // Lee el input del jugador local y lo envía al servidor para que
    // ThirdPersonController mueva el personaje de forma autoritativa.
    public class NetworkPlayerInput : NetworkBehaviour
    {
        private PlayerInput playerInput;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        public override void OnNetworkSpawn()
        {
            // Solo el dueño del objeto envía input; los demás desactivan su PlayerInput
            if (!IsOwner && playerInput != null)
                playerInput.enabled = false;
        }

        // Llamado por el Input System al mover WASD / stick
        public void Move(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;

            Vector2 input = context.ReadValue<Vector2>();
            float cameraY = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;
            SubmitMoveServerRpc(input, cameraY);
        }

        // Llamado por el Input System al presionar salto
        public void Jump(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;

            if (context.performed)
                SubmitJumpServerRpc();
        }

        // Llamado por el Input System al presionar/soltar sprint
        public void Sprint(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;

            SubmitSprintServerRpc(context.ReadValue<float>() > 0.5f);
        }

        // Envía dirección de movimiento y rotación de cámara al servidor
        [ServerRpc]
        private void SubmitMoveServerRpc(Vector2 input, float cameraY)
        {
            var motor = GetComponent<NetworkCharacterMotor>();
            if (motor != null)
            {
                motor.SetMoveInput(input);
                motor.SetCameraRotation(cameraY);
            }
        }

        [ServerRpc]
        private void SubmitJumpServerRpc()
        {
            var motor = GetComponent<NetworkCharacterMotor>();
            if (motor != null)
                motor.SetJump();
        }

        [ServerRpc]
        private void SubmitSprintServerRpc(bool sprinting)
        {
            var motor = GetComponent<NetworkCharacterMotor>();
            if (motor != null)
                motor.SetSprint(sprinting);
        }
    }
}

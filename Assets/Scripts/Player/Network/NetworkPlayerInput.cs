using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

namespace Player
{
    // Lee el input del jugador local y lo pasa directamente a StarterAssetsInputs,
    // sin depender del modo "Behavior" de PlayerInput ni de defines de compilación.
    // Desactiva PlayerInput en los personajes ajenos para que no los controles.
    public class NetworkPlayerInput : NetworkBehaviour
    {
        private PlayerInput        _playerInput;
        private StarterAssetsInputs _inputs;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _inputs      = GetComponent<StarterAssetsInputs>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                // Personaje ajeno: desactivar input local y liberar dispositivos
                if (_playerInput != null)
                {
                    _playerInput.DeactivateInput();
                    _playerInput.enabled = false;
                }
                return;
            }

            // ── Jugador local: asegurar que PlayerInput esté activo ──────────
            // Esto es crítico cuando hay varias instancias del prefab en la misma
            // máquina (el cliente ve su propio player + la réplica del host).
            // Si la réplica se spawneó antes, puede haber acaparado el dispositivo.
            if (_playerInput != null)
            {
                _playerInput.enabled = true;
                _playerInput.ActivateInput();

                // Forzar que el action map por defecto esté habilitado
                if (_playerInput.actions != null)
                {
                    var map = _playerInput.actions.FindActionMap(
                        string.IsNullOrEmpty(_playerInput.defaultActionMap)
                            ? "Player"
                            : _playerInput.defaultActionMap);
                    if (map != null && !map.enabled)
                        map.Enable();
                }
            }

            SubscribeActions();
        }

        public override void OnNetworkDespawn()
        {
            // Limpiar suscripciones al destruirse para evitar memory leaks
            UnsubscribeActions();
        }

        // ── Suscripciones ─────────────────────────────────────────────────────
        private void SubscribeActions()
        {
            if (_playerInput == null || _playerInput.actions == null) return;

            var move   = _playerInput.actions.FindAction("Move");
            var jump   = _playerInput.actions.FindAction("Jump");
            var sprint = _playerInput.actions.FindAction("Sprint");

            if (move != null)
            {
                move.performed += HandleMove;
                move.canceled  += HandleMove;
            }
            if (jump != null)
            {
                jump.performed += HandleJump;
                jump.canceled  += HandleJump;
            }
            if (sprint != null)
            {
                sprint.performed += HandleSprint;
                sprint.canceled  += HandleSprint;
            }
        }

        private void UnsubscribeActions()
        {
            if (_playerInput == null || _playerInput.actions == null) return;

            var move   = _playerInput.actions.FindAction("Move");
            var jump   = _playerInput.actions.FindAction("Jump");
            var sprint = _playerInput.actions.FindAction("Sprint");

            if (move != null)
            {
                move.performed -= HandleMove;
                move.canceled  -= HandleMove;
            }
            if (jump != null)
            {
                jump.performed -= HandleJump;
                jump.canceled  -= HandleJump;
            }
            if (sprint != null)
            {
                sprint.performed -= HandleSprint;
                sprint.canceled  -= HandleSprint;
            }
        }

        // ── Callbacks de input ────────────────────────────────────────────────
        private void HandleMove(InputAction.CallbackContext ctx)
        {
            if (_inputs == null) return;
            // MoveInput() existe siempre, sin depender de defines
            _inputs.MoveInput(ctx.ReadValue<Vector2>());
        }

        private void HandleJump(InputAction.CallbackContext ctx)
        {
            if (_inputs == null) return;
            _inputs.JumpInput(ctx.phase == InputActionPhase.Performed);
        }

        private void HandleSprint(InputAction.CallbackContext ctx)
        {
            if (_inputs == null) return;
            _inputs.SprintInput(ctx.phase == InputActionPhase.Performed);
        }
    }
}

using Unity.Netcode;
using UnityEngine;

namespace Player
{
    // Sincroniza los parámetros del Animator del jugador a todos los clientes.
    //
    // Cómo funciona:
    //   · El OWNER (jugador local) escribe sus parámetros en NetworkVariables cada frame.
    //   · Los demás clientes leen esas variables y las aplican a su Animator local.
    //
    // Setup:
    //   1. Añade este script al prefab del jugador (mismo GameObject que el Animator).
    //   2. Asegúrate de que el prefab también tiene NetworkTransform para posición/rotación.
    //   3. No necesitas tocar ThirdPersonController: este script lee del Animator
    //      DESPUÉS de que ThirdPersonController ya lo actualizó.
    public class NetworkPlayerAnimator : NetworkBehaviour
    {
        private Animator _animator;

        // ── Parámetros sincronizados ───────────────────────────────────────────
        // WritePermission.Owner = solo el dueño puede escribir; todos pueden leer.
        private readonly NetworkVariable<float> _netSpeed = new(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<float> _netMotionSpeed = new(
            1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<bool> _netGrounded = new(
            true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<bool> _netFreeFall = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // IDs del Animator (mismos que en ThirdPersonController)
        private int _idSpeed;
        private int _idMotionSpeed;
        private int _idGrounded;
        private int _idFreeFall;

        // ── Ciclo de vida ──────────────────────────────────────────────────────
        private void Awake()
        {
            _animator = GetComponent<Animator>();

            _idSpeed       = Animator.StringToHash("Speed");
            _idMotionSpeed = Animator.StringToHash("MotionSpeed");
            _idGrounded    = Animator.StringToHash("Grounded");
            _idFreeFall    = Animator.StringToHash("FreeFall");
        }

        private void Update()
        {
            if (_animator == null) return;

            if (IsOwner)
            {
                // ── Owner: leer los valores que ThirdPersonController ya escribió
                //    en el Animator y publicarlos para el resto de clientes.
                _netSpeed.Value       = _animator.GetFloat(_idSpeed);
                _netMotionSpeed.Value = _animator.GetFloat(_idMotionSpeed);
                _netGrounded.Value    = _animator.GetBool(_idGrounded);
                _netFreeFall.Value    = _animator.GetBool(_idFreeFall);
            }
            else
            {
                // ── Otros clientes: aplicar los valores de red al Animator.
                //    ThirdPersonController está desactivado en non-owners
                //    (NetworkCharacterMotor lo desactiva), así que este script
                //    es el único que escribe en el Animator aquí.
                _animator.SetFloat(_idSpeed,       _netSpeed.Value);
                _animator.SetFloat(_idMotionSpeed, _netMotionSpeed.Value);
                _animator.SetBool (_idGrounded,    _netGrounded.Value);
                _animator.SetBool (_idFreeFall,    _netFreeFall.Value);
            }
        }
    }
}

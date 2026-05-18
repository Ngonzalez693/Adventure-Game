using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class ThirdPersonController : MonoBehaviour
	{
		[Header("Player")]
		public float MoveSpeed = 2.0f;
		public float SprintSpeed = 5.335f;
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		public float JumpHeight = 1.2f;
		public float Gravity = -15.0f;

		[Space(10)]
		public float JumpTimeout = 0.50f;
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		public bool Grounded = true;
		public float GroundedOffset = -0.14f;
		public float GroundedRadius = 0.28f;
		public LayerMask GroundLayers;

		// Mantenemos estos campos para que no se rompan referencias en el Inspector,
		// pero ya NO los usamos para rotar la cámara (Cinemachine lo hace)
		[Header("Cinemachine")]
		[Tooltip("Asigna aquí el CameraRoot hijo del Player")]
		public GameObject CinemachineCameraTarget;
		public float TopClamp = 70.0f;
		public float BottomClamp = -30.0f;
		public float CameraAngleOverride = 0.0f;
		public bool LockCameraPosition = false;

		// player
		private float _speed;
		private float _animationBlend;
		private float _targetRotation = 0.0f;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		// Edge-trigger para la tecla S: convierte "ir hacia atrás" en un giro de 180°
		// que solo se dispara al PRESIONAR (no mientras se mantiene apretada).
		private bool _sWasHeld;
		private bool _isTurning;        // true mientras el giro animado está en curso
		public float TurnDuration = 0.3f; // segundos que dura la animación del giro

		// animation IDs
		private int _animIDSpeed;
		private int _animIDGrounded;
		private int _animIDJump;
		private int _animIDFreeFall;
		private int _animIDMotionSpeed;

		private Animator _animator;
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		// Asignado por NetworkCharacterMotor para simular la cámara en el servidor
		[HideInInspector] public Transform networkCameraOverride;

		private const float _threshold = 0.01f;
		private bool _hasAnimator;

		private void Awake()
		{
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			// El Animator está en un hijo (SK_Military_Survivalist), no en el root.
			_animator = GetComponentInChildren<Animator>();
			_hasAnimator = _animator != null;
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();

			AssignAnimationIDs();

			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			// No buscamos el Animator cada frame (costoso). Solo verificamos que sigue válido.
			if (_animator == null) { _animator = GetComponentInChildren<Animator>(); _hasAnimator = _animator != null; }

			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		private void LateUpdate()
		{
			// ✅ ELIMINADO: Ya no llamamos CameraRotation() aquí.
			// Cinemachine Pan Tilt + Input Axis Controller manejan la rotación.
		}

		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDGrounded = Animator.StringToHash("Grounded");
			_animIDJump = Animator.StringToHash("Jump");
			_animIDFreeFall = Animator.StringToHash("FreeFall");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

		private void GroundedCheck()
		{
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

			if (_hasAnimator)
			{
				_animator.SetBool(_animIDGrounded, Grounded);
			}
		}

		private void Move()
		{
			// ── Tecla S = giro animado de 180° (no movimiento hacia atrás) ─────
			// Solo se dispara al PRESIONAR la tecla (edge). Mientras el giro
			// está en curso no se puede volver a disparar.
			bool sHeld = _input.move.y < -0.1f;
			if (sHeld && !_sWasHeld && !_isTurning)
				StartCoroutine(SmoothTurn180());
			_sWasHeld = sHeld;

			// Anulamos el componente hacia atrás: S ya no camina, solo gira.
			// También bloqueamos movimiento mientras el giro está animándose.
			if (_input.move.y < 0f || _isTurning)
				_input.move = new Vector2(0f, 0f);

			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			if (_input.move != Vector2.zero)
			{
				// Usa networkCameraOverride en servidor (sin MainCamera); cámara real en cliente
				Transform camTransform = networkCameraOverride != null
					? networkCameraOverride
					: _mainCamera != null ? _mainCamera.transform : null;
				float camY = camTransform != null ? camTransform.eulerAngles.y : 0f;

				_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + camY;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
			}
		}

		// Rota suavemente al jugador 180° sobre el eje Y en TurnDuration segundos.
		// Usa una curva ease-in/out para que se vea natural.
		private IEnumerator SmoothTurn180()
		{
			_isTurning = true;

			float startY  = transform.eulerAngles.y;
			float targetY = startY + 180f;
			float elapsed = 0f;

			while (elapsed < TurnDuration)
			{
				elapsed += Time.deltaTime;
				float t = Mathf.Clamp01(elapsed / TurnDuration);
				// Ease in-out suave: aceleración y desaceleración
				t = t * t * (3f - 2f * t);

				float currentY = Mathf.LerpAngle(startY, targetY, t);
				transform.rotation = Quaternion.Euler(0f, currentY, 0f);

				// Sincronizamos _targetRotation para que SmoothDampAngle
				// no luche contra el giro cuando se reanude el movimiento.
				_targetRotation    = currentY;
				_rotationVelocity  = 0f;
				yield return null;
			}

			// Aseguramos el ángulo final exacto
			transform.rotation = Quaternion.Euler(0f, targetY, 0f);
			_targetRotation    = targetY;
			_isTurning = false;
		}

		private void JumpAndGravity()
		{
			// Salto desactivado: ignoramos _input.jump por completo.
			// Mantenemos la gravedad y la lógica de caída para que el
			// personaje pueda caer de plataformas / pendientes.
			_input.jump = false;

			if (Grounded)
			{
				_fallTimeoutDelta = FallTimeout;

				if (_hasAnimator)
				{
					_animator.SetBool(_animIDJump, false);
					_animator.SetBool(_animIDFreeFall, false);
				}

				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				_jumpTimeoutDelta = JumpTimeout;

				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}
				else
				{
					if (_hasAnimator)
					{
						_animator.SetBool(_animIDFreeFall, true);
					}
				}

				_input.jump = false;
			}

			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}
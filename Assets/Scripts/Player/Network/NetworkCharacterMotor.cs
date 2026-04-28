using Unity.Netcode;
using UnityEngine;
using StarterAssets;

namespace Player
{
    // Requiere CharacterController porque ThirdPersonController lo necesita
    [RequireComponent(typeof(CharacterController))]
    public class NetworkCharacterMotor : NetworkBehaviour
    {
        private StarterAssetsInputs _inputs;
        private ThirdPersonController _thirdPerson;

        // Transform hijo creado en Awake que simula la cámara en el servidor
        // ThirdPersonController lo usa para saber hacia dónde rotar al personaje
        private Transform _fakeCameraRoot;

        private void Awake()
        {
            _inputs = GetComponent<StarterAssetsInputs>();
            _thirdPerson = GetComponent<ThirdPersonController>();

            // Creamos un objeto hijo vacío que actúa como cámara virtual en el servidor
            var fakeGO = new GameObject("FakeCameraRoot");
            fakeGO.transform.SetParent(transform);
            fakeGO.transform.localPosition = Vector3.zero;
            _fakeCameraRoot = fakeGO.transform;

            if (_thirdPerson != null)
                _thirdPerson.networkCameraOverride = _fakeCameraRoot;
        }

        public override void OnNetworkSpawn()
        {
            // Solo el servidor mueve el personaje con ThirdPersonController
            // En los clientes, NetworkTransform sincroniza posición y rotación
            if (!IsServer && _thirdPerson != null)
                _thirdPerson.enabled = false;
        }

        // Llamado por NetworkPlayerInput vía ServerRpc
        public void SetMoveInput(Vector2 input)
        {
            if (_inputs != null)
                _inputs.move = input;
        }

        public void SetJump()
        {
            if (_inputs != null)
                _inputs.jump = true;
        }

        public void SetSprint(bool sprinting)
        {
            if (_inputs != null)
                _inputs.sprint = sprinting;
        }

        // Recibe la rotación Y de la cámara del cliente para que el servidor
        // pueda rotar el personaje en la dirección correcta
        public void SetCameraRotation(float cameraYRotation)
        {
            if (_fakeCameraRoot != null)
                _fakeCameraRoot.rotation = Quaternion.Euler(0f, cameraYRotation, 0f);
        }
    }
}

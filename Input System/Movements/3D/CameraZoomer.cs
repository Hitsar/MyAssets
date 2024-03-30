using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Player
{
    public class CameraZoomer : MonoBehaviour
    {
        [SerializeField] private float _zoomSpeed = 4;
        [SerializeField] private float _moveAwaySpeed = 1;
        [SerializeField] private float _cameraDistanceToPlayer;
        [SerializeField] private float _maxCameraDistanceToPlayer = 20;
        [SerializeField] private float _minFOV = 20;
        [SerializeField] private float _maxFOV = 60;
        
        private Camera _camera;
        private InputSystem _inputSystem;

        private void Start()
        {
            _inputSystem = new InputSystem();
            _inputSystem.Player.Zoom.performed += Zoom;
            _inputSystem.Player.Enable();

            _camera = Camera.main;
        }

        private void LateUpdate() => _camera.transform.position = transform.parent.position - transform.parent.forward * _cameraDistanceToPlayer;

        private void Zoom(InputAction.CallbackContext callbackContext)
        {
            float value = callbackContext.ReadValue<float>() > 0 ? 1 : -1;
            float lastDistance = _cameraDistanceToPlayer;
            
            if (_camera.fieldOfView == _maxFOV) _cameraDistanceToPlayer = Mathf.Clamp(_cameraDistanceToPlayer - value * _moveAwaySpeed, 0, _maxCameraDistanceToPlayer);
            if (_cameraDistanceToPlayer == 0 && lastDistance == 0) _camera.fieldOfView = value == 1 ? _minFOV : _maxFOV;
        }
        
        private void OnDestroy()
        {
            _inputSystem.Player.Zoom.performed -= Zoom;
            _inputSystem.Player.Disable();
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

namespace CupOHappiness
{
    public sealed class StylesShowcaseFocus : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform[] focusTargets;
        [SerializeField] private Vector3 cameraOffset = new(0f, 2.5f, -12f);
        [SerializeField] private float followSpeed = 8f;

        private InputAction _move;
        private int _currentIndex;

        private void OnEnable()
        {
            _move = inputActions?.FindAction("Player/Move", false);
            if (_move == null)
                return;

            _move.performed += OnMove;
            _move.Enable();
        }

        private void OnDisable()
        {
            if (_move == null)
                return;

            _move.performed -= OnMove;
            _move.Disable();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<Vector2>().x;
            if (direction < -0.5f)
                _currentIndex = (_currentIndex - 1 + focusTargets.Length) % focusTargets.Length;
            else if (direction > 0.5f)
                _currentIndex = (_currentIndex + 1) % focusTargets.Length;
        }

        private void LateUpdate()
        {
            if (focusTargets == null || focusTargets.Length == 0 || focusTargets[_currentIndex] == null)
                return;

            var target = focusTargets[_currentIndex];
            var destination = target.position + cameraOffset;
            transform.position = Vector3.Lerp(transform.position, destination, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(target.position - transform.position), followSpeed * Time.deltaTime);
        }
    }
}
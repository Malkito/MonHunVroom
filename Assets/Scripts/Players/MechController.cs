using LordBreakerX.Utility;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MechController : MonoBehaviour
{
    [Header("Input Properties")]
    [SerializeField]
    [Tooltip("The asset used to get the different inputs for the mech")]
    private InputActionAsset _actions;

    [Header("Mech Parts Properties")]
    [SerializeField]
    [Tooltip("The top half of the mech")]
    private Transform _turso;
    [SerializeField]
    [Tooltip("The Bottom half of the mech")]
    private Transform _hips;

    [Header("Camera Properties")]
    [SerializeField]
    [Tooltip("The Camera the mech controls")]
    private CinemachineOrbitalFollow _camera;

    [Header("Movement Properties")]
    [SerializeField]
    [Tooltip("The speed / distance that the mech moves per second")]
    private float _speed;

    [Header("Weapon Properties")]
    [SerializeField]
    [Tooltip("The points of the mech where projectiles get shot from")]
    private Transform[] _barrelEnds;
    [SerializeField]
    [Tooltip("The max angle that the barrels of the mech can rotate")]
    private float _maxAngle;

    [Header("Animation Properties")]
    [SerializeField]
    [Tooltip("The animator used for controlling the animation of the mech")]
    private Animator _animator;

    private Rigidbody _rigidbody;

    private InputActionMap _inputMap;

    private InputAction _movementAction;

    private Vector2 _movementInput;

    private Transform _cameraTransform;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _cameraTransform = _camera.transform;
    }

    private void OnEnable()
    {
        // gets the player input and if found successfuly enables it
        _inputMap = _actions.GetInputMap("Player");

        if (_inputMap == null) return;

        _inputMap.Enable();

        _movementAction = _inputMap.GetInputAction("Movement");

        InputUtility.ActivateInputAction(_movementAction, OnMovementInput);
    }

    private void OnDisable()
    {
        // disables the player input 
        InputUtility.DeactivateInputAction(_movementAction, OnMovementInput);

        _inputMap.Disable();
    }

    // updates the movement input when ever the input is pressed or released
    private void OnMovementInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _movementInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled) _movementInput = Vector3.zero;
    }

    private void Update()
    {
        Move();
        RotateTorso();
        RotateBarrelEnds();
    }

    private void Move() // Handles the movement of the player based off the camera postion
    {
        if (_movementInput.magnitude >= 0.1f) // checks if any input is being pressed
        {
            _animator.SetBool("Walking", true); 

            Vector3 moveDir = GetCameraRelativeMovement();

            // multiplies above direction by max spped
            _rigidbody.linearVelocity = new Vector3(moveDir.x * _speed, _rigidbody.linearVelocity.y, moveDir.z * _speed); 
            _hips.forward = moveDir; 

        }
        else
        {
            _animator.SetBool("Walking", false);
        }
        _turso.rotation = Quaternion.Euler(0, _turso.rotation.y, 0); // enusures top half of player doesnt rotate in werid ways

    }

    private Vector3 GetCameraRelativeMovement()
    {
        Vector3 forward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
        return (right * _movementInput.x) + (forward * _movementInput.y);
    }

    private void RotateTorso()
    {
        _turso.rotation = Quaternion.Euler(_turso.parent.rotation.x, _camera.HorizontalAxis.Value, _turso.rotation.z);
    }

    private void RotateBarrelEnds()
    {
        foreach (Transform barrelEnds in _barrelEnds)
        {
            float angle = (_camera.VerticalAxis.Value * (_maxAngle * 2)) - _maxAngle; // calculates desired angle
            barrelEnds.localRotation = Quaternion.Euler(angle, barrelEnds.rotation.y, barrelEnds.rotation.z);// rotates the barrel ends based of the vertical axis of the camera
        }
    }
}

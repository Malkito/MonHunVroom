using UnityEngine;

public class WheelRotation : MonoBehaviour
{
    [Header("Vehicle")]
    [SerializeField] private Rigidbody tankRigidbody;

    [Header("Wheel Settings")]
    [SerializeField] private float wheelRadius = 0.35f;
    [SerializeField] private Vector3 spinAxis = Vector3.right;

    [Header("Wheel Side")]
    [Tooltip("True if this wheel is on the left side of the tank")]
    [SerializeField] private bool isLeftWheel;

    [Header("Pivot Turning")]
    [SerializeField] private float pivotSpinSpeed = 4f;

    private void Update()
    {
        if (tankRigidbody == null) return;

        Vector2 inputVector = GameInput.instance.getMovementInputNormalized();

        // Forward/backward velocity
        Vector3 velocity = tankRigidbody.linearVelocity;
        float forwardSpeed = Vector3.Dot(velocity, tankRigidbody.transform.forward);

        float rotationAmount;

        // If tank is not moving forward/back, allow pivot spinning
        if (Mathf.Abs(forwardSpeed) < 0.05f && Mathf.Abs(inputVector.x) > 0.01f)
        {
            float direction = inputVector.x;

            // Left wheels opposite of right wheels
            float sideMultiplier = isLeftWheel ? -1f : 1f;

            rotationAmount = direction * sideMultiplier * pivotSpinSpeed * Mathf.Rad2Deg * Time.deltaTime;
        }
        else
        {
            // Normal movement rotation
            float angularVelocity = forwardSpeed / wheelRadius;
            rotationAmount = angularVelocity * Mathf.Rad2Deg * Time.deltaTime;
        }

        transform.Rotate(spinAxis, -rotationAmount, Space.Self);
    }
}

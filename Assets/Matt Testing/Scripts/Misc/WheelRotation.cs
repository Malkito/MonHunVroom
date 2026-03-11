using UnityEngine;

public class WheelRotation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetObject;
    [SerializeField] private Rigidbody targetRigidbody;

    [Header("Wheel Settings")]
    [SerializeField] private float wheelRadius = 0.35f;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;

    [Header("Movement Direction")]
    [Tooltip("Direction the vehicle moves relative to the wheel (usually forward)")]
    [SerializeField] private Vector3 movementDirection = Vector3.forward;

    private void Update()
    {
        if (targetRigidbody == null) return;

        // Velocity of the vehicle at THIS wheel position
        Vector3 wheelVelocity = targetRigidbody.GetPointVelocity(transform.position);

        // Convert to the wheel's local space
        Vector3 localVelocity = transform.InverseTransformDirection(wheelVelocity);

        // Speed in the movement direction (forward/back)
        float speed = Vector3.Dot(localVelocity, movementDirection);

        // Convert linear speed -> angular speed
        float angularVelocity = speed / wheelRadius;

        // Convert radians to degrees
        float rotationAmount = angularVelocity * Mathf.Rad2Deg * Time.deltaTime;

        transform.Rotate(rotationAxis, rotationAmount, Space.Self);
    }
}

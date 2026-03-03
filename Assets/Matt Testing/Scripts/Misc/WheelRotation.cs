using UnityEngine;

public class WheelRotation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetObject;     // Object whose speed we track
    [SerializeField] private Rigidbody targetRigidbody;  // Optional (recommended)

    [Header("Wheel Settings")]
    [SerializeField] private float wheelRadius = 0.35f;  // In meters
    [SerializeField] private Vector3 rotationAxis = Vector3.right;

    private Vector3 lastPosition;

    private void Start()
    {
        if (targetObject != null)
            lastPosition = targetObject.position;
    }

    private void Update()
    {
        ///if (GameInput.instance.getMovementInputNormalized().magnitude == 0) return;

        float speed = 0f;

        // If Rigidbody provided, use velocity (more accurate)
        if (targetRigidbody != null)
        {
            speed = targetRigidbody.linearVelocity.magnitude;
        }
        else if (targetObject != null)
        {
            // Fallback: calculate speed manually
            Vector3 delta = targetObject.position - lastPosition;
            speed = delta.magnitude / Time.deltaTime;
            lastPosition = targetObject.position;
        }

        // Convert linear speed to angular speed
        // ? = v / r
        float angularVelocity = speed / wheelRadius; // radians per second

        // Convert to degrees per frame
        float rotationAmount = angularVelocity * Mathf.Rad2Deg * Time.deltaTime;

        transform.Rotate(rotationAxis, rotationAmount, Space.Self);

    }
}

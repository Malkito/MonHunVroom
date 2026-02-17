using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
public class NewTankMovement : NetworkBehaviour
{

    /// <summary>
    /// 
    /// The basic movement for the tank player. Script goes on player object
    /// 
    /// </summary>

    private Rigidbody rb;

    public bool canMove;

    [SerializeField] private groundedCheck groundCheck;

    private bool isGrounded;

    [SerializeField] private float linerDampening;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f; // degrees per second

    [Header("Rotation")]
    [SerializeField] private float minRotationSpeed = 90f;     // degrees per second
    [SerializeField] private float maxRotationSpeed = 720f;    // degrees per second
    [SerializeField] private float moveAngleThreshold = 5f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("0 = start of turn, 1 = end of turn")]
    [SerializeField]
    private AnimationCurve rotationDampeningCurve =
       AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float totalTurnAngle;
    private Quaternion targetRotation;
    private Vector3 desiredMoveDirection;
    private bool isTurning;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        canMove = true;
    }

    //X = left and right
    //y = up and down
    private void FixedUpdate()
    {
        if (!IsOwner) return; // Built in network check

        if (!canMove) return; // checks if the player can move. (Set to false when dead)

        if (isGrounded) // ensures that inputs are only checked if the tank is on the ground
        {

            Vector2 inputVector = GameInput.instance.getMovementInputNormalized();
            Move(inputVector);

        }


    }

    public void Move(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
            return;

        // --- Camera-relative direction ---
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        desiredMoveDirection = (camForward * input.y + camRight * input.x).normalized;

        targetRotation = Quaternion.LookRotation(desiredMoveDirection, Vector3.up);

        float currentAngle = Quaternion.Angle(transform.rotation, targetRotation);

        // Initialize turn if starting new direction
        if (!isTurning)
        {
            totalTurnAngle = currentAngle;
            isTurning = true;
        }

        if (currentAngle > 0.1f)
        {
            // Normalize turn progress (0 → 1)
            float progress = 1f - Mathf.Clamp01(currentAngle / totalTurnAngle);

            // Evaluate curve
            float curveValue = rotationDampeningCurve.Evaluate(progress);

            // Lerp between min and max rotation speed
            float currentRotationSpeed = Mathf.Lerp(minRotationSpeed, maxRotationSpeed, curveValue);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                currentRotationSpeed * Time.deltaTime
            );
        }
        else
        {
            isTurning = false;
        }

        // Move only when mostly facing direction
        if (currentAngle <= moveAngleThreshold)
        {
            transform.position += desiredMoveDirection * moveSpeed * Time.deltaTime;
        }

        print("Current Angle: " + currentAngle);
    }



    //Simple ground checks. 

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}

using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using UnityEngine.UI;
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
    [SerializeField] private float accelerationForce = 40f;
    [SerializeField] private float maxSpeed = 6f;

    [Header("Rotation")]
    [SerializeField] private float minTurnTorque = 5f;
    [SerializeField] private float maxTurnTorque = 40f;
    [SerializeField] private float moveAngleThreshold = 5f;
    [SerializeField] private float maxConsideredAngle = 180f; // angle where torque peaks
    [SerializeField] private float AngleCorrection;
    [SerializeField] private float OvershootPrevention;

    [Header("Jump")]
    [SerializeField] private float jumpFrontForce;
    [SerializeField] private float jumpUpForce;
    [SerializeField] private float MaxJumpTimer;
    private float jumpTimer;




    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Image jumpIcon;
    private Vector3 desiredMoveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        canMove = true;
    }

    //X = left and right
    //y = up and down
    private void FixedUpdate()
    {
        Debug.Log("Fixed update running");

        if (!IsOwner) return; // Built in network check

        if (!canMove) return; // checks if the player can move. (Set to false when dead)

        if (isGrounded) // ensures that inputs are only checked if the tank is on the ground
        {

            Vector2 inputVector = GameInput.instance.getMovementInputNormalized();
            Move(inputVector);

            bool jumpInput = GameInput.instance.getJumpInput();
            jump(jumpInput);


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

        Vector3 desiredDirection = (camForward * input.y + camRight * input.x).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);

        float angle = Quaternion.Angle(rb.rotation, targetRotation);

        // -------------------------
        // Smooth Torque (stable)
        // -------------------------
        Vector3 cross = Vector3.Cross(transform.forward, desiredDirection);
        float angleError = Mathf.Asin(Mathf.Clamp(cross.y, -1f, 1f));
        // signed small-angle error in radians

        float angularVelocityY = rb.angularVelocity.y;

        // PD gains (tune these)
        float kp = AngleCorrection;   // proportional (how strongly it corrects angle)
        float kd = OvershootPrevention;    // damping (prevents overshoot)

        // PD control law
        float torque = (kp * angleError) - (kd * angularVelocityY);

        rb.AddTorque(Vector3.up * torque, ForceMode.Acceleration);
        // -------------------------
        // Acceleration Scales With Alignment
        // -------------------------

        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float speedInDesiredDirection = Vector3.Dot(planarVelocity, desiredDirection);

        float alignment = Mathf.Clamp01(Vector3.Dot(transform.forward, desiredDirection));

        float scaledAcceleration = accelerationForce * alignment;

        // Only limit speed along the movement direction
        if (speedInDesiredDirection < maxSpeed)
        {

            rb.AddForce(desiredDirection * scaledAcceleration, ForceMode.Acceleration);
        }
    }


    private void jump(bool input)
    {


        if(input && jumpTimer <= 0)
        {
            Vector3 Frontforce = transform.forward * jumpFrontForce;
            Vector3 upforce = Vector3.up * jumpUpForce;
            rb.AddForce(Frontforce + upforce, ForceMode.VelocityChange);
            jumpTimer = MaxJumpTimer;

            setColor(0.5f);

        }
        else
        {
            if (jumpTimer <= 0)
            {
                setColor(1f);
                return;
            }
            jumpTimer -= Time.deltaTime;


        }
    }


    private void setColor(float alphaValue)
    {
        Color color = jumpIcon.color;
        color.a = alphaValue;
        jumpIcon.color = color;


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


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Vector3 start = transform.position;
        Vector3 end = start + desiredMoveDirection * 100;
        Gizmos.DrawLine(start, end);
    }

}

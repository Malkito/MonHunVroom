using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class tankMovement : NetworkBehaviour
{

    /// <summary>
    /// 
    /// The basic movement for the tank player. Script goes on player object
    /// 
    /// </summary>


    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;

    [Header("Friction")]
    [SerializeField] private bool useSidewaysFriction;
    [SerializeField] private float sidewaysFriction;

    private Rigidbody rb;

    public bool canMove;

    [SerializeField] private groundedCheck groundCheck;

    private bool isGrounded;

    [SerializeField] private float linerDampening;

    [Header("Jump")]
    [SerializeField] private float jumpFrontForce;
    [SerializeField] private float jumpUpForce;
    [SerializeField] private float MaxJumpTimer;
    [SerializeField] private Image jumpIcon;
    private float jumpTimer;

    private playerStats PlayerStats;
    public override void OnNetworkSpawn()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        PlayerStats = GetComponent<playerStats>();
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

            forwardAndBackMovemnet(inputVector.y);

            rotateBody(inputVector.x);

            ApplySidewaysFriction();
            jump(GameInput.instance.getJumpInput());
        }
    }

    private void rotateBody(float inputVector)
    {
        Quaternion rotation = Quaternion.Euler(0f, rotationSpeed * Time.fixedDeltaTime * inputVector, 0f);

        rb.MoveRotation(rb.rotation * rotation);

    }

 
    private void forwardAndBackMovemnet(float inputVector)
    {
        rb.AddForce(gameObject.transform.forward * (moveSpeed + PlayerStats.currentSpeed) * inputVector, ForceMode.Acceleration);

        if(inputVector == 0 && isGrounded)
        {
            rb.linearDamping = linerDampening;

        }
        else
        {
            rb.linearDamping = 0;
        }
    }


    private void ApplySidewaysFriction()
    {
        if (!useSidewaysFriction) return;

        Vector3 velocity = rb.linearVelocity;

        Vector3 forward = transform.forward;
        Vector3 sideways = transform.right;

        float forwardVel = Vector3.Dot(velocity, forward);
        float sidewaysVel = Vector3.Dot(velocity, sideways);

        // Reduce sideways sliding
        sidewaysVel = Mathf.Lerp(sidewaysVel, 0f, sidewaysFriction * Time.fixedDeltaTime);

        Vector3 correctedVelocity = forward * forwardVel + sideways * sidewaysVel;

        rb.linearVelocity = new Vector3(correctedVelocity.x, velocity.y, correctedVelocity.z);
    }

    private void jump(bool input)
    {

        if (input && jumpTimer <= 0)
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

    private void setColor(float alphaValue)
    {
        Color color = jumpIcon.color;
        color.a = alphaValue;
        jumpIcon.color = color;
    }

}

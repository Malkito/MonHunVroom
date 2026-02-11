using UnityEngine;
using Unity.Netcode;

public class tankMovement : NetworkBehaviour
{

    /// <summary>
    /// 
    /// The basic movement for the tank player. Script goes on player object
    /// 
    /// </summary>


    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;

    private Rigidbody rb;

    public bool canMove;

    [SerializeField] private groundedCheck groundCheck;

    private bool isGrounded;

    [SerializeField] private float linerDampening;

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

            forwardAndBackMovemnet(inputVector.y);

            rotateBody(inputVector.x);
        }
    }

    private void rotateBody(float inputVector)
    {
        Quaternion rotation = Quaternion.Euler(0f, rotationSpeed * Time.fixedDeltaTime * inputVector, 0f);

        rb.MoveRotation(rb.rotation * rotation);

    }

 
    private void forwardAndBackMovemnet(float inputVector)
    {
        rb.AddForce(gameObject.transform.forward * moveSpeed * inputVector);

        if(inputVector == 0 && isGrounded)
        {
            rb.linearDamping = linerDampening;

        }
        else
        {
            rb.linearDamping = 0;
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

}

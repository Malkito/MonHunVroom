using UnityEngine;
using Unity.Netcode;

public class tankMovement : NetworkBehaviour
{
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
        if (!IsOwner) return;
        if (!canMove) return;

        if (isGrounded)
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

        if(inputVector == 0)
        {
            rb.linearDamping = linerDampening;

        }
        else
        {
            rb.linearDamping = 0;
        }
    }



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

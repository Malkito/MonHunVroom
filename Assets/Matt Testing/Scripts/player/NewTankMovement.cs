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


    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;

    private Rigidbody rb;

    public bool canMove;

    [SerializeField] private groundedCheck groundCheck;

    private bool isGrounded;

    [SerializeField] private float linerDampening;

    [SerializeField] private CinemachineCamera camera;

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

        }
    }

    private void move(Vector2 input)
    {
        


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

using UnityEngine;
using Unity.Cinemachine;
using Unity.Netcode;
public class MulltiplayerMechMovement : NetworkBehaviour
{


    [Header("Turret Rotation")]
    [SerializeField] private GameObject torso; // Top half of player

    [Header("Camera")]
    [SerializeField] private CinemachineCamera cam;
    public Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private GameObject hips; // lower half of player
    [SerializeField] private float speed; // max speed
    private float VerticalInput;
    private float HorizontalInput;

    [Header("Barrel Ends")]
    [SerializeField] private GameObject[] barrelEnds; // Two barrel ends, one at the end of each arm
    [SerializeField] private float maxAngle;


    [Header("Other")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator anim;
    [SerializeField] private GameInput gameInput;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        if (IsOwner)
        {
            cam.Priority = 5;
        }
        else
        {
            cam.Priority = 3;
        }
    }

    /// MUltiplayer
    /*
    private void FixedUpdate()
    {
        if (IsOwner || IsLocalPlayer) {
            HorizontalInput = Input.GetAxis("Horizontal");
            VerticalInput = Input.GetAxis("Vertical");
            movementServerRpc();
            RotateTorsoServerRpc();
            RotateBarrelEndsServerRpc();
        }
    }
    */

    private void FixedUpdate()
    {
        Vector2 inputVector = gameInput.getMovementInputNormalized();
        movement(inputVector);
        rotateTorso();
        rotatioBarrelEnds();
    }

    private void movement(Vector2 inputVector) // Handles the movement of the player based off the camera postion
    {

        Vector3 direction = new Vector3(inputVector.x, 0f, inputVector.y).normalized; // reads input, while no input, vector equals 0

        if (direction.magnitude >= 0.1f) // checks if any input is being pressed
        {
            anim.SetBool("Walking", true); // starts ainmation

            Vector3 cameraForward = cameraTransform.forward; // checks the forward direction of the camera
            cameraForward.y = 0; // ignores tilt
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;// checks the right direction of the camera
            cameraRight.y = 0; // ignores tilt
            cameraRight.Normalize();

            Vector3 moveDir = (cameraRight * inputVector.x) + (cameraForward * inputVector.y); // writes the direction of the camera multiplied by the input to a direction
            moveDir.Normalize();

            rb.linearVelocity = new Vector3(moveDir.x * speed, rb.linearVelocity.y, moveDir.z * speed); // multiplies above direction by max spped
            hips.transform.forward = moveDir;// sets the lower half of player to face direction its moving
            hips.transform.rotation = Quaternion.Euler(0, hips.transform.rotation.y, 0);

        }
        else
        {
            anim.SetBool("Walking", false); // Stops animation
        }
        torso.transform.rotation = Quaternion.Euler(0, torso.transform.rotation.y, 0); // enusures top half of player doesnt rotate in werid ways

    }

    private void rotateTorso()
    {
        torso.transform.rotation = Quaternion.Euler(torso.transform.parent.rotation.x, cam.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value, torso.transform.rotation.z); // reads the horizontal axis of the camera and rotates the top hlaf of player accordingly

    }

    private void rotatioBarrelEnds()
    {
        foreach (GameObject barrelEndTransform in barrelEnds)
        {
            float angle = (cam.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value * (maxAngle * 2)) - maxAngle; // calculates desired angle for barrel rotation, only considered up and down
            barrelEndTransform.transform.localRotation = Quaternion.Euler(angle, barrelEndTransform.transform.rotation.y, barrelEndTransform.transform.rotation.z);// rotates the barrel ends based of the vertical axis of the camera

        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void movementServerRpc()
    {
       // movement();
    }
    [ServerRpc(RequireOwnership = false)]
    private void RotateTorsoServerRpc()
    {
        rotateTorso();
    }
    [ServerRpc(RequireOwnership = false)]
    private void RotateBarrelEndsServerRpc()
    {
        rotatioBarrelEnds();
    }

}

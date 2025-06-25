using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine.UI;

public class goofyPlayerMoverment : NetworkBehaviour
{
    [Header("Turret Rotation")]
    [SerializeField] private GameObject Torso;
    [SerializeField] private float rotateSpeed;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera cam;
    public Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float BaseSpeed; // max Base speed
    [SerializeField] private float CurrentSpeed; // Current speed

    [Header("Barrel Ends")]
    [SerializeField] private GameObject[] barrelEnds; // Two barrel ends, one at the end of each arm
    [SerializeField] private float maxAngle;


    [Header("Sprint Stats")]
    [SerializeField] private float sprintSpeedMultiplier;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaDeplationRate;
    [SerializeField] private float staminaRegenRate;
    private float currentStamina;
    private bool canRegen;
    private bool canSprint;
    [SerializeField] private float RegenDelayTime;
    [SerializeField] private float minStaminaPercentageToSprint;

    [Header("Other")]
    [SerializeField] private Rigidbody rb;
    //[SerializeField] private Animator anim;
    public bool canMove;
    [SerializeField] private bool IsSinglePlayer;


    private void Start()
    {
        canMove = true;
        canSprint = true;
        currentStamina = maxStamina;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

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
        if (!IsOwner) return;
        if (!canMove) return;


        staminaSlider.value = currentStamina / maxStamina;
        Vector2 inputVector = GameInput.instance.getMovementInputNormalized();
        movement(inputVector);
        print(inputVector);
        rotatioBarrelEnds();

        if (GameInput.instance.getSprintInput() && currentStamina >= 0 && canSprint)
        {
            isSprinting();
        }
        else
        {
            isNotSprinting();
        }
    }


    private void isSprinting() // Is Sprinting
    {
        CurrentSpeed = BaseSpeed * sprintSpeedMultiplier;
        currentStamina -= Time.deltaTime * staminaDeplationRate;
        if (currentStamina <= 0)
        {
            canSprint = false;
            StartCoroutine(sprintRegenDelay(RegenDelayTime));
        }
        else
        {
            canRegen = true;
        }

    }

    private void isNotSprinting() // Is not Sprinting
    {
        CurrentSpeed = BaseSpeed;
        if (canRegen) currentStamina += Time.deltaTime * staminaRegenRate;
        if (currentStamina > (maxStamina * (minStaminaPercentageToSprint / 100)))
        {
            canSprint = true;
        }
    }


    IEnumerator sprintRegenDelay(float delay)
    {
        canRegen = false;
        yield return new WaitForSeconds(delay);
        canRegen = true;
    }



    private void movement(Vector2 inputVector) // Handles the movement of the player based off the camera postion
    {
        Vector3 direction = new Vector3(inputVector.x, 0f, inputVector.y).normalized; // reads input, while no input, vector equals 0

        if (direction.magnitude >= 0.1f) // checks if any input is being pressed
        {
            //anim.SetBool("Walking", true); // starts ainmation

            Vector3 cameraForward = cameraTransform.forward; // checks the forward direction of the camera
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;// checks the right direction of the camera
            cameraRight.Normalize();

            Vector3 moveDir = (cameraRight * inputVector.x) + (cameraForward * inputVector.y); // writes the direction of the camera multiplied by the input to a direction


            moveDir.Normalize();

            rb.AddForce(moveDir * CurrentSpeed, ForceMode.Impulse); // multiplies above direction by max speed
        }
        Quaternion targetRotation = Quaternion.LookRotation(cameraTransform.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }

    private void rotatioBarrelEnds()
    {
        foreach (GameObject barrelEndTransform in barrelEnds)
        {
            float angle = cam.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value; // calculates desired angle for barrel rotation, only considered up and down
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
            barrelEndTransform.transform.localRotation = Quaternion.Euler(barrelEndTransform.transform.rotation.x, barrelEndTransform.transform.rotation.y, angle);// rotates the barrel ends based of the vertical axis of the camera

        }
    }
}

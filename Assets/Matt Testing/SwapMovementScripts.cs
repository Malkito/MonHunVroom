using UnityEngine;
using System.Collections;
using TMPro;


public class SwapMovementScripts : MonoBehaviour
{

    private NewTankMovement CameraReletiveMovement;
    private tankMovement RotationReletive;

    [SerializeField] private TMP_Text MovementText;

    bool CameraReletiveMovementActive;

    bool swapable;


    private void Awake()
    {
        CameraReletiveMovement = GetComponent<NewTankMovement>();

        RotationReletive = GetComponent<tankMovement>();
    }

    private void Start()
    {
        CameraReletiveMovementActive = true;
        swapable = true;
    }

    private void FixedUpdate()
    {
        bool SpacePressed = GameInput.instance.getJumpInput();

        if (SpacePressed && swapable)
        {
            StartCoroutine(swapDelay(SpacePressed));
        }

    }

    IEnumerator swapDelay(bool SpacePressed)
    {
        if (SpacePressed && CameraReletiveMovementActive)
        {
            CameraReletiveMovement.enabled = false;
            RotationReletive.enabled = true;
            CameraReletiveMovementActive = false;
            MovementText.text = "Movement Scheme: Rotation Relative Movement";
        }
        else if (SpacePressed)
        {
            CameraReletiveMovement.enabled = true;
            RotationReletive.enabled = false;
            CameraReletiveMovementActive = true;
            MovementText.text = "Movement Scheme: Camera Relative Movement";
        }
        swapable = false;

        MovementText.color = new Color(1, 1, 1, 0.5f);

        yield return new WaitForSeconds(2f);

        MovementText.color = new Color(1, 1, 1, 1);
        swapable = true;
    }

    



}

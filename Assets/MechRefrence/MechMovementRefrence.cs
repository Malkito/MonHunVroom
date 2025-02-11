using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
public class MechMovementRefrence : MonoBehaviour
{
    /// <summary>
    /// 
    /// Handles the movement, rotation of Mecha Player
    /// 
    /// 
    /// </summary>



    [Header("Turret Rotation")]
    public GameObject turret; // Top half of player

    [Header("Camera")]
    public CinemachineOrbitalFollow cam;
    public Transform cameraTransform;

    [Header("Movement")]
    public GameObject hips; // lower half of player
    public float speed; // max speed

    [Header("Barrel Ends")]
    public GameObject[] barrelEnds; // Two barrel ends, one at the end of each arm
    public float maxAngle;


    [Header("Other")]
    private Rigidbody rb;
    public Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        movement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); // reads input
        rotateTorso();
        rotatioBarrelEnds();

    }


    private void movement(float horizontal, float vertical) // Handles the movement of the player based off the camera postion
    {

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized; // reads input, while no input, vector equals 0

        if (direction.magnitude >= 0.1f) // checks if any input is being pressed
        {
            anim.SetBool("Walking", true); // starts ainmation

            Vector3 cameraForward = cameraTransform.forward; // checks the forward direction of the camera
            cameraForward.y = 0; // ignores tilt
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;// checks the right direction of the camera
            cameraRight.y = 0; // ignores tilt
            cameraRight.Normalize();

            Vector3 moveDir = (cameraRight * horizontal) + (cameraForward * vertical); // writes the direction of the camera multiplied by the input to a direction
            moveDir.Normalize();


            rb.linearVelocity = new Vector3(moveDir.x * speed, rb.linearVelocity.y, moveDir.z * speed); // multiplies above direction by max spped
            hips.transform.forward = moveDir;// sets the lower half of player to face direction its moving

        }
        else
        {
            anim.SetBool("Walking", false); // Stops animation
        }
        turret.transform.rotation = Quaternion.Euler(0, turret.transform.rotation.y, 0); // enusures top half of player doesnt rotate in werid ways

    }

    private void rotateTorso()
    {
       turret.transform.rotation = Quaternion.Euler(turret.transform.parent.rotation.x, cam.HorizontalAxis.Value, turret.transform.rotation.z); // reads the horizontal axis of the camera and rotates the top hlaf of player accordingly

    }

    private void rotatioBarrelEnds()
    {
        foreach(GameObject barrelEndTransform in barrelEnds)
        {
            float angle = (cam.VerticalAxis.Value * (maxAngle * 2)) - maxAngle; // calculates desired angle
            barrelEndTransform.transform.localRotation = Quaternion.Euler(angle, barrelEndTransform.transform.rotation.y, barrelEndTransform.transform.rotation.z);// rotates the barrel ends based of the vertical axis of the camera

        }
    }
}

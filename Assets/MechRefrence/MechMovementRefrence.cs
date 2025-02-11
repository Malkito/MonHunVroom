using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
public class MechMovementRefrence : MonoBehaviour
{
    [Header("Turret Rotation")]
    public GameObject turret;

    [Header("Camera")]
    public CinemachineOrbitalFollow cam;
    public Transform cameraTransform;

    [Header("Movement")]
    public GameObject hips;
    public float speed;

    [Header("Audio")]
    [SerializeField] private AudioListener listener;

    [Header("Barrel Ends")]
    public GameObject[] barrelEnds;
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
        movement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        rotateTorso();
        rotatioBarrelEnds();

    }


    private void movement(float horizontal, float vertical)
    {

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            anim.SetBool("Walking", true);
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();

            Vector3 moveDir = (cameraRight * horizontal) + (cameraForward * vertical);
            moveDir.Normalize();


            rb.linearVelocity = new Vector3(moveDir.x * speed, rb.linearVelocity.y, moveDir.z * speed);
            hips.transform.forward = moveDir;

        }
        else
        {
            anim.SetBool("Walking", false);
        }
        turret.transform.rotation = Quaternion.Euler(0, turret.transform.rotation.y, 0);

    }

    private void rotateTorso()
    {
       turret.transform.rotation = Quaternion.Euler(turret.transform.parent.rotation.x, cam.HorizontalAxis.Value, turret.transform.rotation.z);

    }

    private void rotatioBarrelEnds()
    {
        foreach(GameObject barrelEndTransform in barrelEnds)
        {
            float angle = (cam.VerticalAxis.Value * (maxAngle * 2)) - maxAngle;
            barrelEndTransform.transform.localRotation = Quaternion.Euler(angle, barrelEndTransform.transform.rotation.y, barrelEndTransform.transform.rotation.z);

        }
    }
}

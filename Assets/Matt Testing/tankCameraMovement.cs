using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class tankCameraMovement : NetworkBehaviour
{

    [SerializeField] private GameObject turretHead;
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private GameObject[] turretBarrel;
    [SerializeField] private float maxAngle;
    
    [HideInInspector] public bool canMove;

    [SerializeField] private float rotateSpeed;
    [SerializeField] private Transform turretLookAt;




    void Start()
    {
        canMove = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cam.Priority = 5;
        }
        else
        {
            cam.Priority = 3;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (!canMove) return;

        rotateTurretHead();
        rotatioBarrelEnds();
    }

    private void rotateTurretHead()
    {
        Vector3 direction = turretLookAt.position - transform.position;
        direction.y = 0;

        if(direction.magnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            if(rotateSpeed <= 0)
            {
                turretHead.transform.rotation = targetRotation;
            }
            else
            {
                turretHead.transform.rotation = Quaternion.Slerp(turretHead.transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }

        }

    }

    private void rotatioBarrelEnds()
    {
        foreach (GameObject turretBarrel in turretBarrel)
        {
            float angle = cam.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value; // calculates desired angle for barrel rotation, only considered up and down
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
            turretBarrel.transform.localRotation = Quaternion.Euler(angle, turretBarrel.transform.rotation.y, turretBarrel.transform.rotation.z);// rotates the barrel ends based of the vertical axis of the camera
        }
    }
}

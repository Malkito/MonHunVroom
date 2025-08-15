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


    private void LateUpdate()
    {
        if (!IsOwner) return;
        if (!canMove) return;
    }

    private void rotateTurretHead()
    {
        Vector3 baseEular = gameObject.transform.eulerAngles;

        turretHead.transform.localRotation = Quaternion.Euler(baseEular.x, cam.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value, baseEular.z);
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

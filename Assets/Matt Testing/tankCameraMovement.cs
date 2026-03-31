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
        Vector3 camForward = Camera.main.transform.forward;

        Vector3 direction = Vector3.ProjectOnPlane(camForward, transform.up);

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);

            turretHead.transform.rotation = Quaternion.RotateTowards(turretHead.transform.rotation,targetRotation,rotateSpeed * Time.deltaTime * 360f);
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


    private void OnDrawGizmos()
    {
        if (cam == null) return;

        Vector3 origin = transform.position;

        Vector3 camForward = cam.transform.forward;
        Vector3 projected = Vector3.ProjectOnPlane(camForward, transform.up);

        // Camera forward (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + camForward * 5f);

        // Projected direction (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + projected.normalized * 5f);

        // Warning zone
        if (projected.sqrMagnitude < 0.001f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, 2f);
        }
    }
}

using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class tankCameraMovement : NetworkBehaviour
{

    [SerializeField] private Transform turretHead;
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private Transform barrel;
    [SerializeField] private float maxAngle;
    
    [HideInInspector] public bool canMove;

    [SerializeField] private float rotateSpeed;
    [SerializeField] private Transform turretLookAt;

    [SerializeField] private LayerMask aimLayerMask;

    private Vector3 targetPoint;


    [SerializeField] private float maxDistance;
    [SerializeField] private float turretRotateSpeed = 10f;
    [SerializeField] private float barrelRotateSpeed = 10f;

    [Header("Barrel Limits")]
    [SerializeField] private float minBarrelAngle = -10f;
    [SerializeField] private float maxBarrelAngle = 35f;
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

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (!canMove) return;

        aimTurret();
    }


    private void aimTurret()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, aimLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * maxDistance;
        }

        RotateTurret(targetPoint);
        RotateBarrel(targetPoint);
    }


    private void RotateTurret(Vector3 targetPoint)
    {
        Vector3 desiredDirection = targetPoint - turretHead.position;

        // Turret only rotates around Y
        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);

        turretHead.rotation = Quaternion.Slerp(turretHead.rotation,targetRotation,turretRotateSpeed * Time.fixedDeltaTime);
    }

    private void RotateBarrel(Vector3 targetPoint)
    {
        Vector3 desiredDirection = targetPoint - barrel.position;

        // Convert direction into turret local space
        Vector3 localDirection = turretHead.InverseTransformDirection(desiredDirection);

        // Calculate vertical angle
        float angle = Mathf.Atan2(localDirection.y, localDirection.z) * Mathf.Rad2Deg;

        // Clamp barrel movement
        angle = Mathf.Clamp(angle, minBarrelAngle, maxBarrelAngle);

        Quaternion targetRotation = Quaternion.Euler(-angle, 0f, 0f);

        barrel.localRotation = Quaternion.Slerp(barrel.localRotation,targetRotation,barrelRotateSpeed * Time.deltaTime);
    }
    private void OnDrawGizmos()
    {
        if (cam == null) return;


        Vector3 camForward = cam.transform.forward;

        // Camera forward (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(cam.transform.position, cam.transform.position + camForward * 1000);

        // Projected direction (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(barrel.transform.position, barrel.transform.position + barrel.transform.forward * 1000);


        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPoint, 1f);
    }

    /*
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

    
    */


}

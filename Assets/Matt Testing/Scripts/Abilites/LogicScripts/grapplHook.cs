using UnityEngine;

public class grapplHook : MonoBehaviour
{
    private bool grappleAttached;
    private bool grappleRetracted;

    [SerializeField] float grappleDistance;
    [SerializeField] float grappleAttractionForce;
    public float grappleDelay;
    [SerializeField] LayerMask grappleableLayer;
    private Vector3 grapplePoint;
    [SerializeField] private LineRenderer line;
    [SerializeField] float overShootYAxis;



    playerMovement PM;
    playerShooting PS;


    Rigidbody rb;
    public void deployGrapple(Transform transform)
    {
        PM = transform.GetComponent<playerMovement>();
        PS = transform.GetComponent<playerShooting>();
        rb = transform.GetComponent<Rigidbody>();


        grappleAttached = true;
        RaycastHit hit;

        if (Physics.Raycast(PM.cameraTransform.position, PM.cameraTransform.forward, out hit, grappleDistance, grappleableLayer))
        {
            grapplePoint = hit.point;
            Invoke(nameof(executeGrapple), grappleDelay); // pulls the player
        }
        else
        {
            grapplePoint = PM.cameraTransform.position + PM.cameraTransform.forward * grappleDistance;
            Invoke(nameof(retractGrapple), grappleDelay);

        }

        line.enabled = true;
        line.SetPosition(1, grapplePoint);

    }

    public void retractGrapple()
    {
        grappleAttached = false;
        //Destroy(gameObject);
        line.enabled = false;
        print("Grapple Retracted");


    }

    private void LateUpdate()
    {
        if (grappleAttached)
        {
            line.SetPosition(0, PS.mainBarrelEnds[0].position);
        }
    }

    public void jumpToPos(Vector3 targetPos, float trajectoryHeight)
    {
        rb.linearVelocity = calculateJumpVelocity(transform.position, targetPos, trajectoryHeight);

    }

    private void executeGrapple()
    {
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overShootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overShootYAxis;

        jumpToPos(grapplePoint, highestPointOnArc);


        Invoke(nameof(retractGrapple), 1f);

    }


    private Vector3 calculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x = startPoint.x, 0f, endPoint.z - startPoint.z);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        return velocityXZ + velocityY;
    }


}

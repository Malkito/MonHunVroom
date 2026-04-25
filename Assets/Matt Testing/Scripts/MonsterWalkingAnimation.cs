using UnityEngine;

public class MonsterWalkingAnimation : MonoBehaviour
{
    [Header("References")]
    public Transform body;
    public Transform leftFootTarget;
    public Transform rightFootTarget;

    [Header("Settings")]
    public float stepDistance = 0.5f;
    public float stepHeight = 0.3f;
    public float stepSpeed = 4f;
    public float footSpacing = 0.3f;
    public float stepForwardDistance = 0.5f;
    public LayerMask groundLayer;

    private Vector3 leftFootOldPos, leftFootNewPos;
    private Vector3 rightFootOldPos, rightFootNewPos;

    private float leftLerp = 1;
    private float rightLerp = 1;

    private bool isLeftMoving = false;
    private bool isRightMoving = false;

    private Vector3 leftNormal, rightNormal;

    void Start()
    {
        leftFootOldPos = leftFootTarget.position;
        rightFootOldPos = rightFootTarget.position;

        leftFootNewPos = leftFootOldPos;
        rightFootNewPos = rightFootOldPos;
    }

    void Update()
    {
        UpdateFoot(ref leftFootOldPos, ref leftFootNewPos, ref leftLerp, ref isLeftMoving, ref leftNormal, -footSpacing);
        UpdateFoot(ref rightFootOldPos, ref rightFootNewPos, ref rightLerp, ref isRightMoving, ref rightNormal, footSpacing);

        MoveFeet();
    }

    void UpdateFoot(ref Vector3 oldPos, ref Vector3 newPos, ref float lerp, ref bool isMoving, ref Vector3 normal, float sideOffset)
    {
        // 🔥 Forward-projected ray origin
        Vector3 origin = body.position
                       + body.forward * stepForwardDistance
                       + body.right * sideOffset
                       + Vector3.up;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2f, groundLayer))
        {
            if (!isMoving && Vector3.Distance(newPos, hit.point) > stepDistance)
            {
                if (isLeftMoving || isRightMoving) return;

                isMoving = true;
                lerp = 0;
                oldPos = newPos;
                newPos = hit.point;
                normal = hit.normal;
            }
        }

        if (isMoving)
        {
            lerp += Time.deltaTime * stepSpeed;

            Vector3 tempPos = Vector3.Lerp(oldPos, newPos, lerp);
            tempPos.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            if (sideOffset < 0)
                leftFootTarget.position = tempPos;
            else
                rightFootTarget.position = tempPos;

            if (lerp >= 1)
            {
                isMoving = false;
            }
        }
    }

    void MoveFeet()
    {
        if (!isLeftMoving)
            leftFootTarget.position = leftFootNewPos;

        if (!isRightMoving)
            rightFootTarget.position = rightFootNewPos;

        leftFootTarget.up = leftNormal;
        rightFootTarget.up = rightNormal;
    }
}

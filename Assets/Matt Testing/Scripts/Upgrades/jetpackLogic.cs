using UnityEngine;


public class jetpackLogic : MonoBehaviour, useAbility
{
    [SerializeField] private float hoverForce;
    [SerializeField] static float maxHoverCharge = 500;
    [SerializeField] private float hoverChargeDeplationRate;
    [SerializeField] private float hoverChargeRegenAmout;
    private float currentHoverCharge = 500;

    private bool canHover;


    public void useAbility(Transform transform, bool abilityPressed)
    {
        Rigidbody rb = transform.GetComponent<Rigidbody>();

        if(currentHoverCharge <= 0)
        {
            currentHoverCharge = 0;
            canHover = false;
        }

        if (abilityPressed && currentHoverCharge >= 0 && canHover)
        {
            isHovering(rb);
        }
        else
        {
            
            isNotHovering();
        }

        print("Current Charge: " + currentHoverCharge);
    }

    private void isHovering(Rigidbody rb)
    {
        currentHoverCharge -= Time.deltaTime * hoverChargeDeplationRate;
        rb.AddForce(Vector3.up * hoverForce, ForceMode.Acceleration);
    }

    private void isNotHovering()
    {
        currentHoverCharge += Time.deltaTime * hoverChargeRegenAmout;
        if (currentHoverCharge >= (currentHoverCharge * 0.2)) canHover = true;
        if (currentHoverCharge >= maxHoverCharge) currentHoverCharge = maxHoverCharge;
    }


    private void Update()
    {
        print("Update running");
    }
}

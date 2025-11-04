using UnityEngine;
using UnityEngine.UI;

public class jetpackLogic : MonoBehaviour, useAbility
{
    [SerializeField] private float hoverForce;
    [SerializeField] static float maxHoverCharge = 500;
    [SerializeField] private float hoverChargeDeplationRate;
    [SerializeField] private float hoverChargeRegenAmout;
    private float currentHoverCharge = 500;

    private bool canHover;
    private Slider JetpackSlider;
    private Transform JetpackSliderMeterUI;

    public void useAbility(Transform transform, bool abilityPressed)
    {
        Rigidbody rb = transform.GetComponent<Rigidbody>();

        JetpackSliderMeterUI = FindFireUI(transform, "JetPackMeter");
        JetpackSliderMeterUI.gameObject.SetActive(true);
        JetpackSlider = JetpackSliderMeterUI.GetChild(0).GetComponent<Slider>();

        if (currentHoverCharge <= 0)
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
            print("Not hovering");
        }
        JetpackSlider.value = currentHoverCharge / maxHoverCharge;
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


    private Transform FindFireUI(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindFireUI(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

}

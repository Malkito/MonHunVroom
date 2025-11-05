using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class jetpackLogic : NetworkBehaviour, useAbility, onUpgradePickedup, onUpgradeDropped
{
    [SerializeField] private float hoverForce;
    [SerializeField] static float maxHoverCharge = 500;
    [SerializeField] private float hoverChargeDeplationRate;
    [SerializeField] private float hoverChargeRegenAmout;
    private float currentHoverCharge = 500;

    private bool canHover;
    private Slider JetpackSlider;
    private Transform JetpackSliderMeterUI;

    private GameObject hatOBj;

    private Animator ac;

    private void Awake()
    {
    }

    public void useAbility(Transform transform, bool abilityPressed)
    {
        Rigidbody rb = transform.GetComponent<Rigidbody>();

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

    public void onUpgradePickedup(Transform player)
    {
        JetpackSliderMeterUI = FindChild(player, "JetPackMeter");
        JetpackSliderMeterUI.gameObject.SetActive(true);
        JetpackSlider = JetpackSliderMeterUI.GetChild(0).GetComponent<Slider>();
        /*
        hatOBj = FindChild(player, "HeliHat").gameObject;
        StartCoroutine(deylatHatEnable());
        ac = hatOBj.GetComponent<Animator>();
        */
    }

    [ServerRpc(RequireOwnership = false)]
    private void enableDisableHatServerRpc(bool enableDisable)
    {
        hatOBj.SetActive(enableDisable);
    }


    public void onUpgradeDropped(Transform player)
    {
        JetpackSliderMeterUI.gameObject.SetActive(false);
        /*
        enableDisableHatServerRpc(false);
        gameObject.GetComponent<NetworkObject>().Despawn();
        */

    }

    private void isHovering(Rigidbody rb)
    {
        currentHoverCharge -= Time.deltaTime * hoverChargeDeplationRate;
        rb.AddForce(Vector3.up * hoverForce, ForceMode.Acceleration);
        ac.SetBool("IsHovering", true);
    }

    private void isNotHovering()
    {
        currentHoverCharge += Time.deltaTime * hoverChargeRegenAmout;
        if (currentHoverCharge >= (currentHoverCharge * 0.2)) canHover = true;
        if (currentHoverCharge >= maxHoverCharge) currentHoverCharge = maxHoverCharge;
        ac.SetBool("IsHovering", false);
    }



    private Transform FindChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChild(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

}

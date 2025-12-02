using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class jetpackLogic : NetworkBehaviour, useAbility, onUpgradePickedup, onUpgradeDropped
{
    [Header("Jetpack Settings")]
    [SerializeField] private float hoverForce = 25f;
    [SerializeField] private float maxFuel = 500f;
    [SerializeField] private float fuelDrainRate = 70f;
    [SerializeField] private float fuelRegenRate = 50f;

    private float currentFuel;

    // Runtime state
    private bool isPressingHover = false;
    private Rigidbody playerRb;
    private Animator hatAnimator;
    private GameObject hatObject;
    private Slider fuelSlider;
    private Transform fuelUIRoot;

    private Transform playerTransform;   // Given by upgrade manager

    private bool initialized = false;

    private void Awake()
    {
        currentFuel = maxFuel;
    }

    // -------------------------------
    //           SETUP
    // -------------------------------
    public void onUpgradePickedup(Transform player)
    {
        playerTransform = player;
        playerRb = player.GetComponent<Rigidbody>();

        // ----- Find hat -----
        hatObject = FindChildRecursive(player, "HeliHat").gameObject;
        hatAnimator = hatObject.GetComponent<Animator>();

        // Turn hat ON for everyone
        EnableHatServerRpc(true);

        // ----- Find UI (owner only!) -----
        if (IsOwner)
        {
            Transform playerUI = FindChildRecursive(player, "PlayerUI");
            fuelUIRoot = FindChildRecursive(playerUI, "JetPackMeter");
            fuelSlider = fuelUIRoot.GetChild(0).GetComponent<Slider>();
            fuelUIRoot.gameObject.SetActive(true);
        }

        initialized = true;
    }

    public void onUpgradeDropped(Transform player)
    {
        if (IsOwner && fuelUIRoot != null)
            fuelUIRoot.gameObject.SetActive(false);

        EnableHatServerRpc(false);

        // Despawn logic instance
        NetworkObject.Despawn();
    }

    // -------------------------------
    //        RPC SECTION
    // -------------------------------
    [ServerRpc(RequireOwnership = false)]
    private void EnableHatServerRpc(bool enable)
    {
        EnableHatClientRpc(enable);
    }

    [ClientRpc]
    private void EnableHatClientRpc(bool enable)
    {
        if (hatObject != null)
            hatObject.SetActive(enable);
    }

    // -------------------------------
    //        USE ABILITY
    // -------------------------------
    public void useAbility(Transform pos, bool abilityPressed)
    {
        if (!initialized) return;

        if (abilityPressed && currentFuel > 0f)
            isPressingHover = true;
        else
            isPressingHover = false;

        // Update UI only on owner
        if (IsOwner && fuelSlider != null)
            fuelSlider.value = currentFuel / maxFuel;
    }

    // -------------------------------
    //        MAIN LOGIC
    // -------------------------------
    private void FixedUpdate()
    {
        if (!initialized) return;
        if (playerRb == null) return;

        if (isPressingHover)
        {
            print("is pressing hover: " + isPressingHover);
            Hovering();
        }
        else
        {
            NotHovering();
        }
    }

    private void Hovering()
    {
        if (currentFuel <= 0f)
        {
            currentFuel = 0f;
            isPressingHover = false;
            hatAnimator.SetBool("IsHovering", false);
            return;
        }

        currentFuel -= Time.fixedDeltaTime * fuelDrainRate;
        if (currentFuel < 0f) currentFuel = 0f;

        playerRb.AddForce(Vector3.up * hoverForce, ForceMode.Acceleration);
        hatAnimator.SetBool("IsHovering", true);
    }

    private void NotHovering()
    {
        currentFuel += Time.fixedDeltaTime * fuelRegenRate;
        if (currentFuel > maxFuel) currentFuel = maxFuel;

        hatAnimator.SetBool("IsHovering", false);
    }

    // -------------------------------
    //     CHILD FINDING UTILITY
    // -------------------------------
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}

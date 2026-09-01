using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class noGravityLogic : NetworkBehaviour, useAbility
{
    [Header("Ability Settings")]
    [SerializeField] private float floatForce;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private float effectDuration;
    private float elapsedTime;
    [SerializeField] private bool isEffectActive;

    private void Start()
    {
        isEffectActive = false;
        //startEffectBool = false;
    }

    public void useAbility(Transform transform, bool abilityUsed)
    {
        if (!abilityUsed) return;
        print("Ability used");
        isEffectActive = true;
        startEffect();
        //startEffectServerRpc();


    }

    private void Update()
    {
        print("isEffectActive: " + isEffectActive);

        if (isEffectActive)
        {
            print("Elapsed Time: " + elapsedTime);
            elapsedTime += Time.deltaTime;
        }

        if (elapsedTime >= effectDuration)
        {
            endEffect();
            //endEffectServerRpc();
        }

    }

    private void startEffect()
    {
        if (isEffectActive) return;

        elapsedTime = 0;
        Rigidbody[] allRigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);

        // Disable gravity and add random rotation/float
        foreach (Rigidbody rb in allRigidbodies)
        {
            rb.useGravity = false;

            // Random rotation direction
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * rotationSpeed;

            // Apply slight upward force
            rb.AddForce(Vector3.up * floatForce, ForceMode.VelocityChange);

            // Add torque for random spinning
            rb.AddTorque(randomTorque, ForceMode.VelocityChange);
        }
    }

    private void endEffect()
    {
        Rigidbody[] allRigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        // Re-enable gravity
        foreach (Rigidbody rb in allRigidbodies)
        {
            if (rb != null)
                rb.useGravity = true;
        }

        isEffectActive = false;
    }





    [ServerRpc(RequireOwnership = false)]
    private void startEffectServerRpc()
    {
        startEffect();
    }


    [ServerRpc(RequireOwnership = false)]
    private void endEffectServerRpc()
    {
        endEffect();
    }
}

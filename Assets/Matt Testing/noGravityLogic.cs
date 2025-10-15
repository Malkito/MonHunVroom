using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class noGravityLogic : NetworkBehaviour, useAbility
{
    [Header("Ability Settings")]
    [SerializeField] private float floatForce;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private float effectDuration;
    private bool isEffectActive;

    private void Start()
    {
        isEffectActive = false;
    }

    public void useAbility(Transform transform, bool abilityUsed)
    {
        if (!abilityUsed) return;

        if (!isEffectActive)
        {
            print("1");
            ApplyFloatEffect();
        }
    }

    /*
    [ClientRpc]
    private void ActivateAbilityClientRpcc()
    {
        print("2");
        // Notify all clients to run the effect
        ActivateAbilityClientRpc();
    }
    */

    private void Update()
    {
        print(isEffectActive);
    }

    [ClientRpc]
    private void ActivateAbilityClientRpc()
    {
        print("2");
        StartCoroutine(ApplyFloatEffect());
    }

    private IEnumerator ApplyFloatEffect()
    {
        print("3");
        isEffectActive = true;
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

        // Keep floating for the effect duration
        yield return new WaitForSeconds(effectDuration);

        // Re-enable gravity
        foreach (Rigidbody rb in allRigidbodies)
        {
            if (rb != null)
                rb.useGravity = true;
        }

        isEffectActive = false;
    }
}

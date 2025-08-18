using UnityEngine;
using System.Collections;

public class noGravityLogic : MonoBehaviour, useAbility
{
    [SerializeField] private float floatTime;
    [SerializeField] private float floatForce;
    private Rigidbody[] rigidBodies;
    private float elapsedTime;
    [SerializeField] private float torqueStrentgh;
    private bool floating;

    public void useAbility(Transform transform, bool abilityUsed)
    {
        if (!abilityUsed) return;
        startFloating();
    }


    private void Update()
    {
        if(elapsedTime >= floatTime)
        {
            stopFloating();
        }
        elapsedTime += Time.deltaTime;
    }

    private void startFloating()
    {
        elapsedTime = 0;
        rigidBodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (Rigidbody rb in rigidBodies)
        {
            rb.useGravity = false;
            rb.AddForce(Vector3.up * floatForce, ForceMode.VelocityChange);
            Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f),Random.Range(-1f, 1f),Random.Range(-1f, 1f)).normalized * torqueStrentgh;
            rb.AddTorque(randomTorque, ForceMode.VelocityChange);
        }
    }

    private void stopFloating()
    {
        elapsedTime = 0;
        rigidBodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (Rigidbody rb in rigidBodies)
        {
            rb.useGravity = true;
        }
    }


}

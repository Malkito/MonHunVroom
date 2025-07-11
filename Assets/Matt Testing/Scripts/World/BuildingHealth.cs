using UnityEngine;
using System.Collections;
using Unity.Netcode;
public class BuildingHealth : NetworkBehaviour, dealDamage
{
    /// <summary>
    /// Health goes on all buildings. "Buildings" includes cars, trees, props, etc.
    /// 
    /// 
    /// handles the Health and destruction of the buildings
    /// 
    /// </summary>


    [SerializeField] private float maxHealth;
    public float currentHealth;


    private Coroutine dotCoroutine; // The damage over time couroutine. This varible is used to stop the courotine when there are no fires on hte building


    [SerializeField] private float flashTIme;
    [SerializeField] private Color fireColour;

    public Color damageColour;

    private Material mat;


    public float numOfFiresOnBuilding;

    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        currentHealth = maxHealth;
        
    }

    public void increaseFireNumber() // Called from fire bullets
    {
        numOfFiresOnBuilding++;
    }

    public void decreaseFireNumber() // Called from when idividual fires on the building are destroyed
    {
        numOfFiresOnBuilding--;
    }


    // Update is called once per frame
    void Update()
    {
        if(currentHealth <= 0) 
        {
            DestroyBuildingServerRpc();
        }



        if(numOfFiresOnBuilding  > 0) // if the number of the fires on the building 1 or aboive, begin to deal damage over time absed on the number on fires on the building
        {
            if (dotCoroutine != null)
            {
                return;
            }
            dotCoroutine = StartCoroutine(damageOverTimeCoroutine(numOfFiresOnBuilding, 1));
        }
        else // Fires on building are 0
        {
            stopDamageOverTime();
        }
        
    }


    [ServerRpc(RequireOwnership = false)]
    public void DestroyBuildingServerRpc()
    {
        NetworkObject netObj = gameObject.GetComponent<NetworkObject>();
        netObj.Despawn();
        Destroy(gameObject);

    }
    public void dealDamage(float damage, Color flashColor, GameObject DamageOrigin) // Takes away a certian amount of health, starts the flash damage color Coroutine
    {
        currentHealth -= damage;
        StartCoroutine(flashDamageColor(flashColor));
    }


    private void stopDamageOverTime() // stops the damage overtime when the number of fires is 0
    {
        if(dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
            dotCoroutine = null;
        }
    }


    private IEnumerator damageOverTimeCoroutine(float damagePerTick, float maxBurnTime) // the actual logic for the corutine,
    {
        float elapsedTIme = 0f;
        while(elapsedTIme < maxBurnTime) // if the max burn time has not elapsed
        {
            yield return new WaitForSeconds(1f);
            currentHealth -= damagePerTick;
            StartCoroutine(flashDamageColor(Color.red));
            elapsedTIme += 1f;
        }
        dotCoroutine = null;
    }

    private IEnumerator flashDamageColor(Color flashColor) // flashes the color of hte object when it takes damage
    {
        Color baseMat = mat.color;
        mat.color = flashColor;
        yield return new WaitForSeconds(flashTIme);
        mat.color = baseMat;
    }

}



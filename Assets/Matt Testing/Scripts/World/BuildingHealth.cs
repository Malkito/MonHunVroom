using UnityEngine;
using System.Collections;

public class BuildingHealth : MonoBehaviour, dealDamage
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
            Destroy(gameObject);
        }



        if(numOfFiresOnBuilding  > 0) // if the number of the fires on the builind 1 or aboive, begin to deal damage over time absed on the number on fires on the building
        {
            applyDamageOverTime(numOfFiresOnBuilding, 1);
        }
        else // Fires on building are 0
        {
            stopDamageOverTime();
        }
        
    }
    public void dealDamage(float damage, Color flashColor) // Takes away a certian amount of health, starts the flash damage color Coroutine
    {
        currentHealth -= damage;
        StartCoroutine(flashDamageColor(flashColor));
    }


    public void applyDamageOverTime(float damagePerTick, float maxBurnTime) // checks to see if the damage over time courtine has not started,
    {                                                                        //, if not then write the coroutine to the varible and start it
        if (dotCoroutine != null)
        {
            return;
        }
        dotCoroutine = StartCoroutine(damageOverTimeCoroutine(damagePerTick, maxBurnTime));
    }

    public void stopDamageOverTime() // stops the damage overtime when the number of fires is 0
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
            dealDamage(damagePerTick, fireColour); // deal damage based of number of fires on obj
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



using UnityEngine;
using System.Collections;

public class BuildingHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    public float currentHealth;
    private Coroutine dotCoroutine;
    [SerializeField] private float flashTIme;
    [SerializeField] private Color fireColour;
    public Color damageColour;

    private Material mat;


    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        currentHealth = maxHealth;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(currentHealth <= 0)
        {
            Destroy(gameObject);
        }
        
    }

    public void dealDamage(float damage)
    {
        currentHealth -= damage;
        StartCoroutine(flashDamageColor(damageColour));
    }


    public void applyDamageOverTime(float damagePerTick, float maxBurnTime)
    {
       if(dotCoroutine != null)
        {
            return;
        }
        dotCoroutine = StartCoroutine(damageOverTimeCoroutine(damagePerTick, maxBurnTime));
    }

    public void stopDamageOverTime()
    {
        if(dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
            dotCoroutine = null;
        }
    }


    private IEnumerator damageOverTimeCoroutine(float damagePerTick, float maxBurnTime)
    {
        float elapsedTIme = 0f;
        while(elapsedTIme < maxBurnTime)
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(flashDamageColor(fireColour));
            currentHealth -= damagePerTick;
            elapsedTIme += 1f;
        }
        dotCoroutine = null;
    }

    private IEnumerator flashDamageColor(Color flashColor)
    {
        Color baseMat = mat.color;
        mat.color = flashColor;
        yield return new WaitForSeconds(flashTIme);
        mat.color = baseMat;
    }

}



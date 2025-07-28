using UnityEngine;
using System.Collections;
public class playerHealth : MonoBehaviour, dealDamage
{
    //Very similar to the Building Health Script
    //main Diffrence is when the health hits 0, it will start the repswawn Logic
    //And teh damage color gets all the mat on the player. This willhave teo change after offical model is in




    [Header("Health")]
    [SerializeField] public float maxHealth;
    [SerializeField] public float currentHealth;

    [Header("Color flash")]
    [SerializeField] private float flashTIme;
    [SerializeField] private Color fireColour;
    public Color damageColour;
    private Coroutine dotCoroutine;

    [Header("Other")]
    [SerializeField] private float numOfFiresOnHealth;
    [SerializeField] private Material mat;

    public bool canTakeDamage;

    void Start()
    {
        canTakeDamage = true;
        currentHealth = maxHealth;
    }
    public void increaseFireNumber()
    {
        numOfFiresOnHealth++;
    }

    public void decreaseFireNumber()
    {
        numOfFiresOnHealth--;
    }
    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0)
        {
            print("Helth has hit 0");
            StartCoroutine(respawnManager.Instance.StartSpawnPlayer(transform));
        }

        if (numOfFiresOnHealth > 0)
        {
            applyDamageOverTime(numOfFiresOnHealth, 1);
        }
        else
        {
            stopDamageOverTime();
        }

    }

    public void stopDamageOverTime()
    {
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
            dotCoroutine = null;
        }
    }

    public void applyDamageOverTime(float damagePerTick, float maxBurnTime)
    {
        if (dotCoroutine != null)
        {
            return;
        }
        dotCoroutine = StartCoroutine(damageOverTimeCoroutine(damagePerTick, maxBurnTime));
    }

    private IEnumerator damageOverTimeCoroutine(float damagePerTick, float maxBurnTime)
    {
        float elapsedTIme = 0f;
        while (elapsedTIme < maxBurnTime)
        {
            yield return new WaitForSeconds(1f);
            dealDamage(damagePerTick, fireColour, gameObject);
            elapsedTIme += 1f;
        }
        dotCoroutine = null;
    }

    public void dealDamage(float damage, Color flashColor, GameObject damageOrigin)
    {
        if (!canTakeDamage) return;
        StartCoroutine(FlashDamageColor(flashColor, mat));
        currentHealth -= damage;
    }






    private IEnumerator FlashDamageColor(Color flashColor, Material mat)
    {
        Color originalcolor = mat.color;
        mat.color = flashColor;
        yield return new WaitForSeconds(flashTIme);
        mat.color = originalcolor;
    }

}

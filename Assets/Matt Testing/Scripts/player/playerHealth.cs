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
    [SerializeField] private MeshRenderer[] mat;

    public bool canTakeDamage;

    void Start()
    {
        canTakeDamage = true;
        mat = GetComponentsInChildren<MeshRenderer>();
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
            StartCoroutine(respawnManager.Instance.respawnPlayer(transform));
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
            dealDamage(damagePerTick, fireColour);
            elapsedTIme += 1f;
        }
        dotCoroutine = null;
    }

    public void dealDamage(float damage, Color flashColor)
    {
        if (!canTakeDamage) return;
        StartCoroutine(flashDamageColor(flashColor, mat));
        currentHealth -= damage;

    }






    private IEnumerator flashDamageColor(Color flashColor, MeshRenderer[] meshes)
    {

        Color[] originalcolors = new Color[meshes.Length];

        for(int i = 0; i < meshes.Length; i++)
        {
            originalcolors[i] = meshes[i].material.color;
            meshes[i].material.color = flashColor;
        }

        yield return new WaitForSeconds(flashTIme);

        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].material.color = originalcolors[i];
        }

    }

}

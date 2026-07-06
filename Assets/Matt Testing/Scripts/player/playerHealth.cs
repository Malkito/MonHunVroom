using UnityEngine;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;

public class playerHealth : NetworkBehaviour, dealDamage
{
    [Header("Health")]
    [SerializeField] public float baseMaxHealth = 100f;
    private float maxHealth;
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    float BaseSliderSize = 540;

    [Header("Color flash")]
    [SerializeField] private float flashTIme;
    [SerializeField] private Color fireColour;
    public Color damageColour;
    private Coroutine dotCoroutine;

    [Header("Other")]
    [SerializeField] private float numOfFiresOnHealth;
    [SerializeField] private Material mat;
    [SerializeField] private GameObject deathUI;
    [SerializeField] private Slider healthSlider;

    public bool canTakeDamage;

    playerStats PlayerStats;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerStats = GetComponent<playerStats>();

            currentHealth.Value = maxHealth;
            canTakeDamage = true;
        }
    }

    void Update()
    {
        if (!IsServer) return;

        if (numOfFiresOnHealth > 0)
        {
            applyDamageOverTime(numOfFiresOnHealth, 1);
        }
        else
        {
            stopDamageOverTime();
        }

        healthSlider.value = currentHealth.Value / baseMaxHealth;
    }

    public void increaseFireNumber()
    {
        if (!IsServer) return;
        numOfFiresOnHealth += 10;
    }

    public void decreaseFireNumber()
    {
        if (!IsServer) return;
        numOfFiresOnHealth -= 10;
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
        if (dotCoroutine != null) return;

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
        if (!IsServer) return;
        if (!canTakeDamage) return;

        currentHealth.Value -= damage;



        if (currentHealth.Value < 0)
            currentHealth.Value = 0;

        //StartCoroutine(FlashDamageColor(flashColor, mat));

        if (currentHealth.Value <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        canTakeDamage = false;
    }

    private IEnumerator FlashDamageColor(Color flashColor, Material mat)
    {
        Color originalcolor = mat.color;
        mat.color = flashColor;
        yield return new WaitForSeconds(flashTIme);
        mat.color = originalcolor;
    }

    public void applyHealthChanged()
    {
        maxHealth = baseMaxHealth * PlayerStats.currentHealth.Value; 
        currentHealth.Value = maxHealth;

        RectTransform Rect = healthSlider.GetComponent<RectTransform>();

        float newWidth = BaseSliderSize * PlayerStats.currentHealth.Value;

        Rect.sizeDelta = new Vector2(newWidth, healthSlider.GetComponent<RectTransform>().rect.size.y);

    }


}
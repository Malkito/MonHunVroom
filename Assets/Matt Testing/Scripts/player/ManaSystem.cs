using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using Niki.UI;
using UnityServiceLocator;


public class ManaSystem : NetworkBehaviour
{
    [Header("Mana Varible")]
    [SerializeField] private float maxMana;
    public float manaUsedPerActivation;

    [Header("Regen Varibles")]
    [SerializeField] private float ManaRegenAmount;
    [SerializeField] private float timeBeforeRegen;

    [Header("Other")]
    [SerializeField] Slider ManaSLider;
    private playerShooting PS;

    private float currentMana;
    private bool canRegen;
    public Property<float> ManaFill { get; } = new(1f);

    playerStats PlayerStats;

    private void Awake()
    {
        ServiceLocator.For(this).Register<ManaSystem>(this);
    }

    public override void OnNetworkSpawn()
    {
        PlayerStats = GetComponent<playerStats>();
        PS = GetComponent<playerShooting>();
    }

    void Start()
    {
        ManaSLider.maxValue = maxMana;
        currentMana = maxMana;
        UpdateManaFill();
    }
    void Update()
    {
        ManaSLider.value = currentMana;
        
        if(canRegen && currentMana < maxMana)
        {
            currentMana += ManaRegenAmount + (PlayerStats.currentSpecialBoost.Value / 100);
            currentMana = Mathf.Min(currentMana, maxMana);
            UpdateManaFill();
        }
    }


    public void Activaction(float manaToConsume)
    {
        if (currentMana <= manaToConsume) return;

        currentMana -= manaToConsume;
        UpdateManaFill();

        StopCoroutine(regenDelay());
        StartCoroutine(regenDelay());

        PS.AltShootServerRPC(1);


    }

    private void UpdateManaFill()
    {
        float fill = maxMana > 0f ? Mathf.Clamp01(currentMana / maxMana) : 0f;
        ManaFill.TrySetValue(fill);
    }

    IEnumerator regenDelay()
    {
        canRegen = false;
        yield return new WaitForSeconds(timeBeforeRegen);
        canRegen = true;
    }

}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;


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

    playerStats PlayerStats;

    public override void OnNetworkSpawn()
    {
        PlayerStats = GetComponent<playerStats>();
        PS = GetComponent<playerShooting>();
    }

    void Start()
    {
        ManaSLider.maxValue = maxMana;
        currentMana = maxMana;
    }
    void Update()
    {
        ManaSLider.value = currentMana;
        
        if(canRegen && currentMana < maxMana)
        {
            currentMana += ManaRegenAmount + (PlayerStats.currentSpecialBoost.Value / 100);
        }
    }


    public void Activaction(float manaToConsume)
    {
        if (currentMana <= manaToConsume) return;

        currentMana -= manaToConsume;

        StopCoroutine(regenDelay());
        StartCoroutine(regenDelay());

        PS.AltShootServerRPC(1);


    }

    IEnumerator regenDelay()
    {
        canRegen = false;
        yield return new WaitForSeconds(timeBeforeRegen);
        canRegen = true;
    }

}

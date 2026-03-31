using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManaSystem : MonoBehaviour
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
    private float regenDelayTime;


    private void Awake()
    {
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
            currentMana += ManaRegenAmount;
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

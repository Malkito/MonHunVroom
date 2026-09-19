using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UpgradeScriptableOBJ", menuName = "Scriptable Objects/Upgrades")]
public class UpgradeScriptableOBJ : ScriptableObject
{
    public int upgradeID; // UNIQUE ID

    public bool isPurchased;

    public int amountOfTimePurchased;

    public float spawnWeight;

    public GameObject pickupObject;

    [Tooltip("Sprite shown in the ability slot when this upgrade is equipped.")]
    public Sprite IconImage;

    [Tooltip("Tint applied to both the ability icon and its glint while this upgrade is assigned.")]
    public Color abilityColor = Color.white;

    public string itemDesc;

    public bool isAvailble;

    public bool isBulletUpgrade;

    public GameObject logicScriptObject;

    [Tooltip("Cooldown duration in seconds. The presenter maps remaining time divided by this value to Image.fillAmount from 0 to 1.")]
    public float cooldown;

    public bool canBeUsedWhileDead;

}  

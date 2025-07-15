using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UpgradeScriptableOBJ", menuName = "Scriptable Objects/Upgrades")]
public class UpgradeScriptableOBJ : ScriptableObject
{
    public bool isPurchased;

    public int amountOfTimePurchased;

    public float spawnWeight;

    public GameObject pickupObject;

    public Sprite IconImage;

    public string itemDesc;

    public bool isAvailble;

    public bool isBulletUpgrade;

    public GameObject logicScriptObject;

    public float cooldown;

    public bool canBeUsedWhileDead;

}  

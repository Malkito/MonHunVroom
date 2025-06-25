using UnityEngine;
using Unity.Netcode;
interface dealDamage //Anything that needs to take damage inherites from this
{
    [ServerRpc()]
    public void dealDamage(float damageDealt, Color flashColor, GameObject damageOrigin);

    public void increaseFireNumber();

    public void decreaseFireNumber();
}

public class defaultBullet : MonoBehaviour, bullet
{

    private GameObject BulletDamageOrigin;
    [SerializeField] private BulletSO bulletData; // The deafault bullet data, set in inspector

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent(out dealDamage healthScript))
        {
            healthScript.dealDamage(bulletData.bulletDamage, Color.gray, BulletDamageOrigin); // deals damage if collides with something that can be damaged
        }
    }


    public void setDamageOrigin(GameObject damageOrigin)
    {
        BulletDamageOrigin = damageOrigin;
    }

}


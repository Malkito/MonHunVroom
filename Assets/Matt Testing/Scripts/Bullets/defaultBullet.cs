using UnityEngine;
using Unity.Netcode;
public interface dealDamage //Anything that needs to take damage inherites from this
{
    public void dealDamage(float damageDealt, Color flashColor, GameObject damageOrigin);

    public void increaseFireNumber();

    public void decreaseFireNumber();
}

public class defaultBullet : NetworkBehaviour, bullet
{

    private GameObject BulletDamageOrigin;
    [SerializeField] private BulletSO bulletData; // The deafault bullet data, set in inspector
    public bool isBigShot;
    [SerializeField] private float bigShotDamageMulti;
    public float damageBonus;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.TryGetComponent(out dealDamage healthScript))
        {
            float damage = bulletData.bulletDamage * damageBonus;

            if (isBigShot) {
                damage *= bigShotDamageMulti;
            }
            healthScript.dealDamage(damage, Color.gray, BulletDamageOrigin); // deals damage if collides with something that can be damaged
        }
        destroyBulletServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void destroyBulletServerRpc()
    {
        Destroy(gameObject);
    } 

    public void setDamageOrigin(GameObject damageOrigin)
    {
        BulletDamageOrigin = damageOrigin;
    }

}


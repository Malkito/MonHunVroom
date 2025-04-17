using UnityEngine;
interface dealDamage //Anything that needs to take damage inherites from this
{
    public void dealDamage(float damageDealt, Color flashColor);

    public void increaseFireNumber();

    public void decreaseFireNumber();
}


public class defaultBullet : MonoBehaviour
{

    [SerializeField] private BulletSO bulletData; // The deafault bullet data, set in inspector

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent(out dealDamage healthScript))
        {
            healthScript.dealDamage(bulletData.bulletDamage, Color.gray); // deals damage if collides with something that can be damaged
        }
    }
}


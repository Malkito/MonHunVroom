using UnityEngine;

public class fireBullet : MonoBehaviour
{
    [SerializeField] GameObject fireEffect; 
    private ParticleSystem fireParticle;

    [SerializeField] BulletSO fireBulletSO;
    [SerializeField] float maxBurnTime;


    public BuildingHealth BH;
    [SerializeField] private fireManager FM;
    public void OnCollisionEnter(Collision collision)
    {
        GameObject fire = Instantiate(fireEffect, transform.position, Quaternion.Euler(-90,0,0));
        fireParticle = fire.transform.GetChild(0).GetComponent<ParticleSystem>();
        Destroy(fire, fireParticle.main.duration);

        FM = fire.GetComponent<fireManager>();
        BH = collision.gameObject.GetComponent<BuildingHealth>();
        if(BH != null)
        {
            FM.buildingHealthFireManager = BH;
            FM.startDamageOverTime(fireBulletSO.bulletDamage, fireParticle.main.duration);
        }
        Destroy(gameObject);
    }
}

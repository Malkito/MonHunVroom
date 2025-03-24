using UnityEngine;
using System.Collections.Generic;

public class defaultAltBullet : MonoBehaviour
{
    [SerializeField] private BulletSO bulletData;
    [SerializeField] private float damageRadius;
    [SerializeField] ParticleSystem explosion;

    private void OnCollisionEnter(Collision collision)
    {
        foreach(BuildingHealth health in getBuildingsInArea())
        {
            health.dealDamage(bulletData.bulletDamage);
        }
        GameObject explosionParticle = Instantiate(explosion.gameObject, transform.position, Quaternion.identity);
        Destroy(explosionParticle, explosion.main.duration);
        Destroy(gameObject);
    }

    private BuildingHealth[] getBuildingsInArea()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
        List<BuildingHealth> buildingsInArea = new List<BuildingHealth>();

        foreach (Collider col in hitColliders)
        {
            BuildingHealth BH = col.gameObject.GetComponent<BuildingHealth>();
            if(BH != null)
            {
                buildingsInArea.Add(BH);
            }
        }
        return buildingsInArea.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}

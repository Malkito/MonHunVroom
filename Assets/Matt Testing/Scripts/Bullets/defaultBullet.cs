using UnityEngine;

public class defaultBullet : MonoBehaviour
{
    [SerializeField] private BulletSO bulletData;

    private void OnCollisionEnter(Collision collision)
    {
        BuildingHealth buildingHealth = collision.gameObject.GetComponent<BuildingHealth>();
        if(buildingHealth != null)
        {
            buildingHealth.dealDamage(bulletData.bulletDamage);
        }
    }
}

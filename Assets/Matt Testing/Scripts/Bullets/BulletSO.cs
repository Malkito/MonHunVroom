using UnityEngine;

[CreateAssetMenu(fileName = "BulletSO", menuName = "Scriptable Objects/Bullet")]
public class BulletSO : ScriptableObject
{
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public float bulletLifetime;
    public float bulletDamage;
    public float minTimeBetweenShots;
    public float recoilForce;
}

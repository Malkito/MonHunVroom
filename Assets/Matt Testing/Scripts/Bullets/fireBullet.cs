using UnityEngine;

public class fireBullet : MonoBehaviour
{
    [SerializeField] GameObject fireEffect;
    public void OnCollisionEnter(Collision collision)
    {
        GameObject fire = Instantiate(fireEffect, transform.position, Quaternion.Euler(-90,0,0));
        Destroy(fire, 6);
        Destroy(gameObject);
    }
}

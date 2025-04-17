using UnityEngine;
using System.Collections.Generic;

public class waterBullet : MonoBehaviour
{
    //Goes onto water Bullet prefab, checks if it collides with anything, then instantiates the particle system
    //Particle system has putOutFire script to check collsions with fire

    [SerializeField] private float waterDuration;
    [SerializeField] private GameObject waterSplash;
    [SerializeField] private float sphereSize;
    private fireManager FM;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject water = Instantiate(waterSplash, transform.position, Quaternion.identity);
        Destroy(water, waterDuration);
        Destroy(gameObject);

        foreach(GameObject fireOBj in findFireInArea())
        {
            Destroy(fireOBj);
        }
    }


    private GameObject[] findFireInArea()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereSize);
        List<GameObject> fires = new List<GameObject>();

        foreach(Collider col in hitColliders)
        {
            if (col.gameObject.CompareTag("Fire"))
            {
                fires.Add(col.gameObject);
            }
        }
        return fires.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereSize);
    }
}

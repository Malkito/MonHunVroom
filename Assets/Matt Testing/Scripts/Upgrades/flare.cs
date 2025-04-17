using UnityEngine;

public class flare : MonoBehaviour
{

    [SerializeField] private GameObject airStrikeMissle;
    [SerializeField] private float timeToSpawn;
    private float elapsedTime;
    [SerializeField] private float spawnHeight;
    private bool spawned;
    void Start()
    {
        spawned = false;
        elapsedTime = 0;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= timeToSpawn && !spawned)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.x + spawnHeight, transform.position.z);
            Instantiate(airStrikeMissle, spawnPos, Quaternion.identity);
            spawned = true;
        }
    }
}

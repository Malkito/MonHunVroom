using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Roubble : MonoBehaviour
{
    [SerializeField]
    private float _lifeSpan = 10;

    private void Awake()
    {
        Invoke("Death", _lifeSpan);
    }

    public void Death()
    {
        Destroy(gameObject);
    }

    public Roubble CreateRouble(Vector3 startPosition, float strength)
    {
        Roubble roubbleCopy = Instantiate(this, startPosition, Quaternion.identity);

        Rigidbody roubleBody = roubbleCopy.GetComponent<Rigidbody>();

        Vector3 randomForce = Random.insideUnitSphere * strength;
        randomForce = new Vector3(randomForce.x, strength, randomForce.z);

        roubleBody.AddForce(randomForce, ForceMode.Force);

        return roubbleCopy;
    }
}

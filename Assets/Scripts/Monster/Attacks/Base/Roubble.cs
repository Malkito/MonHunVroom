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

    public Roubble CreateRouble(Vector3 startPosition, float minStrength, float maxStrength)
    {
        Roubble roubbleCopy = Instantiate(this, startPosition, Quaternion.identity);

        Rigidbody roubleBody = roubbleCopy.GetComponent<Rigidbody>();

        float verticalThrowStrength = Random.Range(minStrength, maxStrength);
        float horizontalThrowSrength = Random.Range(minStrength, maxStrength);

        Vector3 randomForce = Random.insideUnitSphere * horizontalThrowSrength;
        randomForce = new Vector3(randomForce.x, verticalThrowStrength, randomForce.z);

        roubleBody.AddForce(randomForce, ForceMode.Force);

        return roubbleCopy;
    }
}

using UnityEngine;

public class Bug : MonoBehaviour
{
    [Header("Movement Properties")]
    [SerializeField]
    [Min(0.0f)]
    private float _bugWanderRadius;

    [SerializeField]
    [Min(0.0f)]
    private float _moveSpeed = 4;

    private Transform _origin;

    private Vector3 _targetOffset;

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _origin.position + _targetOffset, _moveSpeed * Time.deltaTime);

        if (Vector3.Distance(_origin.position + _targetOffset, transform.position) <= 0.2f)
        {
            _targetOffset = Random.insideUnitSphere * _bugWanderRadius;
        }
    }

    public static Bug SpawnBug(Bug prefab, Transform origin)
    {
        Bug bugInstance = Instantiate(prefab, origin);
        Vector3 position = origin.position + (Random.insideUnitSphere * bugInstance._bugWanderRadius);
        bugInstance.transform.position = position;
        bugInstance._targetOffset = Random.insideUnitSphere * bugInstance._bugWanderRadius;
        bugInstance._origin = origin;
        return bugInstance;

    }
}

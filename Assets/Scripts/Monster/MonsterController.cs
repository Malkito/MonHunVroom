using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [SerializeField]
    private Transform[] _eyes;

    [SerializeField]
    private Vector3 _monsterBottom;

    public Vector3 MonsterBottom { get { return _monsterBottom; } }

    public Transform GetRandomEye()
    {
        if (_eyes.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, _eyes.Length);
        return _eyes[index];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + _monsterBottom, 0.1f);
    }
}

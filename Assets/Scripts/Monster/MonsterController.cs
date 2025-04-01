using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [SerializeField]
    private Transform[] _eyes;

    public Transform GetRandomEye()
    {
        if (_eyes.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, _eyes.Length);
        return _eyes[index];
    }
}

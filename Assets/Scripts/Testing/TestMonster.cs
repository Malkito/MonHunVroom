using UnityEngine;

public class TestMonster : MonoBehaviour
{
    [SerializeField]
    private Transform _target;

    TargetOffsetter _targetOffsetter;

    private void Awake()
    {
        _targetOffsetter = new TargetOffsetter(_target, 1);
    }

    private void OnDrawGizmos()
    {
        if (_targetOffsetter == null) return;
        _targetOffsetter.DrawPoints(transform.position);
    }

}

using UnityEngine;

public abstract class Attack : ScriptableObject
{
    [SerializeField]
    private float _randomPositionRange;

    public AttackController AttackHandler { get; private set; }

    protected float RandomPositionRange { get { return _randomPositionRange; } }

    public void Initilize(AttackController attackHandler)
    {
        AttackHandler = attackHandler;
        OnInilization();
    }

    protected virtual void OnInilization() { }

    public virtual void OnUpdate() { }

    public virtual void OnStart() { }

    public virtual void OnStop() { }

    public virtual bool CanFinishAttack()
    {
        return true;
    }

    public virtual Vector3 GetRandomPosition()
    {
        return NavMeshUtility.GetRandomPosition(AttackHandler.transform.position, _randomPositionRange);
    }

    public virtual Vector3 GetTargetPosition()
    {
        return AttackHandler.transform.position;
    }
}

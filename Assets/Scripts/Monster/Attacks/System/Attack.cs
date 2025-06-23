using UnityEngine;

public abstract class Attack : ScriptableObject
{
    [SerializeField]
    private float _randomPositionRange;

    protected AttackController AttackHandler { get; private set; }

    protected float RandomPositionRange { get { return _randomPositionRange; } }

    public Vector3 TargetPosition { get => AttackHandler.TargetPosition; }
    public Vector3 OffsettedTargetPosition { get => AttackHandler.OffsettedTargetPosition; }

    public void Initilize(AttackController attackHandler)
    {
        AttackHandler = attackHandler;
        OnInilization(attackHandler.gameObject);
    }

    protected virtual void OnInilization(GameObject controlledObject) { }

    public virtual void OnUpdate() { }

    public virtual void OnStart() { }

    public virtual void OnStop() { }

    public virtual bool CanFinishAttack()
    {
        return true;
    }
}

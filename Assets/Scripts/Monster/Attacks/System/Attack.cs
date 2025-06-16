using UnityEngine;

public abstract class Attack : ScriptableObject
{
    [SerializeField]
    private float _randomPositionRange;

    protected AttackController AttackHandler { get; private set; }

    protected float RandomPositionRange { get { return _randomPositionRange; } }

    protected TargetResolver TargetProvider { get => AttackHandler.TargetProvider; }

    public Vector3 TargetPosition { get => TargetProvider.GetTargetPosiiton(); }

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

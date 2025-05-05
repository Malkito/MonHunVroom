using UnityEngine;

public abstract class Attack : ScriptableObject
{
    [SerializeField]
    [Header("Attacks Properties")]
    private string _id;

    [SerializeField]
    private float _randomPositionRange;

    public string ID { get { return _id; } }

    public GameObject Parent { get; private set; }

    protected float RandomPositionRange { get { return _randomPositionRange; } }

    public void Initilize(GameObject parentObject)
    {
        Parent = parentObject;
        OnInilization();
    }

    public virtual void OnInilization() { }

    public virtual void OnUpdate() { }

    public virtual void OnStart() { }

    public virtual void OnStop() { }

    public virtual bool CanFinishAttack()
    {
        return true;
    }

    public virtual Vector3 GetRandomPosition()
    {
        return NavMeshUtility.GetRandomPosition(Parent.transform.position, _randomPositionRange);
    }

    public virtual Vector3 GetTargetPosition()
    {
        return Parent.transform.position;
    }
}

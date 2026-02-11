using LordBreakerX.AttackSystem;
using LordBreakerX.Utilities;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Bug Attack")]
public class BugAttack : ScriptableAttack
{
    [Header("Sworm Properties")]
    [SerializeField]
    [Min(0)]
    private float _yOffset = 5;

    [SerializeField]
    [Min(0)]
    private float _spawnRadius = 5;

    [SerializeField]
    [Min(1)]
    private int _minSpawns = 1;

    [SerializeField]
    [Min(1)]
    private int _maxSpawns = 1;

    [Header("Random Target Properties")]
    [SerializeField]
    [Min(0)]
    private float _randomTargetChance = 40.0f;

    [SerializeField]
    [Min(0)]
    private float _targetRadius = 40;

    [SerializeField]
    [Min(1)]
    private int _swormLimit;

    [Header("Prefab Properties")]
    [SerializeField]
    private BugSworm _swormPrefab;

    public override void OnAttackStarted()
    {
        int swormAmount = Mathf.Clamp(Random.Range(_minSpawns, _maxSpawns), 0, _swormLimit - BugSworm.TotalSworms);

        for (int i = 0; i < swormAmount; i++) 
        {
            Vector3 spawnPosition = GetSpawnPosition();
            BugSworm sworm = null;

            if (Probability.IsSuccessful(_randomTargetChance))
            {
                AttackTarget randomTarget = TargetUtility.GetRandomTarget(Controller.gameObject, _targetRadius, Controller.IgnoredLayers);
                sworm = BugSworm.SpawnSworm(_swormPrefab, spawnPosition, randomTarget);
            }
            else
            {
                sworm = BugSworm.SpawnSworm(_swormPrefab, spawnPosition, Target);
            }
                Controller.SpawnProjectile(sworm.gameObject);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 position = Controller.transform.position + (Random.onUnitSphere * _spawnRadius);
        position.y += _yOffset;
        return position;
    }

    public override bool CanUseAttack()
    {
        return BugSworm.TotalSworms < _swormLimit;
    }

}

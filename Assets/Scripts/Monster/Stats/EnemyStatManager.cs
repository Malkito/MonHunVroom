using UnityEngine;
using LordBreakerX.Stats;

public class EnemyStatManager : StatsManager<EnemyStats>
{
    private static EnemyStatManager instance = null;

    public static float MaxHealth => instance.GetStat(EnemyStats.MAX_HEALTH);
    public static float MovementSpeed => instance.GetStat(EnemyStats.MOVEMENT_SPEED);
    public static float TurningSpeed => instance.GetStat(EnemyStats.TURNING_SPEED);
    public static float LaserEyesDamage => instance.GetStat(EnemyStats.LASER_EYES_DAMAGE); 
    public static float LaserEyesSpeed => instance.GetStat(EnemyStats.LASEE_EYES_SPEED); 
    public static float StompDamage => instance.GetStat(EnemyStats.STOMP_DAMAGE);
    public static float StompRadius => instance.GetStat(EnemyStats.STOMP_RADIUS);
    public static float TailswipeDamage => instance.GetStat(EnemyStats.TAILSWIPE_DAMAGE);

    protected override void OnEnable()
    {
        base.OnEnable();

        if (instance == null)
            instance = this;
        else
        {
            Debug.LogWarning("Only one instance of monster stat manager is allowed! (hidden duplicate)");
            gameObject.SetActive(false);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (instance == this)
            instance = null;
    }
}

using LordBreakerX.Stats;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Stats/Monster Stats")]
public class EnemyStats : StatHolder
{
    public const string MAX_HEALTH = "maxHealth";
    public const string MOVEMENT_SPEED = "movementSpeed";
    public const string TURNING_SPEED = "turningSpeed";
    public const string LASER_EYES_DAMAGE = "laserEyesDamage";
    public const string LASEE_EYES_SPEED = "laserEyesSpeed";
    public const string STOMP_DAMAGE = "stompDamage";
    public const string STOMP_RADIUS = "stompRadius";
    public const string TAILSWIPE_DAMAGE = "tailswipeDamage";

    [SerializeField]
    [Header("Health Properties")]
    private Stat _maxHealth = new Stat(MAX_HEALTH, 100, 400);

    [SerializeField]
    [Header("Movement Properties")]
    private Stat _movementSpeed = new Stat(MOVEMENT_SPEED, 14, 25);

    [SerializeField]
    private Stat _turningSpeed = new Stat(TURNING_SPEED, 120);

    [SerializeField]
    [Header("Laser Eyes Properties")]
    private Stat _laserEyesDamage = new Stat(LASER_EYES_DAMAGE, 50, 150);

    [SerializeField]
    private Stat _laserEyesSpeed = new Stat(LASEE_EYES_SPEED, 20);

    [SerializeField]
    [Header("Stomp Properties")]
    private Stat _stompDamage = new Stat(STOMP_DAMAGE, 50, 150);

    [SerializeField]
    private Stat _stompRadius = new Stat(STOMP_RADIUS, 5, 10);

    [SerializeField]
    [Header("TailSwipe Properties")]
    private Stat _tailSwipeDamage = new Stat(TAILSWIPE_DAMAGE, 50, 150);

    public override List<Stat> GetStats()
    {
        List<Stat> stats = new List<Stat>() 
        {
            _maxHealth,
            _movementSpeed,
            _turningSpeed,
            _laserEyesDamage,
            _laserEyesSpeed,
            _stompDamage,
            _stompRadius,
            _tailSwipeDamage
        };
        return stats;
    }
}

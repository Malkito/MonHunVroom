using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Stats")]
public class MonsterStats : ScriptableObject
{
    [SerializeField]
    [Header("Health Properties")]
    [Min(1)]
    private float _maxHealth = 1;

    [SerializeField]
    [Header("Movement Properties")]
    [Min(1)]
    private float _movementSpeed = 14;

    [SerializeField]
    [Min(1)]
    private float _turningSpeed = 120;

    [SerializeField]
    [Header("Laser Eyes Properties")]
    [Min(0)]
    private float _laserEyesDamage;

    [SerializeField]
    [Min(0)]
    private float _laserEyeSpeed = 20;

    [SerializeField]
    [Min(0)]
    [Header("Stomp Properties")]
    private float _stompDamage;

    [SerializeField]
    [Min(0)]
    private float _stompRadius;

    [SerializeField]
    [Min(0)]
    [Header("TailSwipe Properties")]
    private float _tailSwipeDamage;

    public float MaxHealth { get => _maxHealth; }

    public float MovementSpeed { get => _movementSpeed; }
    public float TurningSpeed { get => _turningSpeed; }

    public float LaserEyesDamage { get => _laserEyesDamage; }
    public float LaserEyeSpeed { get => _laserEyeSpeed; }

    public float StompDamage { get => _stompDamage; }
    public float TailSwipeDamage { get => _tailSwipeDamage; }

    public float StompRadius { get => _stompRadius; }
}

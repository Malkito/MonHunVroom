using UnityEngine;

public class MonsterStatManager : MonoBehaviour
{
    [SerializeField]
    private MonsterStats _monsterStats;

    public float MaxHealth { get => _monsterStats.MaxHealth; }

    public float MovementSpeed { get => _monsterStats.MovementSpeed; }
    public float TurningSpeed { get => _monsterStats.TurningSpeed; }

    public float LaserEyesDamage { get =>  _monsterStats.LaserEyesDamage; }
    public float StompDamage { get => _monsterStats.StompDamage; }
    public float TailSwipeDamage { get => _monsterStats.TailSwipeDamage; }

    public float StompRadius { get => _monsterStats.StompRadius; }

    public float LaserEyeSpeed { get => _monsterStats.LaserEyeSpeed; }

}

using UnityEngine;

public class PlayerAttackCooldown
{
    public Transform PlayerTransform { get; private set; }

    private float TimeRemaining { get; set; }

    public PlayerAttackCooldown(Transform playerTransform, float totalCooldown)
    {
        PlayerTransform = playerTransform;
        TimeRemaining = totalCooldown;
    }

    public PlayerAttackCooldown(Transform playerTransform)
    {
        PlayerTransform = playerTransform;
        TimeRemaining = 0;
    }

    public bool UpdateCooldown()
    {
        TimeRemaining -= Time.deltaTime;
        return TimeRemaining <= 0;
    }

    public bool Equals(PlayerAttackCooldown other)
    {
        return other.PlayerTransform == PlayerTransform;
    }

    public override bool Equals(object obj)
    {
        if (obj is PlayerAttackCooldown cooldown)
            return Equals(cooldown);
        return false;
    }

    public override int GetHashCode()
    {
        return PlayerTransform.GetHashCode();
    }

    public static explicit operator PlayerAttackCooldown(Transform playerTransform)
    {
        return new PlayerAttackCooldown(playerTransform);
    }

    public static explicit operator Transform(PlayerAttackCooldown cooldown) 
    {
        return cooldown.PlayerTransform;
    }
}

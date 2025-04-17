using LordBreakerX.Health;

public class ExampleDamageable : Damageable
{
    // Use this method instead of Awake for initialization, 
    // since Awake is already used in the Damageable script. 
    // Alternatively, you could still use Start.
    public override void OnAwake()
    {
        
    }

    // Called whenever the object takes damage. 
    // Works for both damage over time and single instances of damage.
    protected override void OnDamaged(HealthInfo healthInfo)
    {
        
    }

    // Called whenever the object dies (i.e., current health <= 0).
    protected override void OnDeath()
    {
        
    }

    // Called whenever the object is healed. 
    // Works for both healing over time and single instances of healing.
    protected override void OnHealed(HealthInfo healthInfo)
    {
        
    }
}

using UnityEngine;

public class EnergyShield : MonoBehaviour
{

    public float shieldHealth;
    [SerializeField] private float maxShieldHealth;
    private float sheildDuration;
    [SerializeField] private float maxShieldDuration;

    [SerializeField] private GameObject shieldObject;



    void Start()
    {
        sheildDuration = maxShieldDuration;
        shieldHealth = maxShieldHealth;
    }


    void Update()
    {
        
    }
}

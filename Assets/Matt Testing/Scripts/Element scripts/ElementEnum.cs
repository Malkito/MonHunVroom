using UnityEngine;

public class ElementEnum : MonoBehaviour
{

    public static ElementEnum Instance { get; private set; }

    public enum element
    {
        none,
        fire,
        water,
        electric,
        oil
    }


    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }








}
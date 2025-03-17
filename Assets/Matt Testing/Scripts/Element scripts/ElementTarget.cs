using UnityEngine;

public class ElementTarget : MonoBehaviour
{

    public ElementEnum.element currentElement;
    public ElementEnum.element[] allowedElements;
    public bool IsTerrain;
    [SerializeField] private bool canCreateZOne;
    [SerializeField] private float maxElementalDecayTimer;
    [SerializeField] private float elementalDecayTimer;

    [HideInInspector] public ElementEffector elementEffector;


    private void Start()
    {
        elementalDecayTimer = maxElementalDecayTimer;
    }

    private void Update()
    {
        elementalDecayTimer -= Time.deltaTime;
        if(elementalDecayTimer <= 0)
        {
            currentElement = ElementEnum.element.none;
            elementalDecayTimer = 0;
        }
    }

    public void HandleElementChange(ElementEnum.element from, ElementEnum.element to)
    {
        Debug.Log($"Element changed from {from} to {to} on: " + gameObject.name);
        elementalDecayTimer = maxElementalDecayTimer;
        if (from == ElementEnum.element.fire)
        {
            if (to == ElementEnum.element.fire)
            {
                AddFireToFire();
            }
            else if (to == ElementEnum.element.water)
            {
                AddWaterToFire();
            }
            else if (to == ElementEnum.element.oil)
            {
                AddOilToFire();
            }
            else if (to == ElementEnum.element.electric)
            {
                AddElectricToFire();
            }
        }
        else if(from == ElementEnum.element.water)
        {
            if (to == ElementEnum.element.fire)
            {
                AddFireToWater();
            }
            else if (to == ElementEnum.element.water)
            {
                AddWaterToWater();
            }
            else if (to == ElementEnum.element.oil)
            {
                AddOilToWater();
            }
            else if (to == ElementEnum.element.electric)
            {
                AddElectricToWater();
            }
        }
        else if(from == ElementEnum.element.oil)
        {
            if (to == ElementEnum.element.fire)
            {
                AddFireToOIl();
            }
            else if (to == ElementEnum.element.water)
            {
                AddWaterToOil();
            }
            else if (to == ElementEnum.element.oil)
            {
                AddOilToOil();
            }
            else if (to == ElementEnum.element.electric)
            {
                AddElectricToOil();
            }
        }
        else if (from == ElementEnum.element.electric)
        {
            if (to == ElementEnum.element.fire)
            {
                AddFireToElectric();
            }
            else if (to == ElementEnum.element.water)
            {
                AddWaterToElectric();
            }
            else if (to == ElementEnum.element.oil)
            {
                addOiltoElectric();
            }
            else if (to == ElementEnum.element.electric)
            {
                AddElectricToElectric();
            }
        }
        else if (from == ElementEnum.element.none)
        {
            if (to == ElementEnum.element.fire)
            {
                AddFireToNone();
            }
            else if (to == ElementEnum.element.water)
            {
                AddWaterToNone();
            }
            else if (to == ElementEnum.element.oil)
            {
                AddOilToNone();
            }
            else if (to == ElementEnum.element.electric)
            {
                AddElectricToNone();
            }
        }
    }


    /// Add fire to X
    private void AddFireToFire() // Increase the fire zone -- reset the decay timer on player / monster
    {
        Debug.Log("Added fire to fire");
        elementEffector.createZone(elementEffector.elementalZone, elementEffector.gameObject.transform.position);
    }
    private void AddFireToOIl()//change oil zone to fire zone with high decay time
    {
        elementEffector.createZone(elementEffector.elementalZone, gameObject.transform.position);
        Debug.Log("Added fire to oil");
    }
    private void AddFireToWater() // dry up the water
    {
        Debug.Log("Added Fire to water");
        Destroy(gameObject);
    }
    private void AddFireToElectric() // dry up hte electric water
    {
        Debug.Log("Added Fire to Electic");
        Destroy(gameObject);
    
    }
    private void AddFireToNone() // create fire zone
    {
        Debug.Log("Added fire to none");
        elementEffector.createZone(elementEffector.elementalZone, elementEffector.gameObject.transform.position);
    }


    /// Add water to X

    private void AddWaterToFire() // put out fire, dont create water zone --- Stop DOT on player / monster
    {
        Debug.Log("Added Water to fire");
        Destroy(gameObject);
    }
    private void AddWaterToOil() // replace the  oil with water
    {
        Debug.Log("Added water to oil");
        elementEffector.createZone(elementEffector.elementalZone, elementEffector.gameObject.transform.position);
        Destroy(gameObject);
    }
    private void AddWaterToElectric() // increase the size of the electric zone
    {
        elementEffector.createZone(gameObject, elementEffector.gameObject.transform.position);
        Debug.Log("Added water to oil");
    }
    private void AddWaterToWater() // increase the size of the water zone
    {
        elementEffector.createZone(elementEffector.elementalZone, elementEffector.gameObject.transform.position);
        Debug.Log("Added Water to water");
    }

    private void AddWaterToNone() //create water zone
    {
        elementEffector.createZone(elementEffector.elementalZone, elementEffector.gameObject.transform.position);
        Debug.Log("Added Water to none");
    }



    //add Electric to X
    private void AddElectricToWater() // eletrify the water
    {
        elementEffector.createZone(elementEffector.elementalZone, gameObject.transform.position);
        Destroy(gameObject);
        Debug.Log("Added electric to water");
    }

    private void AddElectricToFire() // nothing
    {
        Debug.Log("Added electric to water");
    }

    private void AddElectricToOil() // nothing
    {
        Debug.Log("Added electric to oil");
    }
    private void AddElectricToElectric() // rest the decay timer
    {
        elementalDecayTimer = maxElementalDecayTimer;
        Debug.Log("Added electric to electric");
    }

    private void AddElectricToNone() // nothing --- set player to electric state
    {
        Debug.Log("Added Electric to none");
    }

    //add oil to X

    private void AddOilToFire() // increase fire zone, reset decay timer, increase decay timer
    {
        elementEffector.createZone(gameObject, elementEffector.gameObject.transform.position);
        elementalDecayTimer = maxElementalDecayTimer * 2;
        Debug.Log("Added oil to fire");
    }
    private void AddOilToWater() // replace water with oil
    {
        elementEffector.createZone(elementEffector.elementalZone, elementEffector.gameObject.transform.position);
        Destroy(gameObject);
        Debug.Log("Added oil to water");
    }

    private void addOiltoElectric() // replace electirc wit oil
    {
        elementEffector.createZone(elementEffector.elementalZone, elementEffector.gameObject.transform.position);
        Destroy(gameObject);
        Debug.Log("Added oil to electric");
    }
    private void AddOilToOil() // increase oil zone
    {
        elementEffector.createZone(elementEffector.elementalZone, elementEffector.gameObject.transform.position);
        Debug.Log("Added oil to oil");
    }
    private void AddOilToNone() // create oil zone
    {
        elementEffector.createZone(elementEffector.elementalZone, elementEffector.gameObject.transform.position);
        Debug.Log("Added Oil to none");
    }



}

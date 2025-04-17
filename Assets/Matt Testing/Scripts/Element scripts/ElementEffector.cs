using UnityEngine;

public class ElementEffector : MonoBehaviour
{

    public ElementEnum.element effectorElement;
    public GameObject elementalZone;
    [SerializeField] private bool shouldBeDestroyed;


    private void OnTriggerEnter(Collider other)
    {
        ElementTarget ET = other.gameObject.GetComponent<ElementTarget>();
        if(ET != null)
        {
            foreach(ElementEnum.element element in ET.allowedElements)
            {
                if (element == effectorElement)
                {
                    ET.elementEffector = this;
                    ET.HandleElementChange(ET.currentElement, effectorElement);
                }
            }
        }
        if (shouldBeDestroyed) Destroy(gameObject);
    }



    public void createZone(GameObject obejct, Vector3 pos)
    {
        Destroy(gameObject);
        Instantiate(obejct, pos, Quaternion.identity);
    }

}

using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class EntryDissolver : NetworkBehaviour
{

    [SerializeField] private GameObject dissovleSphere;
    private float dissolveStrength;
    [SerializeField] private float dissolveDuration;

    private AssignColor assginColour;

    private Color EdgeColour;

    private NewTankMovement NTM;

    private void Awake()
    {
        assginColour = GetComponent<AssignColor>();
        NTM = GetComponent<NewTankMovement>();
    }


    private void Start()
    {
        StartDissolver();
    }

    public void StartDissolver()
    {

        StartCoroutine(DissolveMaterial());
    }

    public IEnumerator DissolveMaterial()
    {
        float elapsedTime = 0;

        Material dissolveMaterial = dissovleSphere.GetComponent<Renderer>().material;
        dissolveMaterial.SetColor("_EdgeColour", EdgeColour);

        NTM.canMove = false;

        while (elapsedTime < dissolveDuration - 1)
        {
            elapsedTime += Time.deltaTime;

            dissolveStrength = Mathf.Lerp(0, 1, elapsedTime / dissolveDuration);
            dissolveMaterial.SetFloat("_stepFloat", dissolveStrength);

            yield return null;
        }

        dissolveStrength = 0;
        NTM.canMove = true;

    }

    public void assignEdgeColour(Color edgeColour)
    {
        EdgeColour = edgeColour;

    }







}

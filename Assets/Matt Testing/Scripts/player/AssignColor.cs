using UnityEngine;
using Unity.Netcode;

public class AssignColor : NetworkBehaviour
{
    // Static array of materials accessible to all player instances
    [SerializeField] private Material[] PlayerMaterials;
    [SerializeField] private MeshRenderer[] renderers;

    [SerializeField] private TrailRenderer[] Trails;


    EntryDissolver entryDissolver;

    private void Awake()
    {
        entryDissolver = GetComponent<EntryDissolver>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AssignMaterial();
        }
    }

    private void AssignMaterial()
    {
        // Ensure materials are available
        if (PlayerMaterials == null || PlayerMaterials.Length == 0)
        {
            Debug.LogWarning("PlayerMaterials array is not set or empty!");
            return;
        }

        // Determine which material to use
        int index = (int)OwnerClientId % PlayerMaterials.Length;
        Material chosenMaterial = PlayerMaterials[index];

        if(entryDissolver != null)
        {
            entryDissolver.assignEdgeColour(chosenMaterial.color);
        }


        // Get all MeshRenderers on this object and its children
        //MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in renderers)
        {
            // Assign the chosen material
            renderer.material = chosenMaterial;
        }

        // Sync to clients
        ApplyMaterialClientRpc(index);
    }

    [ClientRpc]
    private void ApplyMaterialClientRpc(int materialIndex)
    {
        if (PlayerMaterials == null || materialIndex >= PlayerMaterials.Length)
            return;

        Material chosenMaterial = PlayerMaterials[materialIndex];
        

        foreach (MeshRenderer renderer in renderers)
        {
            Material[] mats = renderer.materials;
            mats[0] = chosenMaterial;
            renderer.materials = mats;
        }

        

        if(Trails != null)
        {
            foreach(TrailRenderer trail in Trails)
            {
                trail.material = chosenMaterial;
            }
        }


    }
}

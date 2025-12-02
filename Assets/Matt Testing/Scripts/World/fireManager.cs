using Unity.Netcode;
using UnityEngine;

public class fireManager : NetworkBehaviour
{
    [Header("Fire Logic")]
    [SerializeField] private float fireLifetime = 10f;

    // Network-synced reference to the object this fire is attached to
    private NetworkVariable<NetworkObjectReference> attachedObjectRef = new(writePerm: NetworkVariableWritePermission.Server);

    // Local cached object reference for easier access
    [HideInInspector] public GameObject objectFireIsAttachedTo;

    private float currentTime;

    private void Awake()
    {
        currentTime = 0;
    }

    public override void OnNetworkSpawn()
    {
        // SERVER — already knows what it attached to
        if (IsServer)
        {
            if (attachedObjectRef.Value.TryGet(out var obj))
                Initialize(obj.gameObject);
        }
        // CLIENT — wait until server syncs the attached object
        else
        {
            attachedObjectRef.OnValueChanged += OnAttachedObjectAssigned;
        }
    }

    private void OnAttachedObjectAssigned(NetworkObjectReference oldRef, NetworkObjectReference newRef)
    {
        if (newRef.TryGet(out var obj))
        {
            Initialize(obj.gameObject);
            attachedObjectRef.OnValueChanged -= OnAttachedObjectAssigned;
        }
    }

    /// <summary>
    /// Called by the server to assign the object this fire is attached to.
    /// </summary>
    public void SetAttachedObject(GameObject obj)
    {
        if (!IsServer) return;

        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            return;
        }

        attachedObjectRef.Value = netObj; // syncs to clients
        Initialize(obj); // initialize on server immediately
    }

    /// <summary>
    /// Initializes fire behavior once we know the attached object.
    /// </summary>
    private void Initialize(GameObject attachedObject)
    {
        objectFireIsAttachedTo = attachedObject;

        if (objectFireIsAttachedTo == null)
        {
            Debug.LogWarning("fireManager: Attached object is null.");
            return;
        }

        if (objectFireIsAttachedTo.TryGetComponent(out dealDamage healthScript))
        {
            healthScript.increaseFireNumber();
        }
    }

    private void Update()
    {
        if (!IsServer) return; // server controls lifecycle

        currentTime += Time.deltaTime;
        if (currentTime >= fireLifetime)
        {
            // Cleanly despawn when lifetime ends
            NetworkObject.Despawn();
        }
    }

    private void OnDestroy()
    {
        // When fire ends, tell the attached object its fire count decreases
        if (objectFireIsAttachedTo != null &&
            objectFireIsAttachedTo.TryGetComponent(out dealDamage healthScript))
        {
            healthScript.decreaseFireNumber();
        }
    }
}

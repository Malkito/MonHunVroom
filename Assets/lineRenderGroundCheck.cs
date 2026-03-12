using UnityEngine;
using Unity.Netcode;


public class lineRenderGroundCheck : NetworkBehaviour
{
    private TrailRenderer TrailRenderer;

    [SerializeField] private ParticleSystem dirtParticles;
    [SerializeField] private Rigidbody rb;


    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float minSpeedToEmit = 0.1f;

    private ParticleSystem.MainModule main;


    private bool canPlay;

    private void Awake()
    {
        main = dirtParticles.main;
    }

    private void Start()
    {
        if (!IsOwner) return;

        TrailRenderer = gameObject.GetComponent<TrailRenderer>();

        TrailRenderer.emitting = false;

    }


    private void Update()
    {
        float speed = rb.linearVelocity.magnitude;

        if(speed <= minSpeedToEmit || !canPlay)
        {
            if (dirtParticles.isPlaying)
            {
                dirtParticles.Stop();
            }
            return;
        }

        if (!dirtParticles.isPlaying && canPlay)
        {
            dirtParticles.Play();
        }

        main.startSpeed = speed * speedMultiplier;

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            TrailRenderer.emitting = true;
            canPlay = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            TrailRenderer.emitting = false;
            canPlay = false;
        }
    }

}

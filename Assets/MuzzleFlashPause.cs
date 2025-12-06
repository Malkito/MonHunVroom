using UnityEngine;
using System.Collections;

public class MuzzleFlashPause : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particles;
    void Start()
    {
        StartCoroutine(startDelay());
    }


    IEnumerator startDelay()
    {

        yield return new WaitForSeconds(0.2f);

        foreach(ParticleSystem particles in particles)
        {
            particles.Pause();
        }

    }
}

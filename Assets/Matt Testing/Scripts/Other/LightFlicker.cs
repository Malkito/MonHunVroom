using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float flickerSpeed = 5f; // How quickly the flicker changes

    [Header("Color Gradient")]
    [SerializeField] private Gradient colorGradient;
    [Range(0f, 1f)] public float gradientOffset = 0f; // Moves along the gradient

    [SerializeField] private Light flickerLight;
    private float targetIntensity;
    private float gradientTime;

    void Start()
    {
        flickerLight = GetComponent<Light>();

        // Random starting point in gradient for variation
        gradientTime = Random.value;
        targetIntensity = Random.Range(minIntensity, maxIntensity);
    }

    void Update()
    {
        // Smoothly move towards target intensity
        flickerLight.intensity = Mathf.Lerp(flickerLight.intensity, targetIntensity, Time.deltaTime * flickerSpeed);

        // If close enough, pick a new target intensity
        if (Mathf.Abs(flickerLight.intensity - targetIntensity) < 0.05f)
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
        }

        // Animate gradient color over time
        gradientTime += Time.deltaTime * flickerSpeed * 0.1f;
        if (gradientTime > 1f) gradientTime -= 1f;

        flickerLight.color = colorGradient.Evaluate((gradientTime + gradientOffset) % 1f);
    }
}

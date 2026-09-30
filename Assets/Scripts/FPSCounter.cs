using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{

    [SerializeField] private TMP_Text fpsText;

    [SerializeField] private float updateInterval = 1f;

    private float elapsedTime = 0f;
    private int frameCount;

    void Update()
    {
        elapsedTime += Time.unscaledDeltaTime;
        frameCount++;

        if (elapsedTime >= updateInterval)
        {
            float fps = frameCount / elapsedTime;

            fpsText.text = $"{Mathf.RoundToInt(fps)} FPS";

            elapsedTime = 0f;
            frameCount = 0;
        }
    }
}

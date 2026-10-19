using Ami.BroAudio;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Connects the Audio tab's master volume slider to BroAudio.</summary>
public sealed class AudioSettingsController : MonoBehaviour
{
    public const string GlobalVolumeKey = "MonHunVroom.GlobalVolume";
    private const float DefaultVolume = 1f;

    [SerializeField] private Slider globalVolumeSlider;

    private void OnEnable()
    {
        if (globalVolumeSlider == null)
        {
            Debug.LogWarning("Global volume slider is not assigned.", this);
            return;
        }

        globalVolumeSlider.minValue = 0f;
        globalVolumeSlider.maxValue = 1f;
        globalVolumeSlider.wholeNumbers = false;
        globalVolumeSlider.onValueChanged.AddListener(OnGlobalVolumeChanged);

        float volume = PlayerPrefs.GetFloat(GlobalVolumeKey, DefaultVolume);
        globalVolumeSlider.SetValueWithoutNotify(volume);
        ApplyGlobalVolume(volume);
    }

    private void OnDisable()
    {
        if (globalVolumeSlider != null)
            globalVolumeSlider.onValueChanged.RemoveListener(OnGlobalVolumeChanged);
    }

    private void OnGlobalVolumeChanged(float volume)
    {
        ApplyGlobalVolume(volume);
        PlayerPrefs.SetFloat(GlobalVolumeKey, volume);
        PlayerPrefs.Save();
    }

    /// <summary>Loads and applies the saved master volume. Call at application startup.</summary>
    public static void ApplySavedGlobalVolume()
    {
        ApplyGlobalVolume(PlayerPrefs.GetFloat(GlobalVolumeKey, DefaultVolume));
    }

    private static void ApplyGlobalVolume(float volume)
    {
        BroAudio.SetVolume(Mathf.Clamp01(volume));
    }
}

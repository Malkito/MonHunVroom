using System;
using Ami.BroAudio;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>Connects the Audio tab's volume sliders to BroAudio's master and category buses.</summary>
public sealed class AudioSettingsController : MonoBehaviour
{
    public const string GlobalVolumeKey = "MonHunVroom.GlobalVolume";
    private const float DefaultVolume = 1f;

    [Serializable]
    private sealed class CategoryVolume
    {
        public BroAudioType audioType;
        public Slider slider;
        [NonSerialized] public UnityAction<float> listener;
    }

    [SerializeField] private Slider globalVolumeSlider;
    [SerializeField] private CategoryVolume[] categoryVolumes = Array.Empty<CategoryVolume>();

    private void OnEnable()
    {
        ConfigureSlider(globalVolumeSlider, BroAudioType.All);
        if (globalVolumeSlider != null)
            globalVolumeSlider.onValueChanged.AddListener(OnGlobalVolumeChanged);

        foreach (var category in categoryVolumes)
        {
            ConfigureSlider(category.slider, category.audioType);
            if (category.slider == null)
                continue;

            category.listener = volume => SaveVolume(category.audioType, volume);
            category.slider.onValueChanged.AddListener(category.listener);
        }

        ApplySavedVolumes();
    }

    private void OnDisable()
    {
        if (globalVolumeSlider != null)
            globalVolumeSlider.onValueChanged.RemoveListener(OnGlobalVolumeChanged);

        foreach (var category in categoryVolumes)
        {
            if (category.slider != null && category.listener != null)
                category.slider.onValueChanged.RemoveListener(category.listener);
            category.listener = null;
        }
    }

    private void ConfigureSlider(Slider slider, BroAudioType audioType)
    {
        if (slider == null)
        {
            Debug.LogWarning(audioType + " volume slider is not assigned.", this);
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(LoadVolume(audioType));
    }

    private void OnGlobalVolumeChanged(float volume)
    {
        SaveVolume(BroAudioType.All, volume);
    }

    private static void SaveVolume(BroAudioType audioType, float volume)
    {
        volume = Mathf.Clamp01(volume);
        BroAudio.SetVolume(audioType, volume);
        PlayerPrefs.SetFloat(VolumeKey(audioType), volume);
        PlayerPrefs.Save();
    }

    /// <summary>Loads all saved volume levels. Call at application startup.</summary>
    public static void ApplySavedVolumes()
    {
        BroAudio.SetVolume(LoadVolume(BroAudioType.All));
        foreach (BroAudioType audioType in Enum.GetValues(typeof(BroAudioType)))
        {
            if (audioType != BroAudioType.None && audioType != BroAudioType.All)
                BroAudio.SetVolume(audioType, LoadVolume(audioType));
        }
    }

    private static float LoadVolume(BroAudioType audioType)
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey(audioType), DefaultVolume));
    }

    private static string VolumeKey(BroAudioType audioType)
    {
        return audioType == BroAudioType.All ? GlobalVolumeKey : "MonHunVroom.Volume." + audioType;
    }
}

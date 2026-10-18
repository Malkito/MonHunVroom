using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Manages display and camera sensitivity settings for the General options tab.</summary>
public sealed class GeneralSettings : MonoBehaviour
{
    private const string FullscreenModeKey = "Settings.FullscreenMode";
    private const string ResolutionKey = "Settings.Resolution";
    private const string SensitivityKey = "Settings.CameraSensitivity";

    private static readonly FullScreenMode[] FullscreenModes =
    {
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen
    };

    [Header("General tab UI")]
    [SerializeField] private TMP_Dropdown fullscreenModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Slider cameraSensitivitySlider;

    [Header("Camera sensitivity")]
    [SerializeField, Min(0f)] private float defaultCameraSensitivity = 1f;

    private readonly List<Resolution> availableResolutions = new List<Resolution>();
    private bool initializing;

    /// <summary>Read this value from the camera-look code when it applies mouse input.</summary>
    public static float CameraSensitivity { get; private set; } = 1f;

    private void Start()
    {
        InitializeSettings();
    }

    private void OnDestroy()
    {
        if (fullscreenModeDropdown != null)
            fullscreenModeDropdown.onValueChanged.RemoveListener(OnFullscreenModeChanged);
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        if (cameraSensitivitySlider != null)
            cameraSensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
    }

    private void InitializeSettings()
    {
        initializing = true;
        PopulateResolutions();
        SetupFullscreenDropdown();
        SetupResolutionDropdown();
        SetupSensitivitySlider();

        int savedMode = PlayerPrefs.GetInt(FullscreenModeKey, (int)Screen.fullScreenMode);
        int modeIndex = Array.IndexOf(FullscreenModes, (FullScreenMode)savedMode);
        if (modeIndex < 0) modeIndex = 1; // Borderless is the safe desktop default.
        if (fullscreenModeDropdown != null)
            fullscreenModeDropdown.SetValueWithoutNotify(modeIndex);

        int savedResolution = PlayerPrefs.GetInt(ResolutionKey, FindCurrentResolutionIndex());
        if (resolutionDropdown != null && availableResolutions.Count > 0)
            resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(savedResolution, 0, availableResolutions.Count - 1));

        CameraSensitivity = PlayerPrefs.GetFloat(SensitivityKey, defaultCameraSensitivity);
        if (cameraSensitivitySlider != null)
            cameraSensitivitySlider.SetValueWithoutNotify(CameraSensitivity);

        ApplySavedDisplaySettings(modeIndex, resolutionDropdown != null ? resolutionDropdown.value : 0);
        initializing = false;
    }

    private void PopulateResolutions()
    {
        availableResolutions.Clear();
        foreach (Resolution resolution in Screen.resolutions)
        {
            bool duplicate = false;
            foreach (Resolution existing in availableResolutions)
            {
                if (existing.width == resolution.width && existing.height == resolution.height)
                {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate) availableResolutions.Add(resolution);
        }

        if (availableResolutions.Count == 0)
            availableResolutions.Add(Screen.currentResolution);

        availableResolutions.Reverse();
    }

    private void SetupFullscreenDropdown()
    {
        if (fullscreenModeDropdown == null) return;
        fullscreenModeDropdown.ClearOptions();
        fullscreenModeDropdown.AddOptions(new List<string> { "Windowed", "Borderless fullscreen", "Exclusive fullscreen" });
        fullscreenModeDropdown.onValueChanged.AddListener(OnFullscreenModeChanged);
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        var labels = new List<string>(availableResolutions.Count);
        foreach (Resolution resolution in availableResolutions)
            labels.Add($"{resolution.width} x {resolution.height}");
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(labels);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void SetupSensitivitySlider()
    {
        if (cameraSensitivitySlider == null) return;
        cameraSensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void ApplySavedDisplaySettings(int modeIndex, int resolutionIndex)
    {
        Resolution resolution = availableResolutions[Mathf.Clamp(resolutionIndex, 0, availableResolutions.Count - 1)];
        FullScreenMode mode = FullscreenModes[modeIndex];
        Screen.SetResolution(resolution.width, resolution.height, mode);
    }

    private int FindCurrentResolutionIndex()
    {
        for (int i = 0; i < availableResolutions.Count; i++)
            if (availableResolutions[i].width == Screen.width && availableResolutions[i].height == Screen.height)
                return i;
        return 0;
    }

    private void OnFullscreenModeChanged(int index)
    {
        if (initializing || index < 0 || index >= FullscreenModes.Length) return;
        PlayerPrefs.SetInt(FullscreenModeKey, (int)FullscreenModes[index]);
        ApplySavedDisplaySettings(index, resolutionDropdown != null ? resolutionDropdown.value : 0);
        PlayerPrefs.Save();
    }

    private void OnResolutionChanged(int index)
    {
        if (initializing || index < 0 || index >= availableResolutions.Count) return;
        PlayerPrefs.SetInt(ResolutionKey, index);
        ApplySavedDisplaySettings(fullscreenModeDropdown != null ? fullscreenModeDropdown.value : 1, index);
        PlayerPrefs.Save();
    }

    private void OnSensitivityChanged(float value)
    {
        CameraSensitivity = value;
        PlayerPrefs.SetFloat(SensitivityKey, value);
        PlayerPrefs.Save();
    }
}

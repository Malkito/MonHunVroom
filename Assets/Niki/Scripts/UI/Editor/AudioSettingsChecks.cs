using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ami.BroAudio;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class AudioSettingsChecks
{
    private const string SliderPath = "Assets/Niki/Runtime/Prefabs/UI/Shared/Slider.prefab";
    private const string OptionsPath = "Assets/Niki/Runtime/Prefabs/UI/Canvas_Options.prefab";

    public static string Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Run audio settings checks outside Play mode.");

        var sliderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SliderPath);
        Require(sliderPrefab != null, "Shared slider prefab is missing.");
        var canonical = sliderPrefab.GetComponent<Slider>();
        Require(canonical != null && canonical.fillRect != null && canonical.handleRect != null
            && canonical.targetGraphic != null, "Shared slider visuals are not bound.");

        var options = AssetDatabase.LoadAssetAtPath<GameObject>(OptionsPath);
        var controller = options.GetComponentInChildren<AudioSettingsController>(true);
        Require(controller != null, "Audio settings controller is missing.");
        var expected = Enum.GetValues(typeof(BroAudioType)).Cast<BroAudioType>()
            .Where(type => type != BroAudioType.None && type != BroAudioType.All).ToArray();
        var bindings = ReadBindings(controller);
        Require(bindings.Count == expected.Length + 1 && expected.All(bindings.ContainsKey),
            "Bind the master and every BroAudio category exactly once.");
        foreach (var slider in options.GetComponentsInChildren<Slider>(true))
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(slider);
            Require(source == canonical, slider.name + " must use the shared slider prefab.");
        }
        Require(options.GetComponentsInChildren<Slider>(true).Length == expected.Length + 2,
            "Expected six volume sliders and one camera sensitivity slider.");
        var general = new SerializedObject(options.GetComponentInChildren<GeneralSettings>(true));
        Require(general.FindProperty("cameraSensitivitySlider").objectReferenceValue != null,
            "Camera sensitivity slider reference is missing.");
        foreach (var t in options.GetComponentsInChildren<Transform>(true))
            Require(t.GetComponents<Component>().All(c => c != null), t.name + " has a missing script.");

        var keys = bindings.Keys.ToDictionary(type => type, Key);
        var original = keys.Values.ToDictionary(key => key,
            key => (exists: PlayerPrefs.HasKey(key), value: PlayerPrefs.GetFloat(key, 1f)));
        GameObject fixture = null;
        AudioSettingsController instance = null;
        try
        {
            int i = 0;
            foreach (var key in keys.Values)
                PlayerPrefs.SetFloat(key, .15f + .1f * i++);

            fixture = Object.Instantiate(controller.gameObject);
            fixture.SetActive(false);
            fixture.hideFlags = HideFlags.HideAndDontSave;
            instance = fixture.GetComponent<AudioSettingsController>();
            Call(instance, "OnEnable");
            var sliders = ReadBindings(instance);
            foreach (var pair in sliders)
            {
                Require(pair.Value.minValue == 0f && pair.Value.maxValue == 1f && !pair.Value.wholeNumbers,
                    pair.Key + " slider range must be continuous 0–1.");
                Require(Mathf.Approximately(pair.Value.value, PlayerPrefs.GetFloat(keys[pair.Key])),
                    pair.Key + " did not load its saved value.");
            }

            i = 0;
            foreach (var pair in sliders)
            {
                var before = keys.ToDictionary(p => p.Key, p => PlayerPrefs.GetFloat(p.Value));
                float value = .23f + .09f * i++;
                pair.Value.value = value;
                foreach (var key in keys)
                    Require(Mathf.Approximately(PlayerPrefs.GetFloat(key.Value),
                        key.Key == pair.Key ? value : before[key.Key]),
                        pair.Key + " changed the wrong volume preference.");
            }

            Call(instance, "OnDisable");
            var disabled = keys.ToDictionary(p => p.Key, p => PlayerPrefs.GetFloat(p.Value));
            foreach (var slider in sliders.Values)
                slider.value = .99f;
            foreach (var key in keys)
                Require(Mathf.Approximately(PlayerPrefs.GetFloat(key.Value), disabled[key.Key]),
                    key.Key + " retained its disabled listener.");

            Call(instance, "OnEnable");
            foreach (var pair in sliders)
                Require(Mathf.Approximately(pair.Value.value, disabled[pair.Key]),
                    pair.Key + " did not restore on reopen.");
        }
        finally
        {
            if (instance != null)
                Call(instance, "OnDisable");
            if (fixture != null)
                Object.DestroyImmediate(fixture);
            foreach (var pair in original)
            {
                if (pair.Value.exists)
                    PlayerPrefs.SetFloat(pair.Key, pair.Value.value);
                else
                    PlayerPrefs.DeleteKey(pair.Key);
            }
            PlayerPrefs.Save();
            AudioSettingsController.ApplySavedVolumes();
        }
        return "PASS: shared slider references, all six volume controls, sensitivity reference, saved values, independent changes, disable/reopen lifecycle, and no missing scripts. User preferences restored.";
    }

    private static Dictionary<BroAudioType, Slider> ReadBindings(AudioSettingsController controller)
    {
        var s = new SerializedObject(controller);
        var result = new Dictionary<BroAudioType, Slider>
        {
            { BroAudioType.All, (Slider)s.FindProperty("globalVolumeSlider").objectReferenceValue }
        };
        var categories = s.FindProperty("categoryVolumes");
        for (int i = 0; i < categories.arraySize; i++)
        {
            var entry = categories.GetArrayElementAtIndex(i);
            var type = (BroAudioType)entry.FindPropertyRelative("audioType").intValue;
            Require(type != BroAudioType.None && type != BroAudioType.All && !result.ContainsKey(type),
                "Category volumes contain an invalid or duplicate category.");
            result.Add(type, (Slider)entry.FindPropertyRelative("slider").objectReferenceValue);
        }
        Require(result.Values.All(slider => slider != null), "A volume slider reference is missing.");
        Require(result.Values.Distinct().Count() == result.Count, "Volume controls share a slider.");
        return result;
    }

    private static string Key(BroAudioType type)
    {
        return type == BroAudioType.All ? AudioSettingsController.GlobalVolumeKey : "MonHunVroom.Volume." + type;
    }

    private static void Call(AudioSettingsController controller, string method)
    {
        typeof(AudioSettingsController).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(controller, null);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}

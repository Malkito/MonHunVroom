using System.Collections.Generic;
using CupOHappiness.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Builds the keybind tab from a row prefab. Rebinding behavior follows Unity Technologies'
/// InputSystem_Warriors sample and uses this project's shared GameInput action instance.
/// </summary>
public sealed class RebindSettings : MonoBehaviour
{
    private const string PanelName = "TabBody_Keybinds";
    private const string PlayerPrefsKey = "MonHunVroom.InputBindingOverrides";

    [SerializeField] private KeybindBindingRow bindingRowPrefab;
    [SerializeField] private RebindPromptPanel rebindPromptPanel;

    private readonly List<InputActionRebindingExtensions.RebindingOperation> activeRebinds = new();
    private InputActionMap playerMap;
    private string activeBindingGroup;
    private bool built;

    private void OnEnable()
    {
        if (!IsKeybindTab()) return;
        rebindPromptPanel?.Hide();
        BuildPanel();
    }

    private void OnDisable()
    {
        var operations = activeRebinds.ToArray();
        activeRebinds.Clear();
        foreach (var operation in operations)
            operation?.Cancel();
        rebindPromptPanel?.Hide();
    }

    private bool IsKeybindTab()
    {
        for (Transform current = transform; current != null; current = current.parent)
        {
            if (current.name == PanelName) return true;
        }
        return false;
    }

    private void BuildPanel()
    {
        if (bindingRowPrefab == null)
        {
            Debug.LogWarning("Assign a KeybindBindingRow prefab to the keybind panel.", this);
            return;
        }

        var gameInput = GameInput.instance != null ? GameInput.instance : FindFirstObjectByType<GameInput>();
        if (gameInput == null || gameInput.Actions == null)
        {
            Debug.LogWarning("Keybind panel could not find GameInput or its actions asset.", this);
            return;
        }

        playerMap = gameInput.Actions.FindActionMap("Player", throwIfNotFound: false);
        if (playerMap == null)
        {
            Debug.LogWarning("Keybind panel could not find the Player action map.", this);
            return;
        }

        string bindingGroup = GetCurrentBindingGroup(gameInput.Actions);
        if (built && activeBindingGroup == bindingGroup) return;

        ClearGeneratedRows();
        activeBindingGroup = bindingGroup;
        var contentLayout = GetComponent<Layout>();

        foreach (var action in playerMap.actions)
        {
            var bindings = action.bindings;
            for (int bindingIndex = 0; bindingIndex < bindings.Count; bindingIndex++)
            {
                var binding = bindings[bindingIndex];
                if (binding.isComposite || !BelongsToBindingGroup(binding, activeBindingGroup)) continue;
                CreateBindingRow(action, bindingIndex, binding);
            }
        }

        built = true;
        if (contentLayout != null)
        {
            contentLayout.RefreshChildCache();
            contentLayout.SetDirty();
            Canvas.ForceUpdateCanvases();
        }
    }

    private void CreateBindingRow(InputAction action, int bindingIndex, InputBinding binding)
    {
        string targetName = string.IsNullOrEmpty(binding.name) ? "Binding" : binding.name;
        string actionLabel = $"{action.name} — {targetName}";

        var row = Instantiate(bindingRowPrefab, transform, false);
        row.name = $"Binding_{action.name}_{bindingIndex}";
        row.Configure(
            actionLabel,
            action,
            bindingIndex,
            () => StartRebind(action, bindingIndex, row),
            () => ResetBinding(action, bindingIndex, row));
    }

    private void ResetBinding(InputAction action, int bindingIndex, KeybindBindingRow row)
    {
        action.RemoveBindingOverride(bindingIndex);
        SaveOverrides();
        row.RefreshBindingDisplay();
    }

    private void StartRebind(InputAction action, int bindingIndex, KeybindBindingRow row)
    {
        bool restoreActionEnabled = action.enabled;
        if (restoreActionEnabled)
            action.Disable();

        row.SetListening(true);
        rebindPromptPanel?.Show($"Press a key or button for {action.name}. Escape cancels.");

        InputActionRebindingExtensions.RebindingOperation operation = null;
        try
        {
            operation = action.PerformInteractiveRebinding(bindingIndex);
            foreach (string devicePath in GetAllowedDevicePaths(action.bindings[bindingIndex]))
                operation.WithControlsHavingToMatchPath(devicePath);

            operation
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<Keyboard>/escape")
                .WithControlsExcluding("<Gamepad>/start")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(op => FinishRebind(op, action, bindingIndex, row, cancelled: false, restoreActionEnabled: restoreActionEnabled))
                .OnCancel(op => FinishRebind(op, action, bindingIndex, row, cancelled: true, restoreActionEnabled: restoreActionEnabled));

            activeRebinds.Add(operation);
            operation.Start();
        }
        catch
        {
            if (operation != null)
            {
                activeRebinds.Remove(operation);
                operation.Dispose();
            }

            if (restoreActionEnabled)
                action.Enable();
            rebindPromptPanel?.Hide();
            row.SetListening(false);
            throw;
        }
    }

    private void FinishRebind(InputActionRebindingExtensions.RebindingOperation operation, InputAction action, int bindingIndex, KeybindBindingRow row, bool cancelled, bool restoreActionEnabled)
    {
        activeRebinds.Remove(operation);
        operation.Dispose();
        if (restoreActionEnabled)
            action.Enable();
        rebindPromptPanel?.Hide();
        row.SetListening(false);
        if (!cancelled)
            SaveOverrides();
        row.RefreshBindingDisplay();
    }

    private static string GetCurrentBindingGroup(InputActionAsset actions)
    {
        foreach (var map in actions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                var device = action.activeControl?.device;
                if (device is Gamepad) return "Gamepad";
                if (device is Keyboard || device is Mouse) return "Keyboard&Mouse";
            }
        }

        return "Keyboard&Mouse";
    }

    private static bool BelongsToBindingGroup(InputBinding binding, string group)
    {
        foreach (string bindingGroup in (binding.groups ?? string.Empty).Split(';'))
        {
            if (bindingGroup.Trim() == group)
                return true;
        }

        string path = binding.effectivePath ?? string.Empty;
        if (group == "Gamepad")
            return path.StartsWith("<Gamepad>");
        if (group == "Keyboard&Mouse")
            return path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>") || path.StartsWith("<Pointer>");

        return false;
    }

    private void ClearGeneratedRows()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.GetComponent<KeybindBindingRow>() != null)
                Destroy(child.gameObject);
        }
    }

    private static IEnumerable<string> GetAllowedDevicePaths(InputBinding binding)
    {
        string originalPath = binding.path ?? string.Empty;
        if (originalPath.IndexOf("stick", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return new[] { "<Gamepad>/leftStick", "<Gamepad>/rightStick" };

        var paths = new List<string>();
        var groups = (binding.groups ?? string.Empty).Split(';');
        foreach (string group in groups)
        {
            string path = group.Trim() switch
            {
                "Keyboard&Mouse" => "<Keyboard>",
                "Gamepad" => "<Gamepad>",
                "Touch" => "<Touchscreen>",
                "Joystick" => "<Joystick>",
                "XR" => "<XRController>",
                _ => null
            };
            if (!string.IsNullOrEmpty(path) && !paths.Contains(path)) paths.Add(path);
            if (group.Trim() == "Keyboard&Mouse" && !paths.Contains("<Mouse>")) paths.Add("<Mouse>");
        }

        // Bindings without a control-scheme group still stay on their original device layout.
        if (paths.Count == 0 && !string.IsNullOrEmpty(binding.effectivePath) && binding.effectivePath[0] == '<')
        {
            int layoutEnd = binding.effectivePath.IndexOf('>');
            if (layoutEnd > 1) paths.Add(binding.effectivePath.Substring(0, layoutEnd + 1));
        }

        return paths;
    }

    private static void SaveOverrides()
    {
        var actions = GameInput.instance != null ? GameInput.instance.Actions : null;
        if (actions == null) return;
        PlayerPrefs.SetString(PlayerPrefsKey, actions.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }
}

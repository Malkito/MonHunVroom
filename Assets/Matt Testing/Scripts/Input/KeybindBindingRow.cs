using CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Wires an action label, reusable binding display, and row buttons.</summary>
public sealed class KeybindBindingRow : MonoBehaviour
{
    [SerializeField] private TMP_Text actionNameText;
    [SerializeField] private InputBindingDisplay bindingDisplay;
    [SerializeField] private Button rebindButton;
    [SerializeField] private Button resetButton;

    public void Configure(string actionLabel, InputAction action, int bindingIndex, UnityAction onRebind, UnityAction onReset)
    {
        actionNameText.text = actionLabel;
        if (bindingDisplay == null)
            bindingDisplay = GetComponentInChildren<InputBindingDisplay>(includeInactive: true);

        if (bindingDisplay != null)
            bindingDisplay.SetBindingTarget(action, bindingIndex);
        else
            Debug.LogWarning("Keybind row is missing its InputBindingDisplay reference.", this);

        rebindButton.onClick.RemoveAllListeners();
        rebindButton.onClick.AddListener(onRebind);
        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(onReset);
    }

    public void RefreshBindingDisplay()
    {
        bindingDisplay?.RefreshGraphic();
    }

    public void SetListening(bool listening)
    {
        rebindButton.interactable = !listening;
        resetButton.interactable = !listening;
    }
}

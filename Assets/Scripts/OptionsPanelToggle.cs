using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Controls an options panel and provides a quit action for UI buttons.</summary>
public class OptionsPanelToggle : MonoBehaviour
{
    [Header("Options panel")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject firstSelectedElement;

    /// <summary>Show or hide the options panel.</summary>
    public void ToggleOptionsPanel()
    {
        if (optionsPanel == null)
        {
            Debug.LogWarning("Options panel is not assigned.", this);
            return;
        }

        SetOptionsPanelVisible(!optionsPanel.activeSelf);
    }

    /// <summary>Show the options panel and select its first UI element.</summary>
    public void OpenOptionsPanel()
    {
        SetOptionsPanelVisible(true);
    }

    /// <summary>Hide the options panel.</summary>
    public void CloseOptionsPanel()
    {
        SetOptionsPanelVisible(false);
    }

    /// <summary>Stop Play mode in the Editor, or quit the built application.</summary>
    public void StopPlaybackOrQuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private InputAction optionsAction;
    private readonly Dictionary<InputActionMap, bool> previousActionMapStates = new Dictionary<InputActionMap, bool>();
    private readonly Dictionary<CinemachineInputAxisController, bool> previousCameraInputStates = new Dictionary<CinemachineInputAxisController, bool>();
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;
    private bool inputStateCaptured;

    private void OnEnable()
    {
        SubscribeToOptionsAction();
    }

    private void Start()
    {
        SubscribeToOptionsAction();
    }

    private void SubscribeToOptionsAction()
    {
        InputAction currentAction = GameInput.instance != null ? GameInput.instance.OptionsAction : null;
        if (optionsAction == currentAction) return;

        if (optionsAction != null)
            optionsAction.performed -= OnOptionsPressed;

        optionsAction = currentAction;
        if (isActiveAndEnabled && optionsAction != null)
            optionsAction.performed += OnOptionsPressed;
    }

    private void OnDisable()
    {
        if (optionsAction != null)
            optionsAction.performed -= OnOptionsPressed;

        RestoreInputState();
    }

    private void OnOptionsPressed(InputAction.CallbackContext context)
    {
        ToggleOptionsPanel();
    }

    private void SetOptionsPanelVisible(bool visible)
    {
        if (optionsPanel == null)
        {
            Debug.LogWarning("Options panel is not assigned.", this);
            return;
        }

        if (visible)
            EnterMenuInputMode();
        else
            RestoreInputState();

        optionsPanel.SetActive(visible);
        if (!visible)
            return;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return;

        eventSystem.SetSelectedGameObject(firstSelectedElement);
    }

    private void EnterMenuInputMode()
    {
        if (inputStateCaptured) return;

        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        previousActionMapStates.Clear();
        InputActionAsset actions = GameInput.instance != null ? GameInput.instance.Actions : null;
        if (actions != null)
        {
            foreach (InputActionMap map in actions.actionMaps)
            {
                previousActionMapStates[map] = map.enabled;
                if (map.name == "UI" || map.name == "Global")
                    map.Enable();
                else
                    map.Disable();
            }
        }

        previousCameraInputStates.Clear();
        foreach (CinemachineInputAxisController controller in
                 FindObjectsByType<CinemachineInputAxisController>(FindObjectsSortMode.None))
        {
            previousCameraInputStates[controller] = controller.enabled;
            controller.enabled = false;
        }

        inputStateCaptured = true;
    }

    private void RestoreInputState()
    {
        if (!inputStateCaptured) return;

        foreach (KeyValuePair<InputActionMap, bool> entry in previousActionMapStates)
        {
            if (entry.Key == null) continue;
            if (entry.Value) entry.Key.Enable();
            else entry.Key.Disable();
        }

        foreach (KeyValuePair<CinemachineInputAxisController, bool> entry in previousCameraInputStates)
        {
            if (entry.Key != null)
                entry.Key.enabled = entry.Value;
        }

        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
        previousActionMapStates.Clear();
        previousCameraInputStates.Clear();
        inputStateCaptured = false;
    }
}

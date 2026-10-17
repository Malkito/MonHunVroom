using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

        optionsPanel.SetActive(visible);
        if (!visible)
            return;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return;

        eventSystem.SetSelectedGameObject(firstSelectedElement);
    }
}

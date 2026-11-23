using TMPro;
using UnityEngine;

/// <summary>Controls the shared popup shown while an input binding is listening.</summary>
public sealed class RebindPromptPanel : MonoBehaviour
{
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup backgroundInteraction;
    private bool _ownsInteraction;
    private bool _wasInteractable;
    private bool _blockedRaycasts;

    public void Show(string message)
    {
        if (promptText != null)
            promptText.text = message;

        SetBackgroundBlocked(true);
        var root = GetPopupRoot();
        root.transform.localScale = Vector3.one;
        root.SetActive(true);
    }

    public void Hide()
    {
        SetBackgroundBlocked(false);
        GetPopupRoot().SetActive(false);
    }

    private void OnDisable() => SetBackgroundBlocked(false);

    private void SetBackgroundBlocked(bool blocked)
    {
        if (!backgroundInteraction) return;
        if (blocked && !_ownsInteraction)
        {
            _wasInteractable = backgroundInteraction.interactable;
            _blockedRaycasts = backgroundInteraction.blocksRaycasts;
            _ownsInteraction = true;
            backgroundInteraction.interactable = false;
            backgroundInteraction.blocksRaycasts = false;
        }
        else if (!blocked && _ownsInteraction)
        {
            backgroundInteraction.interactable = _wasInteractable;
            backgroundInteraction.blocksRaycasts = _blockedRaycasts;
            _ownsInteraction = false;
        }
    }

    private GameObject GetPopupRoot()
    {
        return popupRoot != null ? popupRoot : gameObject;
    }
}

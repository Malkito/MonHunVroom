using TMPro;
using UnityEngine;

/// <summary>Controls the shared popup shown while an input binding is listening.</summary>
public sealed class RebindPromptPanel : MonoBehaviour
{
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text promptText;

    public void Show(string message)
    {
        if (promptText != null)
            promptText.text = message;

        var root = GetPopupRoot();
        root.transform.localScale = Vector3.one;
        root.SetActive(true);
    }

    public void Hide()
    {
        GetPopupRoot().SetActive(false);
    }

    private GameObject GetPopupRoot()
    {
        return popupRoot != null ? popupRoot : gameObject;
    }
}

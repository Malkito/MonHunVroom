using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class sessionConnecting : MonoBehaviour
{
    public sessionConnecting instance;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text startGameButtonText;

    public void Connecting()
    {
        startGameButton.interactable = false;
        startGameButtonText.text = ("Connecting.......");
    }
    public void Connected()
    {
        startGameButton.interactable = false;
        startGameButtonText.text = ("Start Game");
    }
}

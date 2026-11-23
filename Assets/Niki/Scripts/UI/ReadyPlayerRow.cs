using TMPro;
using UnityEngine;

namespace CupOHappiness.UI
{
    public sealed class ReadyPlayerRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private GameObject readyMark;

        public void Bind(string displayName, bool ready)
        {
            playerName.text = string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName;
            readyMark.SetActive(ready);
        }
    }
}

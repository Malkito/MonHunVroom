using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace CupOHappiness.UI
{
    public sealed class LobbyPlayerRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text role;
        [SerializeField] private Button kickButton;
        private ISession session;
        private string playerId;
        private Action<string> showError;
        private bool removing;

        private void Awake() => kickButton.onClick.AddListener(Kick);

        public void Bind(ISession currentSession, IReadOnlyPlayer player, Action<string> reportError)
        {
            session = currentSession;
            playerId = player.Id;
            showError = reportError;
            playerName.text = DisplayName(player);
            role.text = player.Id == session.Host ? "HOST" : "";
            role.gameObject.SetActive(player.Id == session.Host);
            kickButton.gameObject.SetActive(session.IsHost && player.Id != session.CurrentPlayer.Id);
            kickButton.interactable = !removing;
        }

        public static string DisplayName(IReadOnlyPlayer player)
        {
            return player.Properties.TryGetValue(MultiplayerLobbyPanel.PlayerNameProperty, out var name)
                && !string.IsNullOrWhiteSpace(name.Value) ? name.Value : "Player";
        }

        private async void Kick()
        {
            if (removing || session == null || !session.IsHost || playerId == session.CurrentPlayer.Id) return;
            removing = true;
            kickButton.interactable = false;
            try { await session.AsHost().RemovePlayerAsync(playerId); }
            catch (Exception error) { showError?.Invoke("Could not remove player: " + error.Message); }
            finally { if (this) removing = false; }
        }
    }
}

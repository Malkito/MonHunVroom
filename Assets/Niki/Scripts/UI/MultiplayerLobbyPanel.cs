using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace CupOHappiness.UI
{
    // Multiplayer Widgets still own create, join, copy-code, and leave operations.
    [ExecuteAlways]
    public sealed class MultiplayerLobbyPanel : MonoBehaviour
    {
        // Shared with Unity Multiplayer Widgets and its chat/player-name lookup.
        internal const string PlayerNameProperty = "w_PlayerName";
        private const string SavedNameKey = "Multiplayer.PlayerName";
        [SerializeField] private RectTransform design;
        [SerializeField] private TMP_Text sessionName;
        [SerializeField] private TMP_Text sessionCode;
        [SerializeField] private Button copyButton;
        [SerializeField] private TMP_Text playerCount;
        [SerializeField] private TMP_Text status;
        [SerializeField] private GameObject[] emptySlots;
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TMP_InputField sessionNameInput;
        [SerializeField] private TMP_InputField joinCodeInput;
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private CanvasGroup createGate;
        [SerializeField] private CanvasGroup joinGate;
        [SerializeField] private CanvasGroup leaveGate;
        [SerializeField] private Transform playerContent;
        [SerializeField] private LobbyPlayerRow playerRowPrefab;
        [SerializeField] private GameObject levelSelect;
        private readonly Dictionary<string, LobbyPlayerRow> rows = new();
        private bool joining;
        private bool savingName;
        private float nextRefresh;

        private void OnEnable()
        {
            Fit();
            if (!Application.isPlaying) return;
            playerNameInput.SetTextWithoutNotify(PlayerPrefs.GetString(SavedNameKey, ""));
            playerNameInput.onEndEdit.AddListener(SetPlayerName);
            sessionNameInput.onEndEdit.AddListener(SetSessionName);
            joinCodeInput.onEndEdit.AddListener(NormalizeCode);
            joining = false;
            nextRefresh = 0;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            playerNameInput.onEndEdit.RemoveListener(SetPlayerName);
            sessionNameInput.onEndEdit.RemoveListener(SetSessionName);
            joinCodeInput.onEndEdit.RemoveListener(NormalizeCode);
        }

        private void OnRectTransformDimensionsChange() => Fit();

        private void Fit()
        {
            if (!design) return;
            var available = ((RectTransform)transform).rect.size - new Vector2(40, 32);
            float scale = Mathf.Max(.01f, Mathf.Min(available.x / 1100f, available.y / 710f));
            design.localScale = Vector3.one * scale;
        }

        private static ISession CurrentSession()
        {
            return UnityServices.State == ServicesInitializationState.Initialized
                ? MultiplayerService.Instance.Sessions.Values.FirstOrDefault() : null;
        }

        private void Update()
        {
            Fit();
            if (!Application.isPlaying) return;
            var session = CurrentSession();
            bool canEnter = session == null && !joining && !savingName && !string.IsNullOrWhiteSpace(playerNameInput.text);
            createGate.interactable = canEnter && !string.IsNullOrWhiteSpace(sessionNameInput.text);
            joinGate.interactable = canEnter && !string.IsNullOrWhiteSpace(joinCodeInput.text);
            createGate.alpha = createGate.interactable ? 1 : .45f;
            joinGate.alpha = joinGate.interactable ? 1 : .45f;
            sessionNameInput.interactable = joinCodeInput.interactable = session == null && !joining;
            playerNameInput.interactable = !joining && !savingName;
            if (levelSelect) levelSelect.SetActive(session != null && session.IsHost);
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .25f;
            RefreshSession(session);
        }

        private void LateUpdate()
        {
            if (!copyButton) return;
            string code = Application.isPlaying ? CurrentSession()?.Code : null;
            copyButton.interactable = !string.IsNullOrWhiteSpace(code)
                && sessionCode && sessionCode.text == code;
        }

        private void RefreshSession(ISession session)
        {
            if (leaveGate)
            {
                bool inSession = session != null;
                leaveGate.alpha = inSession ? 1 : 0;
                leaveGate.interactable = leaveGate.blocksRaycasts = inSession;
            }
            sessionName.text = session == null ? " " : session.Name;
            if (sessionCode) sessionCode.text = session?.Code ?? " ";
            playerCount.text = session == null ? "PLAYERS  (0 / 4)" : $"PLAYERS  ({session.PlayerCount} / {session.MaxPlayers})";
            var players = session == null ? Array.Empty<IReadOnlyPlayer>() : session.Players.ToArray();
            foreach (string id in rows.Keys.Where(id => players.All(p => p.Id != id)).ToArray())
            {
                Destroy(rows[id].gameObject);
                rows.Remove(id);
            }
            int index = 0;
            foreach (var player in players.OrderByDescending(p => p.Id == session.Host))
            {
                if (!rows.TryGetValue(player.Id, out var row))
                {
                    row = Instantiate(playerRowPrefab, playerContent);
                    rows.Add(player.Id, row);
                }
                row.Bind(session, player, ShowError);
                row.transform.SetSiblingIndex(index++);
            }
            int vacant = session == null ? emptySlots.Length : Mathf.Clamp(session.MaxPlayers - session.PlayerCount, 0, emptySlots.Length);
            for (int i = 0; i < emptySlots.Length; i++)
            {
                emptySlots[i].SetActive(i < vacant);
                emptySlots[i].transform.SetAsLastSibling();
            }
        }

        public async void SetPlayerName(string value)
        {
            string name = NormalizePlayerName(value);
            playerNameInput.SetTextWithoutNotify(name);
            if (name.Length == 0)
            {
                ShowError("Enter your player name before creating or joining a session.");
                return;
            }
            PlayerPrefs.SetString(SavedNameKey, name);
            PlayerPrefs.Save();
            var session = CurrentSession();
            if (session == null || savingName) return;
            await PublishName(session, name);
        }

        public static string NormalizePlayerName(string value) => (value ?? "").Trim();
        public void SetSessionName(string value) => sessionNameInput.text = (value ?? "").Trim();
        private void NormalizeCode(string value) => joinCodeInput.SetTextWithoutNotify(string.Concat(value.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant());

        private async Task PublishName(ISession session, string name)
        {
            savingName = true;
            try
            {
                session.CurrentPlayer.SetProperty(PlayerNameProperty, new PlayerProperty(name, VisibilityPropertyOptions.Member));
                await session.SaveCurrentPlayerDataAsync();
                if (this && CurrentSession()?.Id == session.Id) RefreshSession(session);
            }
            catch (Exception error)
            {
                if (this) ShowError("Could not save player name: " + error.Message);
            }
            finally { if (this) savingName = false; }
        }

        public void ShowJoining()
        {
            joining = true;
            status.text = "Connecting to session...";
        }

        public async void ShowJoined(ISession session)
        {
            joining = false;
            status.text = "Share your session code to invite friends.";
            string name = NormalizePlayerName(playerNameInput.text);
            if (name.Length > 0)
            {
                PlayerPrefs.SetString(SavedNameKey, name);
                PlayerPrefs.Save();
                await PublishName(session, name);
            }
        }

        public void ShowFailure(SessionException error)
        {
            joining = false;
            ShowError("Could not join: " + error.Message);
        }

        public void ShowLeft()
        {
            joining = false;
            status.text = "Create a session or enter a friend's code.";
            RefreshSession(null);
        }

        private void ShowError(string message) => status.text = message;
    }
}

using System;
using System.Collections.Generic;
using Unity.Services.Multiplayer;

namespace CupOHappiness.UI.Editor
{
    public static class MultiplayerLobbyNameChecks
    {
        private sealed class Player : IReadOnlyPlayer
        {
            public string Id => "test-player";
            public string AllocationId => "";
            public DateTime Joined => DateTime.MinValue;
            public DateTime LastUpdated => DateTime.MinValue;
            public Dictionary<string, PlayerProperty> Data = new();
            public IReadOnlyDictionary<string, PlayerProperty> Properties => Data;
        }

        public static string Run()
        {
            Require(MultiplayerLobbyPanel.NormalizePlayerName(null) == "", "Null name");
            Require(MultiplayerLobbyPanel.NormalizePlayerName("   ") == "", "Blank name");
            Require(MultiplayerLobbyPanel.NormalizePlayerName("  Niki The Driver  ") == "Niki The Driver", "Name with spaces");
            Require(MultiplayerLobbyPanel.NormalizePlayerName("  ニキ  ") == "ニキ", "Unicode name");
            var player = new Player();
            Require(LobbyPlayerRow.DisplayName(player) == "Player", "Missing property fallback");
            player.Data["w_PlayerName"] = new PlayerProperty("Rook", VisibilityPropertyOptions.Member);
            Require(LobbyPlayerRow.DisplayName(player) == "Rook", "Session player property");
            player.Data["w_PlayerName"] = new PlayerProperty("Nikita", VisibilityPropertyOptions.Member);
            Require(LobbyPlayerRow.DisplayName(player) == "Nikita", "Live name update");
            player.Data["w_PlayerName"] = new PlayerProperty(" ", VisibilityPropertyOptions.Member);
            Require(LobbyPlayerRow.DisplayName(player) == "Player", "Blank property fallback");
            return "8 player-name checks passed.";
        }

        private static void Require(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException(label + " failed.");
        }
    }
}

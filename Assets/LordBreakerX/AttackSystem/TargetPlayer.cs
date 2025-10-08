using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TargetPlayer
{
    private ulong _clientID;

    public Transform PlayerTransform { get; private set; }

    public TargetPlayer(NetworkObject player, ulong clientID)
    {
        PlayerTransform = player.gameObject.transform;
        _clientID = clientID;
    }

    public static TargetPlayer GetPlayerWithID(ulong clientID, List<TargetPlayer> players)
    {
        foreach(TargetPlayer targetPlayer in players)
        {
            if (targetPlayer._clientID == clientID)
            {
                return targetPlayer;
            }
        }

        return null;
    }
}

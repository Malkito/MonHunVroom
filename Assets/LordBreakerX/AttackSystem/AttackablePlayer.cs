using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AttackablePlayer
{
    private ulong _clientID;

    public Transform PlayerTransform { get; private set; }

    public AttackablePlayer(NetworkObject player, ulong clientID)
    {
        PlayerTransform = player.gameObject.transform;
        _clientID = clientID;
    }

    public static AttackablePlayer GetPlayerEntryWithID(ulong clientID, List<AttackablePlayer> players)
    {
        foreach(AttackablePlayer attackablePlayer in players)
        {
            if (attackablePlayer._clientID == clientID)
            {
                return attackablePlayer;
            }
        }

        return null;
    }
}

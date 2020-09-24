using UnityEngine;
using System.Collections;

/// <summary>
/// Stores data about an Actor: Its team, its local player number (1-4, assuming the Actor holding this is a player), etc.
/// </summary>
public class IDData : MonoBehaviour
{
    public Color Team;
    public byte PlayerID;

    public void SetRenderColor()
    {
        if (!Team.Equals(ActorConstants.EnemiesTeam))
        {
            renderer.material.SetColor("_TeamCol", Team);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Covers a tunnel from a Roguelike generator.
/// </summary>
public class TunnelRegion : FilledRegion
{
    public bool IsHorizontal { get { return Covering.Width > Covering.Height; } }
    public bool IsVertical { get { return Covering.Width < Covering.Height; } }
    public bool IsSquare { get { return Covering.Width == Covering.Height; } }

    public TunnelRegion(Region covering, bool[,] map)
        : base(covering)
    {
        //Build spawn regions.

        PotentialSpawns[Spawns.Powerup].Add(Covering);
        PotentialSpawns[Spawns.Waypoint].Add(Covering);

        if (IsHorizontal)
            PotentialSpawns[Spawns.Team].Add(Covering);
    }

    public override string ToString()
    {
        return "Tunnel";
    }
}
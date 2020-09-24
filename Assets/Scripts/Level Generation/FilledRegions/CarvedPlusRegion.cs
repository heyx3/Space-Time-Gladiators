using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CarvedPlusRegion : FilledRegion
{
    public CarvedPlusRegion(Region covering)
        : base(covering)
    {
        Region r;

        //Use the hole in the middle for objectives.
        r = new Region(Covering.Center, Covering.Center);
        PotentialSpawns[Spawns.Powerup].Add(r);
        PotentialSpawns[Spawns.Waypoint].Add(r);

        //Get the size of one of the filled halves.
        //The CarvedPlusPattern is only suitable for square regions,
        //   so this value is the same for width and height.
        int spaces = Covering.Width + 1 - 2;
        spaces /= 2;

        //If it's wide enough, use the horizontal part of the cross as two spawns.
        if (spaces >= 4)
        {
            r = new Region(Covering.Left + 1, Covering.CenterY, spaces - 1, 0);
            PotentialSpawns[Spawns.Team].Add(r);
            r.X = r.Right + 2;
            PotentialSpawns[Spawns.Team].Add(r);
        }
    }

    public override string ToString()
    {
        return "Carved\nPlus";
    }
}
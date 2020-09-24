using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class XRegion : FilledRegion
{
    public XRegion(Region covering, bool[,] map)
        : base(covering)
    {
        //Available spawns are the center and the four points orthogonal to center just outside the X.

        PotentialSpawns[Spawns.Powerup].Add(new Region(Covering.Center, Covering.Center));
        PotentialSpawns[Spawns.Waypoint].Add(new Region(Covering.Center, Covering.Center));

        //Grab the first free map cell to the left of center.
        Location counter = Covering.Center.Left;
        while (map[counter.X, counter.Y] && Covering.Touches(counter, true, true, false))
        {
            counter = counter.Left;
            if (counter.X < 0) counter.X = map.GetLength(0) - 1;
        }
        if (Covering.Touches(counter, true, true, false))
        {
            PotentialSpawns[Spawns.Powerup].Add(new Region(counter, counter));
            PotentialSpawns[Spawns.Waypoint].Add(new Region(counter, counter));
        }

        //Right of center.
        counter = Covering.Center.Right;
        while (map[counter.X, counter.Y] && Covering.Touches(counter, true, true, false))
        {
            counter = counter.Right;
            if (counter.X >= map.GetLength(0))
                counter.X = 0;
        }
        if (Covering.Touches(counter, true, true, false))
        {
            PotentialSpawns[Spawns.Powerup].Add(new Region(counter, counter));
            PotentialSpawns[Spawns.Waypoint].Add(new Region(counter, counter));
        }

        //Above center.
        counter = Covering.Center.Above;
        while (map[counter.X, counter.Y] && Covering.Touches(counter, true, true, false))
        {
            counter = counter.Above;
            if (counter.Y < 0) counter.Y = map.GetLength(1) - 1;
        }
        if (Covering.Touches(counter, true, true, false))
        {
            PotentialSpawns[Spawns.Powerup].Add(new Region(counter, counter));
            PotentialSpawns[Spawns.Waypoint].Add(new Region(counter, counter));
        }

        //Below center.
        counter = Covering.Center.Below;
        while (map[counter.X, counter.Y] && Covering.Touches(counter, true, true, false))
        {
            counter = counter.Below;
            if (counter.Y >= map.GetLength(1)) counter.Y = 0;
        }
        if (Covering.Touches(counter, true, true, false))
        {
            PotentialSpawns[Spawns.Powerup].Add(new Region(counter, counter));
            PotentialSpawns[Spawns.Waypoint].Add(new Region(counter, counter));
        }
    }

    public override string ToString()
    {
        return "X";
    }
}

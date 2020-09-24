using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SteppedHallwayRegion : FilledRegion
{
    public SteppedHallwayRegion(Region covering, List<Region> freeAreas)
        : base(covering)
    {
        foreach (Region r in freeAreas)
        {
            PotentialSpawns[Spawns.Powerup].Add(r);
            PotentialSpawns[Spawns.Waypoint].Add(r);
            if (r.Width + 1 >= 4)
                PotentialSpawns[Spawns.Team].Add(r);
        }
    }

    public override string ToString()
    {
        return "Stepped\nHallway";
    }
}

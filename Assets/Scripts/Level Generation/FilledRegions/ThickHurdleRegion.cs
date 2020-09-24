using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ThickHurdleRegion : FilledRegion
{
    public ThickHurdleRegion(Region covering, List<Region> spacesBetweenHurdles)
        : base(covering)
    {
        foreach (Region r in spacesBetweenHurdles)
        {
            PotentialSpawns[Spawns.Powerup].Add(r);
            PotentialSpawns[Spawns.Waypoint].Add(r);
            if (r.Width + 1 >= 4)
                PotentialSpawns[Spawns.Team].Add(r);
        }
    }

    public override string ToString()
    {
        return "Thick\nHurdle";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PlatformsRegion : FilledRegion
{
    public PlatformsRegion(Region covering, ICollection<Region> platformGroups)
        : base(covering)
    {
        //Filter out any platforms that are too small.
        const int spacesNeeded = 4;
        foreach (Region r in platformGroups)
            if (r.Width + 1 >= spacesNeeded)
            {
                PotentialSpawns[Spawns.Team].Add(r);
                PotentialSpawns[Spawns.Powerup].Add(r);
                PotentialSpawns[Spawns.Waypoint].Add(r);
            }
    }

    public override string ToString()
    {
        return "Platforms";
    }
}

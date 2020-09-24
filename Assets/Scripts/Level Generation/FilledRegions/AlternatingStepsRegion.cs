using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AlternatingStepsRegion : FilledRegion
{
    public AlternatingStepsRegion(Region covering, IEnumerable<Region> platforms)
        : base(covering)
    {
        Region r;
        foreach (Region r2 in platforms)
        {
            r = new Region(r2.X, r2.Y - 1, r2.Width, r2.Height);

            if (r2.Width < 0 || r2.Height < 0) continue;

            PotentialSpawns[Spawns.Powerup].Add(r);
            PotentialSpawns[Spawns.Waypoint].Add(r);

            if (r.Width + 1 >= 4)
                PotentialSpawns[Spawns.Team].Add(r);
        }
    }

    public override string ToString()
    {
        return "Alternating\nSteps";
    }
}

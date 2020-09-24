using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CircleRegion : FilledRegion
{
    public CircleRegion(Region covering, double innerRadius)
        : base(covering)
    {
        int halfSideCenter = (int)(0.5 * Math.Sqrt(2.0) * innerRadius);

        Location center = Covering.Center;

        Region r = new Region(new Location(center.X - halfSideCenter, center.Y - halfSideCenter),
                              new Location(center.X + halfSideCenter, center.Y + halfSideCenter));
        PotentialSpawns[Spawns.Powerup].Add(r);
        PotentialSpawns[Spawns.Waypoint].Add(r);
    }

    public override string ToString()
    {
        return "Circle";
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LineRegion : FilledRegion
{
    public LineRegion(Region covered)
        : base(covered)
    {
        if (Covering.Width > Covering.Height)
        {
            Region topOfLine = new Region(Covering.TopLeft.Right, Covering.TopRight.Left);

            PotentialSpawns[Spawns.Powerup].Add(topOfLine);
            PotentialSpawns[Spawns.Waypoint].Add(topOfLine);

            //If the line is long enough, use it as a player spawn.
            if (topOfLine.Width + 1 >= 4)
                PotentialSpawns[Spawns.Team].Add(topOfLine);
        }
        else
        {
            int x = Covering.CenterX;
            Region topOfLine = new Region(x, Covering.Top, 0, 0);

            PotentialSpawns[Spawns.Powerup].Add(topOfLine);
            PotentialSpawns[Spawns.Waypoint].Add(topOfLine);
        }
    }

    public override string ToString()
    {
        return "Line";
    }
}
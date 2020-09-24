using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ConcentricSquaresRegion : FilledRegion
{
    public ConcentricSquaresRegion(Region covering, Region insideSmallestSquare)
        : base(covering)
    {
        //Wouldn't be very fair for a player to start in the middle of a bunch of concentric squares, so don't put player spawns in there.
        PotentialSpawns[Spawns.Powerup].Add(insideSmallestSquare);
        PotentialSpawns[Spawns.Waypoint].Add(insideSmallestSquare);

        //However, players could spawn above the squares or maybe inside the very first shell.
        Region r = new Region(Covering.TopLeft.Right, Covering.TopRight.Left);
        if (r.Width + 1 >= 4)
        {
            //Above the squares.
            PotentialSpawns[Spawns.Team].Add(r);
            PotentialSpawns[Spawns.Waypoint].Add(r);
            PotentialSpawns[Spawns.Powerup].Add(r);

            //Inside the first shell.
            r.Y += 2;
            r.X += 2;
            r.Width -= 4;
            if (r.Width + 1 >= 4)
            {
                //Do both the top and bottom of the shell.

                PotentialSpawns[Spawns.Team].Add(r);
                PotentialSpawns[Spawns.Waypoint].Add(r);
                PotentialSpawns[Spawns.Powerup].Add(r);

                r.Y = Covering.Bottom - 2;

                PotentialSpawns[Spawns.Team].Add(r);
                PotentialSpawns[Spawns.Waypoint].Add(r);
                PotentialSpawns[Spawns.Powerup].Add(r);
            }
        }
    }

    public override string ToString()
    {
        return "Concentric\nSquares";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ZipperRegion : FilledRegion
{
    private bool Horizontal { get { return Covering.Width > Covering.Height; } }
    private bool Vertical { get { return Covering.Height > Covering.Width; } }

    /// <summary>
    /// Creates a new zipper region.
    /// </summary>
    /// <param name="zipperBottomsTops">If the region was a horizontal corridor,
    /// the regions representing the top/bottom spaces above/below the plateaus.
    /// Otherwise, an empty collection.</param>
    public ZipperRegion(Region covering, ICollection<Region> zipperBottomsTops)
        : base(covering)
    {
        foreach (Region r in zipperBottomsTops)
        {
            if (r.Width + 1 >= 4)
                PotentialSpawns[Spawns.Team].Add(r);
            PotentialSpawns[Spawns.Powerup].Add(r);
        }
    }

    public override string ToString()
    {
        return "Zipper";
    }
}

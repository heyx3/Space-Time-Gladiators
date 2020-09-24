using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// A fill pattern for tall corridors that fill in steps
/// alternating on the left and right sides of the region.
/// </summary>
public class AlternatingStepsPattern : FillPattern
{
    public const int MaxWidth = 7;
    public const int MinWidth = 3;

    //TODO: More customizability.

    /// <summary>
    /// The width of a single step as a proportion of the region size.
    /// </summary>
    public const float StepSizeScale = 0.37f;

    public const int Space = 2;
    public const int MinSteps = 4;
    public const int PlatformThickness = 1;
    public int MinHeight
    {
        get
        {
            return 2 + (PlatformThickness * MinSteps) + (Space * (MinSteps - 1));
        }
    }
    
    public Suitability MaxSuitability { get; set; }

    public AlternatingStepsPattern() { MaxSuitability = Suitability.Very; }

    public Suitability GetSuitability(Region r)
    {
        if (r.Width + 1 > MaxWidth ||
            r.Width + 1 < MinWidth ||
            r.Height + 1 < MinHeight)
            return Suitability.Not;

        return MaxSuitability;
    }

    public FilledRegion Apply(FillData data)
    {
        //Keep track of the spawn areas above the steps, indexed by y coordinate.
        Dictionary<int, Region> platformSpaces = new Dictionary<int, Region>();

        //Get step data.
        int stepWidth = (int)Math.Round(StepSizeScale * (data.BeingFilled.Width + 1), 0);
        int spaceWidth = data.BeingFilled.Width + 1 - stepWidth - stepWidth;
        bool left = true;

        Location line1, line2;

        for (int y = data.BeingFilled.Bottom - 1; y > data.BeingFilled.Top; y -= Space)
        {
            //Fill the line.
            if (left)
            {
                line1 = new Location(data.BeingFilled.Left, y);
                line2 = new Location(data.BeingFilled.Left + stepWidth - 1, y);

                data.FillLine(true, line1, line2);
                platformSpaces.Add(y, new Region(line1, line2, true));
            }
            else
            {
                line1 = new Location(data.BeingFilled.Left + stepWidth + spaceWidth, y);
                line2 = new Location(data.BeingFilled.Right, y);

                data.FillLine(true, line1, line2);
                platformSpaces.Add(y, new Region(line1, line2, true));
            }

            //Change sides.
            left = !left;
        }

        //Free any holes.
        foreach (Location l in data.HolesAlongPerimeter())
            //If the hole is on the left side and there's a platform in the way:
            if (data.BeingFilled.Touches(l.Right, true, true, false) &&
                data.GetMapAt(l.Right))
            {
                //Remove the left edge of the platform.

                data.SetMapAt(l.Right, false);

                Region r = platformSpaces[l.Y];
                ++r.X;
                --r.Width;
                platformSpaces[l.Y] = r;

            }
            //Otherwise, if the hole is on the right side and there's a platform in the way:
            else if (data.BeingFilled.Touches(l.Left, true, true, false) &&
                     data.GetMapAt(l.Left))
            {
                //Remove the right edge of the platform.

                data.SetMapAt(l.Left, false);

                Region r = platformSpaces[l.Y];
                --r.Width;
                platformSpaces[l.Y] = r;
            }

        return new AlternatingStepsRegion(data.BeingFilled, platformSpaces.Values);
    }
}
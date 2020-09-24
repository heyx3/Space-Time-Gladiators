using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Fills a region with an outer full circle of walls, and an inner full circle of air.
/// </summary>
public class CirclePattern : FillPattern
{
    public int InnerCircle;

    public Suitability MaxSuitability { get; set; }

    public CirclePattern(int innerRadius)
    {
        InnerCircle = innerRadius;
        MaxSuitability = Suitability.Very;
    }

    public Suitability GetSuitability(Region r)
    {
        //Only applies to square regions with an odd number of spaces that are large enough.
        if (r.Width == r.Height &&
            r.Width % 2 == 0 &&
            r.Width >= (InnerCircle * 2) + 2)
            return MaxSuitability;

        return Suitability.Not;
    }

    public FilledRegion Apply(FillData data)
    {
        float OuterCircle = data.BeingFilled.Width * 0.5f;

        if (OuterCircle <= InnerCircle)
            throw new ArgumentOutOfRangeException("The outer radius has to be larger than the inner radius!");

        Location center = data.BeingFilled.Center;

        //Make the circles.
        data.FillCircle(true, center, OuterCircle);
        data.FillCircle(false, center, InnerCircle);

        //Make the lines to get to the inner circle.
        data.FillLine(false, center, data.BeingFilled.BottomMid);
        data.FillLine(false, center, data.BeingFilled.TopMid);
        data.FillLine(false, center, data.BeingFilled.LeftMid);
        data.FillLine(false, center, data.BeingFilled.RightMid);

        //For spawn points, give the two points just to the left/right of the inner circle.
        return new CircleRegion(data.BeingFilled, InnerCircle);
    }
}

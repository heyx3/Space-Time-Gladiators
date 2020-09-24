using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
    /// Fills the region with a solid block, then carves out centered vertical and horizontal lines.
    /// </summary>
public class CarvedPlusPattern : FillPattern
{
    public Suitability MaxSuitability { get; set; }

    public CarvedPlusPattern() { MaxSuitability = Suitability.Very; }

    public Suitability GetSuitability(Region r)
    {
        if (r.Width == r.Height && r.Area > 10 && r.Width % 2 == 0) return MaxSuitability;
        else return Suitability.Not;
    }
    public FilledRegion Apply(FillData data)
    {
        //Fill the square.
        data.FillRegion(true, new Region(data.BeingFilled.TopLeft.Right.Below, data.BeingFilled.BottomRight.Left.Above));

        //Create the lines.
        Region horz = new Region(data.BeingFilled.LeftMid.Right, data.BeingFilled.Center.Left);
        Region horz2 = new Region(data.BeingFilled.Center.Right, data.BeingFilled.RightMid.Left);
        Region vert = new Region(data.BeingFilled.TopMid.Below, data.BeingFilled.Center.Above);
        Region vert2 = new Region(data.BeingFilled.Center.Below, data.BeingFilled.BottomMid.Above);
        data.FillRegion(false, horz);
        data.FillRegion(false, horz2);
        data.FillRegion(false, vert);
        data.FillRegion(false, vert2);

        Region center = new Region(data.BeingFilled.Center, data.BeingFilled.Center);
        data.Holes.Add(center.Center);
        data.FillRegion(false, center);

        return new CarvedPlusRegion(data.BeingFilled);
    }
}

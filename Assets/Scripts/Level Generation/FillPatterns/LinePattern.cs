using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// A pattern for filling very thin horizontal/vertical corridors when no other pattern fits.
/// Puts a single line through the middle of the corridor.
/// </summary>
public class LinePattern : FillPattern
{
    public const int MaxWidth = 4;

    public Suitability MaxSuitability { get; set; }

    public LinePattern()
    {
        MaxSuitability = Suitability.Very;
    }

    public Suitability GetSuitability(Region r)
    {
        bool horizontal = r.Width > r.Height;
        bool vertical = r.Height > r.Width;

        if ((horizontal && r.Height < MaxWidth && r.Height >= 2) ||
            (vertical && r.Width < MaxWidth && r.Width >= 2))
            return MaxSuitability;
        else return Suitability.Not;
    }

    public FilledRegion Apply(FillData data)
    {
        bool horizontal = data.BeingFilled.Width > data.BeingFilled.Height;
        bool vertical = data.BeingFilled.Height > data.BeingFilled.Width;

        //Put a thin line through the middle.
        if (horizontal)
            data.FillRegion(true, new Region(data.BeingFilled.Left + 1, data.BeingFilled.CenterY, data.BeingFilled.Width - 2, 0, true));
        else if (vertical)
            data.FillRegion(true, new Region(data.BeingFilled.CenterX, data.BeingFilled.Top + 1, 0, data.BeingFilled.Height - 2, true));

        return new LineRegion(data.BeingFilled);
    }
}
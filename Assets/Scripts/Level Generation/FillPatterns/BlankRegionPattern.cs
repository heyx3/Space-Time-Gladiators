using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Leaves all regions smaller than a given size blank.
/// </summary>
public class BlankRegionPattern : FillPattern
{
    /// <summary>
    /// The largest region area to leave blank.
    /// </summary>
    public float MaxRegionArea;

    public Suitability MaxSuitability { get; set; }

	public BlankRegionPattern()
		: this(4) { }
    public BlankRegionPattern(float maxRegionArea)
    {
        MaxRegionArea = maxRegionArea;
        MaxSuitability = Suitability.Very;
    }

    public Suitability GetSuitability(Region r)
    {
        if (r.Area <= MaxRegionArea || r.Width == 0 || r.Height == 0) return Suitability.Very;
        else return Suitability.Not;
    }
    public FilledRegion Apply(FillData data) { return new BlankRegion(data.BeingFilled); }
}

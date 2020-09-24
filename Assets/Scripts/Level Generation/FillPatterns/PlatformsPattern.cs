using System;
using System.Collections.Generic;

/// <summary>
    /// Puts down random, evenly-spaced platforms.
    /// </summary>
public class PlatformsPattern : FillPattern
{
    //TODO: Make an ideal number of platforms.

    public int SpaceBetween;
    public float PercentageHoles;
    public int Border;

    /// <summary>
    /// Gets or sets the maximum-allowable Suitability for this pattern to any region.
    /// </summary>
    public Suitability MaxSuitability { get; set; }

    public int MinArea;
    public Interval AcceptableXToYRatios;

    public PlatformsPattern(int spaceBetweenPlatforms, int border, float percentOfPlatformsAreHoles,
                            int minAcceptableArea, Interval acceptableXToYRatios)
    {
        SpaceBetween = spaceBetweenPlatforms;
        PercentageHoles = percentOfPlatformsAreHoles;
        Border = border;

        MinArea = minAcceptableArea;
        AcceptableXToYRatios = acceptableXToYRatios;

        MaxSuitability = Suitability.Very;
    }

    public Suitability GetSuitability(Region r)
    {
        //Make sure the region is large enough.
        if (r.Area < MinArea) return Suitability.Not;
        if (r.Height < 2 * SpaceBetween) return Suitability.Not;
        if (r.Width < 2 * Border) return Suitability.Not;

        //Make sure the region has a good ratio.
        return AcceptableXToYRatios.Inside((float)r.Width / r.Height) ? MaxSuitability : Suitability.Not;
    }

    public FilledRegion Apply(FillData data)
    {
        //Keep track of the platforms as individual spawning regions.
        List<Region> playSpawns = new List<Region>();
        int startingX;

        //Use a counter.
        for (int y = data.BeingFilled.Bottom - SpaceBetween; y >= data.BeingFilled.Top + SpaceBetween; y -= SpaceBetween)
        {
            startingX = data.BeingFilled.Left + Border;

            //Go through each block in the current line.
            int x;
            for (x = data.BeingFilled.Left + Border; x <= data.BeingFilled.Right - Border; ++x)
            {
                //With a certain chance, fill it.
                if (MathF.R.NextDouble() > PercentageHoles)
                {
                    data.SetMapAt(new Location(x, y), true);
                }
                //Otherwise, cut off the platform region here.
                else
                {
                    playSpawns.Add(new Region(new Location(startingX, y - 1), new Location(x - 1, y - 1)));
                    startingX = x + 1;
                }
            }

            playSpawns.Add(new Region(new Location(startingX, y - 1), new Location(x - 1, y - 1)));
        }

        return new PlatformsRegion(data.BeingFilled, playSpawns);
    }
}
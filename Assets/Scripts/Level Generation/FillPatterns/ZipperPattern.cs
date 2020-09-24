using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
    /// Creates a series of opposing plateaus, forming a zipper patten.
    /// </summary>
public class ZipperPattern : FillPattern
{
    public PlateauGenerationProperties Generation;

    public Suitability MaxSuitability { get; set; }

    /// <summary>
    /// An interval occupying some subset of [0, 1].
    /// Represents the allowable ratios of width/height (or height/width) for this pattern to be applicable to a corridor.
    /// </summary>
    public Interval AcceptableRegionRatios;

    public int MinArea;

    public ZipperPattern(PlateauGenerationProperties plateauGen, Interval acceptableRatios, int minArea)
    {
        Generation = plateauGen;
        AcceptableRegionRatios = acceptableRatios;
        MinArea = minArea;

        MaxSuitability = Suitability.Very;
    }

    public Suitability GetSuitability(Region r)
    {
        Generation.BeingFilled = r;

        //Flip the region for the plateau calculations if the region is vertical.
        if (r.Height > r.Width) Generation.BeingFilled = new Region(r.X, r.Y, r.Height, r.Width);

        //If the region isn't long/wide enough, exit.
        if (Generation.BeingFilled.Width < Generation.MinRegionWidth)
            return Suitability.Not;
        if (Generation.BeingFilled.Height < 1) return Suitability.Not;

        //Use the ratio to find the suitability.
        float ratio = (float)r.Width / r.Height;
        if (ratio > 1.0f) ratio = 1.0f / ratio;
        Suitability ret = AcceptableRegionRatios.Inside(ratio) ? MaxSuitability : Suitability.Not;

        //If the plateaus won't fit in perfectly, don't use this pattern.

        int space = Generation.BeingFilled.Width + 1;
        space -= 2 * Generation.Border;
        int plateauSpace = Generation.SpaceBetweenPlateaus;
        int width = Generation.PlateauWidth;
        int numb = Generation.NumbPlateaus;
        bool bad = space - (numb * width) - ((numb - 1) * plateauSpace) != 0;

        if (bad) return Suitability.Not;
        else return ret;
    }

    public FilledRegion Apply(FillData data)
    {
        //Start on either the relative top or bottom of the corridor.
        bool top = MathF.R.NextDouble() > 0.5;

        //Set up the plateau properties.
        bool horizontal = data.BeingFilled.Width > data.BeingFilled.Height;
        if (horizontal) Generation.BeingFilled = data.BeingFilled;
        else Generation.BeingFilled = new Region(data.BeingFilled.Y,
                                                 data.BeingFilled.X,
                                                 data.BeingFilled.Height,
                                                 data.BeingFilled.Width);

        int spaceBetween = Generation.SpaceBetweenPlateaus;
        int border = Generation.Border;
        int plateauWidth = Generation.PlateauWidth;
        int numbPlateaus = Generation.NumbPlateaus;

        //Fill the area. Also keep track of the free space above/below plateaus.
        List<Region> freeAreas = new List<Region>();
        for (int i = 0; i < numbPlateaus; ++i)
        {
            if (top)
            {
                if (horizontal)
                {
                    data.FillRegion(true, new Region(data.BeingFilled.Left + border + (i * (spaceBetween + plateauWidth)),
                                                     data.BeingFilled.Top,
                                                     plateauWidth - 1,
                                                     data.BeingFilled.Height - 1));
                    freeAreas.Add(new Region(data.BeingFilled.Left + border + (i * (spaceBetween + plateauWidth)),
                                             data.BeingFilled.Bottom,
                                             plateauWidth - 1, 0));
                }
                else
                {
                    data.FillRegion(true, new Region(data.BeingFilled.Left,
                                                     data.BeingFilled.Top + border + (i * (spaceBetween + plateauWidth)),
                                                     data.BeingFilled.Width - 1,
                                                     plateauWidth - 1));
                    freeAreas.Add(new Region(data.BeingFilled.Left + 1,
                                             data.BeingFilled.Top + border + (i * (spaceBetween + plateauWidth)) - 1,
                                             data.BeingFilled.Width - 2, 0));
                }
            }
            else
            {
                if (horizontal)
                {
                    data.FillRegion(true, new Region(data.BeingFilled.Left + border + (i * (spaceBetween + plateauWidth)),
                                                     data.BeingFilled.Top + 1,
                                                     plateauWidth - 1,
                                                     data.BeingFilled.Height - 1));
                    freeAreas.Add(new Region(data.BeingFilled.Left + border + (i * (spaceBetween + plateauWidth)),
                                             data.BeingFilled.Top,
                                             plateauWidth - 1, 0));
                }
                else
                {
                    data.FillRegion(true, new Region(data.BeingFilled.Left + 1,
                                                     data.BeingFilled.Top + border + (i * (spaceBetween + plateauWidth)),
                                                     data.BeingFilled.Width - 1,
                                                     plateauWidth - 1));
                    freeAreas.Add(new Region(data.BeingFilled.Left + 1,
                                             data.BeingFilled.Top + border + (i * (spaceBetween + plateauWidth)) - 1,
                                             data.BeingFilled.Width - 2, 0));
                }
            }

            top = !top;
        }

        Region area = data.BeingFilled;
        //Make room for any holes that the plateaus covered.
        //TODO: Instead of clearing out the bottom, just clear that column of plateau. Possibly also do this for other plateau patterns?
        foreach (Location l in data.HolesAlongPerimeter())
        {
            //Horizontal.
            if (area.Height < area.Width)
            {
                //The hole is covered if it is just below/above the region and the space above/below it (respectively) is filled.
                if ((area.Touches(l.Above, true, true, false) && data.GetMapAt(l.Above)) ||
                    (area.Touches(l.Below, true, true, false) && data.GetMapAt(l.Below)))
                {
                    //Make a little tunnel under the plateau.
                    int[] plateauSides = SteppedHallwayPattern.PlateauSides(l.X, numbPlateaus, plateauWidth, spaceBetween, border, area.Left, area.Right);

                    Region cleared;
                    if (l.Y - 1 == area.Bottom)
                        cleared = new Region(plateauSides[0], data.BeingFilled.Bottom, plateauWidth - 1, 0);
                    else cleared = new Region(plateauSides[0], data.BeingFilled.Top, plateauWidth - 1, 0);

                    data.FillRegion(false, cleared);
                }
            }

            //Vertical
            else if ((area.Touches(l.Left, true, true, false) && data.GetMapAt(l.Left)) ||
                     (area.Touches(l.Right, true, true, false) && data.GetMapAt(l.Right)))
            {
                //Make a little tunnel under the plateau.
                int[] plateauSides = SteppedHallwayPattern.PlateauSides(l.Y, numbPlateaus, plateauWidth, spaceBetween, border, area.Top, area.Bottom);

                Region cleared;
                if (l.X - 1 == area.Right)
                    cleared = new Region(data.BeingFilled.Right, plateauSides[0], 0, plateauWidth - 1);
                else cleared = new Region(data.BeingFilled.Left, plateauSides[0], 0, plateauWidth - 1);

                data.FillRegion(false, cleared);
            }
        }

        if (horizontal)
        {
            return new ZipperRegion(data.BeingFilled, freeAreas);
        }
        else
        {
            return new ZipperRegion(data.BeingFilled, new List<Region>());
        }
    }
}
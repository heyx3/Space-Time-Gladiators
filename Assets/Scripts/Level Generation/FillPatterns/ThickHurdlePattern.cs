using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    /// <summary>
    /// Fills the region with "hurdles": plateau pairs that sit opposite each other along the corridor's sides (either vertical or horizontal).
    /// </summary>
public class ThickHurdlePattern : FillPattern
{
    public const int IdealHeight = 6;

    public Suitability MaxSuitability { get; set; }

    /// <summary>
    /// The height of a plateau (as a percentage of the region's height).
    /// </summary>
    public float HurdleAverageSpaceScale;
    /// <summary>
    /// The maximum possible deviance from the average height of a hurdle (as a percentage of the region height).
    /// </summary>
    public float HurdleHeightVariance;

    public PlateauGenerationProperties PlateauGeneration;

    public ThickHurdlePattern(PlateauGenerationProperties plateaus, float averageHeightScale, float heightVariance)
    {
        HurdleAverageSpaceScale = averageHeightScale;
        HurdleHeightVariance = heightVariance;

        PlateauGeneration = plateaus;

        MaxSuitability = Suitability.Very;
    }

    public Suitability GetSuitability(Region r)
    {
        PlateauGeneration.BeingFilled = r;

        bool horizontal = r.Height <= r.Width;
        bool vertical = r.Width < r.Height;

        if (vertical)
            PlateauGeneration.BeingFilled = new Region(r.X, r.Y, r.Height, r.Width);
        else if (!horizontal) return Suitability.Not;

        //Plateau data.
        int space = PlateauGeneration.BeingFilled.Width + 1;
        space -= 2 * PlateauGeneration.Border;
        int plateauSpace = PlateauGeneration.SpaceBetweenPlateaus;
        int width = PlateauGeneration.PlateauWidth;
        int numb = PlateauGeneration.NumbPlateaus;

        //If the plateaus won't fit in perfectly, lower the suitability.
        bool perfectFit = (space - (numb * width) - ((numb - 1) * plateauSpace) == 0);

        if (horizontal)
        {
            //Make sure it's within acceptable bounds.
            if (r.Height < 1 || r.Width < PlateauGeneration.MinRegionWidth) return Suitability.Not;

            //Get a suitability based on the height of the region.
            Suitability ret = (new Interval(IdealHeight * 2, IdealHeight, 3).Inside(r.Height)) ?
                                MaxSuitability :
                                Suitability.Not;

            //Check to see if the plateaus will fit perfectly.
            if (!perfectFit)
                return Suitability.Not;
            else return ret;
        }

        if (vertical)
        {
            //Make sure it's within acceptable bounds.
            if (r.Width < 1 || r.Height < PlateauGeneration.MinRegionWidth) return Suitability.Not;

            //Get a value based on the width of the region (too wide and the region will be rejected).
            Suitability ret = (new Interval(IdealHeight * 2, IdealHeight, 3).Inside(r.Width)) ?
                                MaxSuitability :
                                Suitability.Not;

            //Check to see if the plateaus will fit perfectly.
            if (!perfectFit)
                return Suitability.Not;
            else return ret;
        }

        throw new InvalidOperationException("wat");
    }

    public FilledRegion Apply(FillData data)
    {
        //The spawning areas.
        List<Region> spawningAreas = new List<Region>();

        //Get/set some data.
        Region area = data.BeingFilled;
        PlateauGeneration.BeingFilled = data.BeingFilled;
        //If the corridor is going to be vertical, flip the width and height.
        if (area.Height > area.Width)
            PlateauGeneration.BeingFilled = new Region(area.Y, area.X, area.Height, area.Width);

        int numb = PlateauGeneration.NumbPlateaus;
        int plateauSize = PlateauGeneration.PlateauWidth;

        int space = PlateauGeneration.SpaceBetweenPlateaus;
        int border = PlateauGeneration.Border;

        //Plateau size data.
        //"maxSize" is the largest allowable hurdle height.
        int maxSize = PlateauGeneration.BeingFilled.Height - 1;

        if (area.Height > area.Width)
        {
            //Vertical.

            //Add the plateaus.
            int hurdleWidth, plateauExtent;
            for (int i = 0; i < numb; ++i)
            {
                //Get a width within the given random variation.
                hurdleWidth = (int)Math.Round((area.Width + 1) * (HurdleAverageSpaceScale + ((2 * HurdleHeightVariance * (float)GeneratorSettings.R.NextDouble()) - HurdleHeightVariance)), 0);
                //Keep it constrained.
                if (hurdleWidth > maxSize) hurdleWidth = maxSize;
                if (hurdleWidth < 1) hurdleWidth = 1;

                //Get the width of the plateau given the size of a hurdle.
                plateauExtent = area.Width + 1 - hurdleWidth;
                plateauExtent /= 2;

                //Fill in the plateaus.
                data.FillRegion(true, new Region(area.Left, area.Top + border + (i * (space + plateauSize)), plateauExtent - 1, plateauSize - 1));
                if ((2 * plateauExtent) + hurdleWidth >= area.Width + 1)
                    data.FillRegion(true, new Region(area.Right - plateauExtent + 1, area.Top + border + (i * (space + plateauSize)), plateauExtent - 1, plateauSize - 1));
                else data.FillRegion(true, new Region(area.Right - plateauExtent, area.Top + border + (i * (space + plateauSize)), plateauExtent, plateauSize - 1));
            }

            //Make room for any holes that the plateaus covered.
            foreach (Location l in data.HolesAlongPerimeter())
                //The hole is covered if it is just left/right of the region and the space just right/left of it (respectively) is filled.
                if ((area.Touches(l.Left, true, true, false) && data.GetMapAt(l.Left)) ||
                    (area.Touches(l.Right, true, true, false) && data.GetMapAt(l.Right)))
                {
                    //Make a little tunnel under the plateau.
                    int[] plateauSides = SteppedHallwayPattern.PlateauSides(l.Y, numb, plateauSize, space, border, area.Top, area.Bottom);
                    Region cleared;
                    if (l.X - 1 == area.Right)
                        cleared = new Region(data.BeingFilled.Right, plateauSides[0], 0, plateauSize - 1);
                    else cleared = new Region(data.BeingFilled.Left, plateauSides[0], 0, plateauSize - 1);
                    data.FillRegion(false, cleared);
                }
        }
        else
        {
            //Horizontal.

            //Add the plateaus.
            int hurdleHeight, plateauHeight;
            for (int i = 0; i < numb; ++i)
            {
                //Get a height within the given random variation.
                hurdleHeight = (int)Math.Round((area.Height + 1) * (HurdleAverageSpaceScale + ((2 * HurdleHeightVariance * (float)GeneratorSettings.R.NextDouble()) - HurdleHeightVariance)), 0);
                //Keep it constrained.
                if (hurdleHeight > maxSize) hurdleHeight = maxSize;
                if (hurdleHeight < 1) hurdleHeight = 1;

                //Get the height of the plateau given the size of a hurdle.
                plateauHeight = area.Height + 1 - hurdleHeight;
                plateauHeight /= 2;

                //Fill in the plateaus.
                data.FillRegion(true, new Region(area.Left + border + (i * (space + plateauSize)), area.Top, plateauSize - 1, plateauHeight - 1));
                if ((2 * plateauHeight) + hurdleHeight >= area.Height + 1)
                {
                    data.FillRegion(true, new Region(area.Left + border + (i * (space + plateauSize)), area.Bottom - plateauHeight + 1, plateauSize - 1, plateauHeight - 1));
                    spawningAreas.Add(new Region(area.Left + border + (i * (space + plateauSize)), area.Top + plateauHeight, plateauSize - 1, data.BeingFilled.Height - plateauHeight - plateauHeight - 2));
                }
                else
                {
                    data.FillRegion(true, new Region(area.Left + border + (i * (space + plateauSize)), area.Bottom - plateauHeight, plateauSize - 1, plateauHeight));
                    spawningAreas.Add(new Region(area.Left + border + (i * (space + plateauSize)), area.Top + plateauHeight, plateauSize - 1, data.BeingFilled.Height - plateauHeight - plateauHeight - 1));
                }
            }
        }


        //Make room for any holes that the plateaus covered.
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
                    int[] plateauSides = SteppedHallwayPattern.PlateauSides(l.X, numb, plateauSize, space, border, area.Left, area.Right);

                    Region cleared;
                    if (l.Y - 1 == area.Bottom)
                        cleared = new Region(plateauSides[0], data.BeingFilled.Bottom, plateauSize - 1, 0);
                    else cleared = new Region(plateauSides[0], data.BeingFilled.Top, plateauSize - 1, 0);

                    data.FillRegion(false, cleared);
                }
            }

            //Vertical.
            else if ((area.Touches(l.Left, true, true, false) && data.GetMapAt(l.Left)) ||
                     (area.Touches(l.Right, true, true, false) && data.GetMapAt(l.Right)))
            {
                //Make a little tunnel under the plateau.
                int[] plateauSides = SteppedHallwayPattern.PlateauSides(l.Y, numb, plateauSize, space, border, area.Top, area.Bottom);

                Region cleared;
                if (l.X - 1 == area.Right)
                    cleared = new Region(data.BeingFilled.Right, plateauSides[0], 0, plateauSize - 1);
                else cleared = new Region(data.BeingFilled.Left, plateauSides[0], 0, plateauSize - 1);

                data.FillRegion(false, cleared);
            }
        }

        //Return the new regions.
        return new ThickHurdleRegion(data.BeingFilled, spawningAreas);
    }
}
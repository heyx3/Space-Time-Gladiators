using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
    /// Takes a horizontal corridor and adds periodic plateaus to it.
    /// </summary>
public class SteppedHallwayPattern : FillPattern
{
    public const int IdealHeight = 4;

    public float PlateauRelativeHeight;

    public PlateauGenerationProperties Generation;

    public Suitability MaxSuitability { get; set; }

    /// <summary>
    /// Creates a new SteppedHallwayPattern with the given properties.
    /// </summary>
    /// <param name="plateauRelativeHeight">From 0.0-1.0, the height of a plateau relative to the height of the region.</param>
    /// <param name="plateauRelativeHeightMaxVariation">The maximum allowable variation in the plateau height.</param>
    public SteppedHallwayPattern(PlateauGenerationProperties plateauGeneration, float plateauRelativeHeight)
    {
        PlateauRelativeHeight = Math.Max(0, Math.Min(1, plateauRelativeHeight));

        Generation = plateauGeneration;

        MaxSuitability = Suitability.Very;
    }

    public Suitability GetSuitability(Region r)
    {
        Generation.BeingFilled = r;

        //Make sure the region is tall/wide enough.
        if (r.Width < Generation.MinRegionWidth)
            return Suitability.Not;
        if (r.Height < 1) return Suitability.Not;

        //Get a suitability based on the height of the region.
        Suitability ret = (new Interval(IdealHeight * 2.0f, IdealHeight, 3).Inside(r.Height)) ?
                            MaxSuitability :
                            Suitability.Not;

        //If the plateaus won't fit in perfectly, lower the suitability.
        int space = r.Width + 1;
        space -= 2 * Generation.Border;
        int plateauSpace = Generation.SpaceBetweenPlateaus;
        int width = Generation.PlateauWidth;
        int numb = Generation.NumbPlateaus;
        bool bad = space - (numb * width) - ((numb - 1) * plateauSpace) != 0;

        if (bad)
            return Suitability.Not;
        return ret;
    }
    public FilledRegion Apply(FillData data)
    {
        List<Region> spawnAreas = new List<Region>();

        //Get data.
        Generation.BeingFilled = data.BeingFilled;
        int numb = Generation.NumbPlateaus;
        int width = Generation.PlateauWidth;
        int space = Generation.SpaceBetweenPlateaus;
        int border = Generation.Border;

        //Plateau location data.
        int height = (int)Math.Round((PlateauRelativeHeight * data.BeingFilled.Height), 0);
        if (height >= data.BeingFilled.Height) height = data.BeingFilled.Height - 1;
        int top = data.BeingFilled.Bottom - height;

        //Add the plateaus.
        Region r;
        for (int i = 0; i < numb; ++i)
        {
            r = new Region(data.BeingFilled.Left + border + (i * (space + width)), top, width - 1, data.BeingFilled.Bottom - top);
            data.FillRegion(true, r);
            spawnAreas.Add(new Region(new Location(r.Left, data.BeingFilled.Top),
                                      r.TopRight.Above));
        }

        //Make room for any holes that the plateaus covered.
        foreach (Location l in data.HolesAlongPerimeter())
            //The hole is covered if it is just under the region and the space right above it is covered.
            if (data.BeingFilled.Touches(l.Above, true, true, false) &&
                data.GetMapAt(l.Above))
            {
                //Make a litle tunnel under the plateau.
                int[] plateauSides = PlateauSides(l.X, numb, width, space, border, data.BeingFilled.Left, data.BeingFilled.Right);
                Region cleared = new Region(plateauSides[0], data.BeingFilled.Bottom, width - 1, 0);
                data.FillRegion(false, cleared);
            }

        return new SteppedHallwayRegion(data.BeingFilled, spawnAreas);
    }
    /// <summary>
    /// Takes an x coordinate and gets the start and end x coordinates of the plateau/space it is inside.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the given coordinate isn't inside a plateau.</exception>
    public static int[] PlateauSides(int xCoord, int numb, int width, int space, int border, int left, int right)
    {
        int[] sides = new int[2] { left + border, left + border + width - 1 };

        int amount = width + space;
        while (true)
        {
            if (xCoord >= sides[0] && xCoord <= sides[1])
                return sides;

            sides[0] += amount;
            sides[1] += amount;

            if (sides[1] > right)
                throw new ArgumentOutOfRangeException("The coordinate wasn't in a plateau!");
        }
    }
}
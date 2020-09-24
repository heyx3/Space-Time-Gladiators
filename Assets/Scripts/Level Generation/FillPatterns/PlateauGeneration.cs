using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Holds properties for the generation of a plateau (for use in several fill patterns).
/// </summary>
public interface PlateauGenerationProperties
{
    Region BeingFilled { get; set; }

    int NumbPlateaus { get; }
    int PlateauWidth { get; }

    int SpaceBetweenPlateaus { get; }
    int Border { get; }

    int MinRegionWidth { get; }
}
    
/// <summary>
/// Generates plateaus with a fixed number of them and a fixed space between them.
/// </summary>
public class PlateauFixedNumbAndSpace : PlateauGenerationProperties
{
    public Region BeingFilled { get; set; }

    public int NumbPlateaus { get; set; }
    public int PlateauWidth
    {
        get
        {
            //Get the available width for plateaus.
            int width = BeingFilled.Width + 1;
            width -= 2 * Border;
            width -= (NumbPlateaus - 1) * SpaceBetweenPlateaus;

            return width / NumbPlateaus;
        }
    }

    public int SpaceBetweenPlateaus { get; set; }
    public int Border { get; set; }

    public int MinRegionWidth
    {
        get { return (2 * Border) + (SpaceBetweenPlateaus * (NumbPlateaus - 1)) + NumbPlateaus; }
    }

    public PlateauFixedNumbAndSpace(int numbPlateaus, int spaceBetweenPlateaus, int border)
    {
        NumbPlateaus = numbPlateaus;

        SpaceBetweenPlateaus = spaceBetweenPlateaus;
        Border = border;
    }
}

/// <summary>
    /// A plateau generator that keeps the plateaus at a fixed width with fixed spacing in between them.
    /// </summary>
public class PlateauFixedWidthAndSpace : PlateauGenerationProperties
{
    public Region BeingFilled { get; set; }

    public int NumbPlateaus
    {
        get
        {
            //Get the width left for putting plateaus in (including spaces).
            int spaces = BeingFilled.Width + 1;
            spaces -= 2 * Border;

            //Continuously add platforms on until we run out of space.
            int numbPlats = 0;
            while (spaces >= PlateauWidth)
            {
                numbPlats += 1;
                spaces -= PlateauWidth + SpaceBetweenPlateaus;
            }
            return numbPlats;
        }
    }
    public int PlateauWidth { get; set; }

    public int SpaceBetweenPlateaus { get; set; }
    public int Border { get; set; }

    public int MinRegionWidth
    {
        get { return (2 * Border) + PlateauWidth; }
    }

    public PlateauFixedWidthAndSpace(int plateauWidth, int spaceBetween, int border)
    {
        PlateauWidth = plateauWidth;
        SpaceBetweenPlateaus = spaceBetween;
        Border = border;
    }
}

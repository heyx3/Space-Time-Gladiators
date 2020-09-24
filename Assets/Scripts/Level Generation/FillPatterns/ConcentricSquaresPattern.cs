using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// A fill pattern that fills a square region with a series of square wall "shells",
/// one inside another, with a single hole in each shell.
/// </summary>
public class ConcentricSquaresPattern : FillPattern
{
    public const int MinArea = 9 * 9;

    public Suitability MaxSuitability { get; set; }

    public ConcentricSquaresPattern() { MaxSuitability = Suitability.Very; }

    public Suitability GetSuitability(Region r)
    {
        //Region has to be square.
        if (!r.IsSquare) return Suitability.Not;

        //Should have a large-enough area.
        int area = r.Area;
        if (area <= MinArea) return Suitability.Not;

        return Suitability.Very;
    }

    public FilledRegion Apply(FillData data)
    {
        //Get the space between each shell. In most cases it should be 1.
        //In the case of a very large room, it could be 2.
        //Played around with a graphing calculator to get a good function that reflects this.
        int minSpace = 1, maxSpace = 2;
        double coefficient = Math.Sqrt(maxSpace - minSpace) / (data.BeingFilled.Area - MinArea);
        int spaceBetweenShells = (int)Math.Round(Math.Pow(coefficient * (data.BeingFilled.Area - MinArea), 2.0) + 1.0, 0);

        //Make sure my function is valid.
        if (spaceBetweenShells < 1 || spaceBetweenShells > 2)
            throw new InvalidOperationException("Oops!");

        //Continuously fill in smaller and smaller shells centered around the region center.
        Region shell = new Region(data.BeingFilled.TopLeft.Right.Below, data.BeingFilled.BottomRight.Left.Above, true);
        sbyte holeDir = 1;
        //Smallest-allowable shell is 3x3, which means a region of width/height 2.
        while (shell.Area > 4)
        {
            //Fill in the perimeter.
            data.FillPerimeter(true, shell);
            
            //Clear the hole.
            if (holeDir < 0)
                data.SetMapAt(shell.LeftMid, false);
            else data.SetMapAt(shell.RightMid, false);

            //Flip the side the next hole will be on.
            holeDir *= -1;

            //Shrink the shell.
            for (int i = 0; i < spaceBetweenShells; ++i)
                shell = new Region(shell.TopLeft.Right.Below, shell.BottomRight.Left.Above, false);
        }

        return new ConcentricSquaresRegion(data.BeingFilled, new Region(shell.TopLeft.Left.Above, shell.BottomRight.Right.Below));
    }
}
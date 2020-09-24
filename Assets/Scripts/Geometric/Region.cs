using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
    /// A discrete rectangular area.
    /// </summary>
public struct Region
{
    public static bool SameTopBottom(Region one, Region two)
    {
        return one.Top == two.Top && one.Bottom == two.Bottom;
    }
    public static bool SameSides(Region one, Region two)
    {
        return one.Left == two.Left && one.Right == two.Right;
    }

    /// <summary>
    /// Combines all regions in the given collection of regions into a more compact collection covering the same space.
    /// </summary>
    public static List<Region> Combine(List<Region> rs)
    {
        //Turn rs into a copy.
        rs = rs.ToList();

        //Combine all adjacent bounds.
        for (int i = 0; i < rs.Count; ++i)
            for (int j = i + 1; j < rs.Count; ++j)
                /* The bounds are adjacent if either:
                 * 1) They share the same top/bottom and the left side
                 *      of one equals the right side of the other
                 * 2) They share the same left/right and the top side of
                 *      one equals the bottom side of the other */
                if ((SameTopBottom(rs[i], rs[j]) && (rs[i].Left == rs[j].Right || rs[i].Right == rs[j].Left)) ||
                    (SameSides(rs[i], rs[j]) && (rs[i].Top == rs[j].Bottom || rs[i].Bottom == rs[j].Top)))
                {
                    rs.Add(new Region(new Location(System.Math.Min(rs[i].Left, rs[j].Left), System.Math.Min(rs[i].Top, rs[j].Top)),
                                      new Location(System.Math.Max(rs[i].Right, rs[j].Right), System.Math.Max(rs[i].Bottom, rs[j].Bottom))));

                    rs.RemoveAt(j);
                    rs.RemoveAt(i);

                    --i;
                    break;
                }

        return rs;
    }

    public int X, Y, Width, Height;

    public int Top { get { return Y; } }
    public int Bottom { get { return Y + Height; } }

    public int Left { get { return X; } }
    public int Right { get { return X + Width; } }

    /// <summary>
    /// Is the width 0?
    /// </summary>
    public bool Thin { get { return Width == 0; } }
    /// <summary>
    /// Is the height 0?
    /// </summary>
    public bool Flat { get { return Height == 0; } }

    public bool IsSquare { get { return Width == Height; } }

    public Location Dimensions { get { return new Location { X = Width, Y = Height }; } }

    public Location TopLeft { get { return new Location { X = X, Y = Y }; } }
    public Location BottomLeft { get { return new Location { X = X, Y = Bottom }; } }
    public Location TopRight { get { return new Location { X = Right, Y = Y }; } }
    public Location BottomRight { get { return new Location { X = Right, Y = Bottom }; } }

    public Location TopMid { get { return new Location { X = CenterX, Y = Top }; } }
    public Location BottomMid { get { return new Location { X = CenterX, Y = Bottom }; } }
    public Location LeftMid { get { return new Location { X = Left, Y = CenterY }; } }
    public Location RightMid { get { return new Location { X = Right, Y = CenterY }; } }

    public Location Center { get { return new Location { X = (Left + Right) / 2, Y = (Top + Bottom) / 2 }; } }

    public Region LeftEdge { get { return new Region(TopLeft, BottomLeft); } }
    public Region RightEdge { get { return new Region(TopRight, BottomRight); } }
    public Region TopEdge { get { return new Region(TopLeft, TopRight); } }
    public Region BottomEdge { get { return new Region(BottomLeft, BottomRight); } }

    public int CenterX { get { return (Left + Right) / 2; } }
    public int CenterY { get { return (Top + Bottom) / 2; } }

    public int Area { get { return Width * Height; } }

    public Region(int x, int y, int width, int height)
        : this(x, y, width, height, false) { }
    public Region(int x, int y, int width, int height, bool sanityCheck)
    {
        if (sanityCheck)
        {
            if (width < 0)
            {
                x += width;
                width *= -1;
            }
            if (height < 0)
            {
                y += height;
                height *= -1;
            }
        }

        X = x; Y = y; Width = width; Height = height;
    }
    public Region(Location topLeft, Location bottomRight)
        : this(topLeft, bottomRight, false) { }
    public Region(Location topLeft, Location bottomRight, bool sanityCheck)
        : this(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y, sanityCheck) { }

    public bool Inside(Location l)
    {
        return l.X > Left && l.X < Right && l.Y > Top && l.Y < Bottom;
    }
    public bool OnEdge(Location l)
    {
        return (InsideHeight(l.Y) && OnSides(l.X)) ||
               (InsideWidth(l.X) && OnTopBottom(l.Y));
    }
    public bool OnCorner(Location l)
    {
        return OnSides(l.X) && OnTopBottom(l.Y);
    }
    public bool Touches(Location l, bool includeInside, bool includeEdge, bool includeCorner)
    {
        return (includeInside && Inside(l)) ||
               (includeEdge && OnEdge(l)) ||
               (includeCorner && OnCorner(l));
    }

    public bool InsideWidth(int x)
    {
        return x > Left && x < Right;
    }
    public bool InsideHeight(int y)
    {
        return y > Top && y < Bottom;
    }
    public bool TouchingWidth(int x)
    {
        return x >= Left && x <= Right;
    }
    public bool TouchingHeight(int y)
    {
        return y >= Top && y <= Bottom;
    }

    public bool OnSides(int x)
    {
        return x == Left || x == Right;
    }
    public bool OnTopBottom(int y)
    {
        return y == Top || y == Bottom;
    }

    /// <summary>
    /// Returns true if this region touches some or all of the inside of the given region.
    /// </summary>
    public bool TouchesInside(Region other)
    {
        //If the regions touch, then either:
        // 1) One region is fully inside another, or
        // 2) For at least one pair of opposite edges on one of the rectangles, the first edge will be on the opposite side of the other rectangle from the other edge.
        return SubRegionOf(other) || other.SubRegionOf(this) ||
               ((other.InsideHeight(Top) || other.InsideHeight(Bottom)) &&
                  (System.Math.Sign(Left - other.Right) == -System.Math.Sign(Right - other.Right) || System.Math.Sign(Left - other.Left) == -System.Math.Sign(Right - other.Left))) ||
               ((InsideHeight(other.Top) || InsideHeight(other.Bottom)) &&
                  (System.Math.Sign(other.Left - Right) == -System.Math.Sign(other.Right - Right) || System.Math.Sign(other.Left - Left) == -System.Math.Sign(other.Right - Left))) ||
               ((other.InsideWidth(Left) || other.InsideWidth(Right)) &&
                  (System.Math.Sign(Top - other.Top) == -System.Math.Sign(Bottom - other.Top) || System.Math.Sign(Top - other.Bottom) == -System.Math.Sign(Bottom - other.Bottom))) ||
               ((InsideWidth(other.Left) || InsideWidth(other.Right)) &&
                  (System.Math.Sign(other.Top - Top) == -System.Math.Sign(other.Bottom - Top) || System.Math.Sign(other.Top - Bottom) == -System.Math.Sign(other.Bottom - Bottom)));
    }
    /// <summary>
    /// Gets if this Region is completely covered by the given Region.
    /// </summary>
    public bool SubRegionOf(Region r)
    {
        return (r.Inside(TopLeft) || r.OnEdge(TopLeft) || r.OnCorner(TopLeft)) &&
               (r.Inside(TopRight) || r.OnEdge(TopRight) || r.OnCorner(TopRight)) &&
               (r.Inside(BottomLeft) || r.OnEdge(BottomLeft) || r.OnCorner(BottomLeft)) &&
               (r.Inside(BottomRight) || r.OnEdge(BottomRight) || r.OnCorner(BottomRight));
    }
    /// <summary>
    /// Returns true if this region touches the other region on the edges.
    /// </summary>
    /// <param name="includeSolelyCorners">What to return if the two Regions only touch at a corner.</param>
    public bool TouchesEdgeOnly(Region other, bool includeSolelyCorners)
    {
        return (includeSolelyCorners || (!TouchesCornerOnly(other) && !other.TouchesCornerOnly(this))) &&
               (((TouchingHeight(other.Top) || TouchingHeight(other.Bottom) || other.TouchingHeight(Top) || other.TouchingHeight(Bottom)) && (other.Right == Left || other.Left == Right)) ||
                ((TouchingWidth(other.Left) || TouchingWidth(other.Right) || other.TouchingWidth(Left) || other.TouchingWidth(Right)) && (other.Top == Bottom || other.Bottom == Top)));
    }
    /// <summary>
    /// Finds if this Region is touching the given one only by a single corner.
    /// </summary>
    public bool TouchesCornerOnly(Region other)
    {
        return TopLeft == other.BottomRight ||
               TopRight == other.BottomLeft ||
               BottomLeft == other.TopRight ||
               BottomRight == other.TopLeft;
    }

    /// <summary>
    /// Subtracts the given region from this one and returns the regions resulting from the subtraction.
    /// </summary>
    /// <param name="edgeBorder">The space between the new regions and "sub".</param>
    /// <returns>A list holding either this region (if "sub" doesn't touch this)
    /// or the regions making up what is left over after "sub" is subtracted from this.</returns>
    public List<Region> Subtract(Region sub, int edgeBorder)
    {
        sub = sub.InflateEachSide(new Location(edgeBorder, edgeBorder));

        //Start with this region.
        List<Region> newRegs = new List<Region>() { this };
        List<Region> splitRegs = new List<Region>();

        //For each edge in "sub", split the regions up along that edge.

        //Left edge.

        splitRegs.Clear();
        //Go through each region and split.
        foreach (Region r in newRegs)
            splitRegs.AddRange(r.SplitAlongVerticalLine(sub.Left));

        //Store the new split regions.
        newRegs.Clear();
        foreach (Region r in splitRegs)
            newRegs.Add(r);

        //Right edge.

        splitRegs.Clear();
        //Go through each region and split.
        foreach (Region r in newRegs)
            splitRegs.AddRange(r.SplitAlongVerticalLine(sub.Right));

        //Store the new split regions.
        newRegs.Clear();
        foreach (Region r in splitRegs)
            newRegs.Add(r);

        //Top edge.

        splitRegs.Clear();
        //Go through each region and split.
        foreach (Region r in newRegs)
            splitRegs.AddRange(r.SplitAlongHorizontalLine(sub.Top));

        //Store the new split regions.
        newRegs.Clear();
        foreach (Region r in splitRegs)
            newRegs.Add(r);

        //Bottom edge.

        splitRegs.Clear();
        //Go through each region and split.
        foreach (Region r in newRegs)
            splitRegs.AddRange(r.SplitAlongHorizontalLine(sub.Bottom));

        //Store the new split regions.
        newRegs.Clear();
        foreach (Region r in splitRegs)
            newRegs.Add(r);

        //Remove all regions inside the subtracted region.
        //Also remove all regions with area smaller than 0 (due to the edge border).
        for (int i = 0; i < newRegs.Count; ++i)
            if ((newRegs[i].TouchesInside(sub) && !newRegs[i].TouchesEdgeOnly(sub, true)) ||
                newRegs[i].Width < 0 ||
                newRegs[i].Height < 0)
                newRegs.RemoveAt(i--);

        //Now we're done.
        return newRegs;
    }

    /// <summary>
    /// Splits this Region along the infinite horizontal line at the given y coordinate.
    /// </summary>
    /// <returns>The resulting regions after the split is done (either 1 or 2 elements
    /// depending on whether the line intersects this Region).</returns>
    public List<Region> SplitAlongHorizontalLine(int y)
    {
        if (InsideHeight(y))
            return new List<Region>()
                {
                    new Region(TopLeft, new Location(Right, y)),
                    new Region(new Location(Left, y), BottomRight),
                };
        else return new List<Region>() { this };
    }
    /// <summary>
    /// Splits this Region along the infinite vertical line at the given x coordinate.
    /// </summary>
    /// <returns>The resulting regions after the split is done (either 1 or 2 elements
    /// depending on whether the line intersects this Region).</returns>
    public List<Region> SplitAlongVerticalLine(int x)
    {
        if (InsideWidth(x))
            return new List<Region>()
                {
                    new Region(TopLeft, new Location(x, Bottom)),
                    new Region(new Location(x, Top), BottomRight),
                };
        else return new List<Region>() { this };
    }

    public Region Translate(Location delta)
    {
        return new Region(X + delta.X, Y + delta.Y, Width, Height);
    }
    public Region InflateEachSide(Location amount)
    {
        return new Region(X - amount.X, Y - amount.Y,
                          Width + amount.X + amount.X, Height + amount.Y + amount.Y);
    }

    public override int GetHashCode()
    {
        return Center.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        Region? r = obj as Region?;
        return r != null && this == r.Value;
    }
    public override string ToString()
    {
        return "Region: " + Left + ", " + Top + " to " + Right + ", " + Bottom;
    }
    public static bool operator ==(Region one, Region two)
    {
        return one.X == two.X && one.Y == two.Y && one.Width == two.Width && one.Height == two.Height;
    }
    public static bool operator !=(Region one, Region two)
    {
        return one.X != two.X || one.Y != two.Y || one.Width != two.Width || one.Height != two.Height;
    }
}
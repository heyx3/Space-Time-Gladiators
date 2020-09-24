using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
    /// Encapsulated behavior that can split a region up by adding in walls.
    /// </summary>
public interface FillPattern
{
    Suitability MaxSuitability { get; set; }
    Suitability GetSuitability(Region r);
    FilledRegion Apply(FillData data);
}

/// <summary>
    /// How suitable a Fill Pattern can be to a given region.
    /// </summary>
public class Suitability
{
    public static Suitability Not = new Suitability(0);
    public static Suitability Little = new Suitability(1);
    public static Suitability Moderate = new Suitability(2);
    public static Suitability Very = new Suitability(3);

    public static Suitability Max(Suitability one, Suitability two)
    {
        if (one.GreaterThan(two)) return one;
        else return two;
    }
    public static Suitability Min(Suitability one, Suitability two)
    {
        if (one.GreaterThan(two)) return two;
        else return one;
    }

    public bool GreaterThan(Suitability other) { return value > other.value; }
    public bool LessThan(Suitability other) { return value < other.value; }
    public bool EqualTo(Suitability other) { return value == other.value; }

    /// <summary>
    /// Gets the Suitability object one step up.
    /// </summary>
    public Suitability More()
    {
        if (this == Not) return Little;
        if (this == Little) return Moderate;
        return Very;
    }
    /// <summary>
    /// Gets the suitability object one step down.
    /// </summary>
    public Suitability Less()
    {
        if (this == Very) return Moderate;
        if (this == Moderate) return Little;
        return Not;
    }

    /// <summary>
    /// Linearly interpolates between Not, Little, Moderate, and Very for the given value.
    /// </summary>
    /// <param name="leastSuitable">The largest possible Not Suitable value.</param>
    /// <param name="mostSuitable">The smallest possible Very Suitable value.</param>
    /// <param name="value">The value to convert to a suitability.</param>
    /// <returns>"value" converted to a Suitability, using the given least/most suitable values for reference.</returns>
    public static Suitability Interpolate(float leastSuitable, float mostSuitable, float value)
    {
        float range = mostSuitable - leastSuitable;
        float third = range / 3.0f;

        if (leastSuitable > value) return Suitability.Not;
        if (leastSuitable + third > value) return Suitability.Little;
        if (leastSuitable + third + third > value) return Suitability.Moderate;

        return Suitability.Very;
    }
    /// <summary>
    /// Linearly interpolates between Not, Little, Moderate, and Very for the given value.
    /// </summary>
    /// <param name="suitabilityInterval">The interval of suitable values,
    /// where the start is the largest possible Not Suitable value and the end is the smallest possible Very Suitable value.</param>
    /// <param name="value">The value to convert to a suitability.</param>
    /// <returns>"value" converted to a Suitability, using the given least/most suitable values for reference.</returns>
    public static Suitability Interpolate(Interval suitabilityInterval, float value)
    {
        return Interpolate(suitabilityInterval.Start, suitabilityInterval.End - suitabilityInterval.RoundedEpsilon, value);
    }
    /// <summary>
    /// Linearly interpolates between Not, Little, Moderate, and Very for the given value.
    /// </summary>
    /// <param name="suitabilityInterval">The interval of suitable values,
    /// where start is the largest possible Not Suitable value below center,
    /// center is the target value,
    /// and the end is the smallest possible Not Suitable value above center.</param>
    /// <param name="value">The value to convert to a suitability.</param>
    /// <param name="verySuitableRange">The size of the "Very Suitable" range at the center of the interval.</param>
    /// <returns>"value" converted to a Suitability, using the given least/most suitable values for reference.</returns>
    public static Suitability InterpolateTowardsCenter(Interval suitabilityInterval, float value, float verySuitableRange)
    {
        if (value > suitabilityInterval.Center)
            value = suitabilityInterval.ReflectAroundCenter(value);

        return Interpolate(suitabilityInterval.Start, suitabilityInterval.Center - (verySuitableRange * 0.5f), value);
    }

    private byte value;
    private Suitability(byte val) { value = val; }

    public override string ToString()
    {
        if (this == Not) return "Not";
        if (this == Little) return "Little";
        if (this == Moderate) return "Moderate";
        return "Very";
    }
}

/// <summary>
    /// A collection of level data for passing to a FillPattern. Also encapsulates the ability to fill in a specific area of the map with a given value.
    /// </summary>
public class FillData
{
    public Interval SizeX, SizeY;

    public bool WrapX, WrapY;

    /// <summary>
    /// If trying to access a value outside the level,
    /// and the level doesn't wrap around in that direction,
    /// this is the value that will be returned.
    /// </summary>
    public bool OutsideLevelValue;

    public Location WorldSize;

    public bool[,] Map;
    public Region BeingFilled;

    public List<Region> CurrentRegions;
    public List<Location> Holes;

    public FillData(bool[,] map, Region beingFilled, List<Region> currentRegions, List<Location> holes,
                    bool wrapX, bool wrapY, bool outsideLevelValue)
    {
        Map = map;
        BeingFilled = beingFilled;
        CurrentRegions = currentRegions;
        Holes = holes;

        WrapX = wrapX;
        WrapY = wrapY;
        
        SizeX = new Interval(0, Map.GetLength(0) - 1, true, 0);
        SizeY = new Interval(0, Map.GetLength(1) - 1, true, 0);

        WorldSize = new Location(Map.GetLength(0), Map.GetLength(1));

        OutsideLevelValue = outsideLevelValue;
    }

    /// <summary>
    /// Gets all holes along the OUTSIDE perimeter of the region being filled
    /// (i.e. touching the edge of a region whose sides are pushed out one unit from the given).
    /// </summary>
    public IEnumerable<Location> HolesAlongPerimeter()
    {
        return HolesAlongPerimeter(null);
    }
    /// <summary>
    /// Gets all holes along the OUTSIDE perimeter of a region (i.e. touching the edge of a region whose sides are pushed out one unit from the given).
    /// </summary>
    /// <param name="r">The region to use as the perimeter, or "null" if "BeingFilled" should be used.</param>
    /// <returns>All holes touching the edge of a region equivalent to the given region with all sides pushed out one unit.</returns>
    public IEnumerable<Location> HolesAlongPerimeter(Region? r)
    {
        if (!r.HasValue) r = BeingFilled;
        Region reg = r.Value;
        reg.X -= 1; reg.Y -= 1;
        reg.Width += 2; reg.Height += 2;

        for (int i = 0; i < Holes.Count; ++i)
            if (reg.OnEdge(Holes[i]))
                yield return Holes[i];
    }

    /// <summary>
    /// If "x" is inside the level, or the level doesn't wrap in the X direction, return "x".
    /// Otherwise, return "x" wrapped to be inside the level.
    /// </summary>
    public int GetXValue(int x)
    {
        if (!WrapX) return x;

        return (int)SizeX.Wrap(x);
    }
    /// <summary>
    /// If "y" is inside the level, or the level doesn't wrap in the Y direction, return "y".
    /// Otherwise, return "y" wrapped to be inside the level.
    /// </summary>
    public int GetYValue(int y)
    {
        if (!WrapY) return y;

        return (int)SizeY.Wrap(y);
    }

    /// <summary>
    /// Gets the value of the map at the given location.
    /// </summary>
    public bool GetMapAt(Location l)
    {
        int x = GetXValue(l.X),
            y = GetYValue(l.Y);

        //If the position is outside the level map, return the default value.
        if (!SizeX.Inside(x) || !SizeY.Inside(y))
            return OutsideLevelValue;

        //Otherwise, return the normal value.
        return Map[x, y];
    }
    /// <summary>
    /// Sets the value of the map at the given location.
    /// If the value is outside the map, this function does nothing.
    /// </summary>
    public void SetMapAt(Location l, bool value)
    {
        int x = GetXValue(l.X),
            y = GetYValue(l.Y);

        //If the position is outside the level map, don't do anything.
        if (!SizeX.Inside(x) || !SizeY.Inside(y))
            return;

        //Otherwise, set the normal value.
        Map[x, y] = value;
    }

    /// <summary>
    /// Fills the Map with the given value for the given area.
    /// </summary>
    public void FillRegion(bool value, Region r)
    {
        for (int i = r.Left; i <= r.Right; ++i)
            for (int j = r.Top; j <= r.Bottom; ++j)
                SetMapAt(new Location(i, j), value);
    }
    /// <summary>
    /// Fills the Map with the given value around the given Region's perimeter.
    /// </summary>
    public void FillPerimeter(bool value, Region r)
    {
        for (int i = r.Left; i <= r.Right; ++i)
        {
            SetMapAt(new Location(i, r.Top), value);
            SetMapAt(new Location(i, r.Bottom), value);
        }
        for (int j = r.Top; j <= r.Bottom; ++j)
        {
            SetMapAt(new Location(r.Left, j), value);
            SetMapAt(new Location(r.Right, j), value);
        }
    }

    /// <summary>
    /// Fills a line with the given value.
    /// </summary>
    public void FillLine(bool val, Location start, Location end)
    {
        FillLine(val, start, end, true);
    }
    /// <summary>
    /// Fills a line with the given value.
    /// </summary>
    public void FillLine(bool val, Location start, Location end, bool horizontalBias)
    {
        //Check special cases.
        if (start.X == end.X || start.Y == end.Y)
        {
            FillRegion(val, new Region(start, end, true));
            return;
        }

        //Get data. Use a counter to go cell-by-cell from start to end.
        Location counter = start;
        Location initialDist = new Region(start, end, true).Dimensions;
        bool moveHorizontally;

        //Start off the algorithm.
        SetMapAt(counter, val);
        //Make a path to the end.
        while (counter != end)
        {
            //If the relative x dist to "end" is larger than relative "y", move horizontally.
            //If the relative x dist to "end" is smaller than relative "y", move vertically.
            //If they are equal, follow the bias.
            Location amount = new Location(end.X - counter.X, end.Y - counter.Y);

            float relativeX = Math.Abs(amount.X) / (float)initialDist.X;
            float relativeY = Math.Abs(amount.Y) / (float)initialDist.Y;

            if (relativeX > relativeY)
                moveHorizontally = true;
            else if (relativeX < relativeY)
                moveHorizontally = false;
            else moveHorizontally = horizontalBias;

            //Now move.
            amount = new Location(Math.Sign(amount.X), Math.Sign(amount.Y));
            if (moveHorizontally)
                counter.X += amount.X;
            else counter.Y += amount.Y;

            //Set the current position to the value.
            SetMapAt(counter, val);
        }
    }

    /// <summary>
    /// Fills a circle with the given value.
    /// </summary>
    public void FillCircle(bool value, Location center, float radius)
    {
        //Go through every cell inside the circle's bounding box looking for cells that are close enough.

        float radSqr = radius * radius;
        Region area = new Region(new Location((int)Math.Floor(center.X - radius), (int)Math.Floor(center.Y - radius)),
                                 new Location((int)Math.Ceiling(center.X + radius), (int)Math.Ceiling(center.Y + radius)));

        Location l;
        for (int i = area.Left; i <= area.Right; ++i)
            for (int j = area.Top; j <= area.Bottom; ++j)
            {
                l = new Location(i, j);

                //If this location is actually inside the circle, fill it.
                if (Location.DistanceSquared(l, center) <= radSqr)
                    SetMapAt(l, value);
            }
    }
}
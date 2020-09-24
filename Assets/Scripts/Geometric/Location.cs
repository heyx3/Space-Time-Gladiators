using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
    /// Represents a two-dimensional coordinate.
    /// </summary>
public struct Location
{
    public static Location Zero { get { return new Location(0, 0); } }

    public int X;
    public int Y;

    public Location Above { get { return new Location(X, Y - 1); } }
    public Location Below { get { return new Location(X, Y + 1); } }
    public Location Left { get { return new Location(X - 1, Y); } }
    public Location Right { get { return new Location(X + 1, Y); } }

    public Location(int x, int y) { X = x; Y = y; }
    public Location(int val) { X = val; Y = val; }

    public override int GetHashCode()
    {
        return (X * 73856093) ^ (Y * 19349663);
    }
    public override bool Equals(object obj)
    {
        Location? l = obj as Location?;
        return l.HasValue && l.Value.X == X && l.Value.Y == Y;
    }
    public override string ToString()
    {
        return "{" + X + ", " + Y + "}";
    }

    public static float Distance(Location one, Location two)
    {
        return (float)Math.Sqrt(((one.X - two.X) * (one.X - two.X)) + ((one.Y - two.Y) * (one.Y - two.Y)));
    }
    public static float DistanceSquared(Location one, Location two)
    {
        return (float)(((one.X - two.X) * (one.X - two.X)) + ((one.Y - two.Y) * (one.Y - two.Y)));
    }

    public static Location Average(params Location[] ls)
    {
        Location ret = Location.Zero;
        if (ls.Length == 0) return ret;

        for (int i = 0; i < ls.Length; ++i)
            ret += ls[i];

        ret /= ls.Length;

        return ret;
    }
    public static Location Average(List<Location> ls)
    {
        Location ret = Location.Zero;
        if (ls.Count == 0) return ret;

        for (int i = 0; i < ls.Count; ++i)
            ret += ls[i];

        ret /= ls.Count;

        return ret;
    }
    
    public static Location operator +(Location one, Location two)
    {
        return new Location(one.X + two.X, one.Y + two.Y);
    }
    public static Location operator -(Location one, Location two)
    {
        return new Location(one.X - two.X, one.Y - two.Y);
    }
    public static Location operator *(Location one, Location two)
    {
        return new Location(one.X * two.X, one.Y * two.Y);
    }
    public static Location operator /(Location one, Location two)
    {
        return new Location(one.X / two.X, one.Y / two.Y);
    }

    public static Location operator +(Location l, int val)
    {
        return new Location(l.X + val, l.Y + val);
    }
    public static Location operator -(Location l, int val)
    {
        return new Location(l.X - val, l.Y - val);
    }
    public static Location operator *(Location l, int val)
    {
        return new Location(l.X * val, l.Y * val);
    }
    public static Location operator /(Location l, int val)
    {
        return new Location(l.X / val, l.Y / val);
    }

    public static bool operator ==(Location one, Location two)
    {
        return one.X == two.X && one.Y == two.Y;
    }
    public static bool operator !=(Location one, Location two)
    {
        return one.X != two.X || one.Y != two.Y;
    }
}
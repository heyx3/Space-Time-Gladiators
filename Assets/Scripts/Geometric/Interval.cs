using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
    /// Represents a range that can be inclusive or exclusive to its bounds.
    /// </summary>
public class Interval
{
    public static Interval RotationBoundaries = new Interval(-(float)Math.PI, (float)Math.PI, true, 4);
    public static Interval ZeroToOneInterval = new Interval(0.0f, 1.0f, true, 6);
    public static Interval ByteInterval = new Interval(0.0f, 255.0f, true, 4);

    /// <summary>
    /// The inclusive left endpoint of the interval, rounded to the given number of decimal places.
    /// </summary>
    public float Start
    {
        get { return Center - (Range * 0.5f); }
        set
        {
            float diff = value - Start;
            Range -= diff;
            Center += diff;
        }
    }
    /// <summary>
    /// The exclusive right endpoint of the interval, rounded to the given number of decimal places.
    /// </summary>
    public float End
    {
        get { return Center + (Range * 0.5f) + RoundedEpsilon; }
        set
        {
            float diff = value - End;
            Range += diff;
            Center += diff;
        }
    }

    /// <summary>
    /// The range that this Interval covers, rounded to the given number of decimal places.
    /// </summary>
    public float Range
    {
        get { return range; }
        set
        {
            range = (float)System.Math.Round(value, DecimalPlaceAccuracy);
        }
    }
    protected float range;

    /// <summary>
    /// The midpoint of the interval.
    /// </summary>
    public virtual float Center
    {
        get { return center; }
        set
        {
            center = (float)System.Math.Round(value, DecimalPlaceAccuracy);
        }
    }
    protected float center;

    /// <summary>
    /// The number of decimal places all numbers this Interval uses will be rounded to.
    /// </summary>
    public int DecimalPlaceAccuracy
    {
        get { return decimalPlaceAccuracy; }
        set
        {
            decimalPlaceAccuracy = value;
            RoundedEpsilon = 1.0f / (float)System.Math.Pow(10, DecimalPlaceAccuracy);
        }
    }
    int decimalPlaceAccuracy;

    /// <summary>
    /// Gets the value halfway between the start and the center.
    /// </summary>
    public float FirstQuarter
    {
        get { return Center - (Range * 0.25f); }
    }
    /// <summary>
    /// Gets the value between the center and the end.
    /// </summary>
    public float ThirdQuarter
    {
        get { return Center + (Range * 0.25f); }
    }

    public Interval FirstQuarterInterval
    {
        get { return new Interval(Start, FirstQuarter, true, DecimalPlaceAccuracy); }
    }
    public Interval SecondQuarterInterval
    {
        get { return new Interval(FirstQuarter, Center, true, DecimalPlaceAccuracy); }
    }
    public Interval ThirdQuarterInterval
    {
        get { return new Interval(Center, ThirdQuarter, true, DecimalPlaceAccuracy); }
    }
    public Interval FourthQuarterInterval
    {
        get { return new Interval(ThirdQuarter, End - RoundedEpsilon, true, DecimalPlaceAccuracy); }
    }

    public Interval FirstHalf
    {
        get { return new Interval(Start, Center, true, DecimalPlaceAccuracy); }
    }
    public Interval SecondHalf
    {
        get { return new Interval(Center, End - RoundedEpsilon, true, DecimalPlaceAccuracy); }
    }

    /// <summary>
    /// The smallest possible number this Interval can deal with, given its decimal place accuracy.
    /// </summary>
    public float RoundedEpsilon
    {
        get;
        private set;
    }

    /// <summary>
    /// Returns the Interval with the smallest starting value.
    /// </summary>
    /// <returns>The Interval with the smallest starting value, or the first given Interval if they are equal.</returns>
    public static Interval SmallestStart(Interval one, Interval two)
    {
        if (one.Start <= two.Start)
            return one;
        return two;
    }
    /// <summary>
    /// Returns the Interval with the smallest starting value.
    /// </summary>
    /// <returns>The Interval with the smallest starting value, or the first given Interval if they are equal.</returns>
    public static Interval SmallestEnd(Interval one, Interval two)
    {
        if (one.End <= two.End)
            return one;
        return two;
    }
    /// <summary>
    /// Returns the Interval with the smallest starting value.
    /// </summary>
    /// <returns>The Interval with the smallest starting value, or the first given Interval if they are equal.</returns>
    public static Interval SmallestRange(Interval one, Interval two)
    {
        if (one.Range <= two.Range)
            return one;
        return two;
    }

    /// <summary>
    /// Creates a new interval with the given arguments.
    /// If 'end' is greater than 'start', they will be switched.
    /// </summary>
    /// <param name="startInclude">Whether or not to include the start of the interval as an actual member of it.</param>
    /// <param name="endInclude">Whether or not to include the end of the interval as an actual member of it.</param>
    /// <param name="switchEnds">If 'start' is greater than 'end', the two will be switched.
    /// If that happens, this variable indicates whether or not 'startInclude' and 'endInclude' should be switched as well.</param>
    public Interval(float start, float end, bool startInclude, bool endInclude, bool switchEnds, int decimalPlaceAccuracy)
    {
        if (start > end)
        {
            float temp = start;
            start = end;
            end = temp;

            if (switchEnds)
            {
                bool temp2 = startInclude;
                startInclude = endInclude;
                endInclude = temp2;
            }
        }

        DecimalPlaceAccuracy = decimalPlaceAccuracy;
        if (!startInclude) start += RoundedEpsilon;
        if (!endInclude) end -= RoundedEpsilon;

        range = end - start;
        center = start + (Range / 2);
    }
    public Interval(float start, float end, bool includeEnds, int decimalPlaces)
        : this(start, end, includeEnds, includeEnds, false, decimalPlaces) { }
    /// <summary>
    /// Creates a new Interval with the given range and center.
    /// </summary>
    public Interval(float range, float center, int decimalPlaces)
    {
        this.range = range;
        this.center = center;
        DecimalPlaceAccuracy = decimalPlaces;
    }
    /// <summary>
    /// Creates a new Interval given the Interval's start and range.
    /// </summary>
    public Interval(int decimalPlaces, float range, float start, bool startInclusive)
    {
        DecimalPlaceAccuracy = decimalPlaces;
        if (!startInclusive) start += RoundedEpsilon;

        this.range = range;
        center = start + (range * 0.5f);
    }
    public Interval(Interval copy)
        : this(copy.range, copy.center, copy.decimalPlaceAccuracy) { }

    /// <summary>
    /// Figures out if a given number is inside the interval.
    /// </summary>
    /// <returns>Whether or not 'number' is inside the interval.</returns>
    public bool Inside(float number)
    {
        number = (float)System.Math.Round(number, DecimalPlaceAccuracy);
        return (number < End && number >= Start);
    }

    /// <summary>
    /// Clamps the given number to be inside this interval.
    /// </summary>
    public float Clamp(float number)
    {
		if (number < Start) return Start;
		if (number > End - RoundedEpsilon) return End - RoundedEpsilon;
		return number;
    }
    /// <summary>
    /// Wraps the given number to be inside this range (similar to a modulo operation).
    /// </summary>
    public float Wrap(float number)
    {
        //To do this, take the number and subtract the starting value from it.
        //Then take that result mod the range of this interval, then add back in the starting value.
        number -= Start;

        //Negative numbers don't work correctly with the mod operator.
        if (number < 0)
        {
            int amount = (int)System.Math.Ceiling(-number / Range);
            number += amount * (Range);
        }
        number %= (Range + RoundedEpsilon);

        number += Start;
        return (float)System.Math.Round(number, DecimalPlaceAccuracy);
    }
    /// <summary>
    /// Maps a given value from this Interval to the given Interval
    /// (e.x. if the given value is the midpoint of this Interval, the midpoint of "destination" will be returned).
    /// </summary>
    /// <returns>Finds 'value's position relative to this Interval's bounds, and returns the equivalent value relative to 'destination'.</returns>
    public float Map(Interval destination, float value)
    {
        //Get the difference.
        float valueToEnd = value - (End - RoundedEpsilon);
        //Map it.
        valueToEnd *= destination.Range / Range;

        return (float)System.Math.Round((destination.End - destination.RoundedEpsilon) + valueToEnd, DecimalPlaceAccuracy);
    }

    /// <summary>
    /// Gets a random number from inside this Interval.
    /// </summary>
    public float Random() { return (float)System.Math.Round(Start + (GeneratorSettings.R.NextDouble() * Range), DecimalPlaceAccuracy); }

    /// <summary>
    /// Finds if the given Interval is completely inside this Interval.
    /// </summary>
    /// <returns>Whether or not 'inside' is completely within the constraints of this Interval.</returns>
    public bool Contains(Interval inside)
    {
        return (inside.Start >= Start && inside.End <= End);
    }

    /// <summary>
    /// Figures out if 1) The Intervals touch, and 2) Neither of the Intervals is completely inside the other.
    /// </summary>
    /// <returns>Returns whether or not the intervals touch yet don'x completely contain the other.</returns>
    public bool Intersects(Interval other)
    {
        if (!Contains(other) && !other.Contains(this))
            if ((Start <= other.End && Start >= other.Start) ||
                (End <= other.End && End >= other.Start))
                return true;
        return false;
    }

    /// <summary>
    /// Reflects the given value using this range's center as the reflection point.
    /// </summary>
    public float ReflectAroundCenter(float value)
    {
        float diff = System.Math.Abs(Center - value);

        if (Center < value)
            return Center - diff;
        else return Center + diff;
    }

    /// <summary>
    /// Returns whether or not the given Interval touches this Interval at all.
    /// </summary>
    /// <returns>"true" if the Intervals are at least partially inside each other, "false" otherwise.</returns>
    public bool Touches(Interval other)
    {
        return (Contains(other) || other.Contains(this) || Intersects(other));
    }

    /// <summary>
    /// Pushes out the endpoints of the interval using the given scale.
    /// </summary>
    /// <returns>A new Interval whose midpoint is the same as this Interval's midpoint, and whose range is equal to this Interval's range times "scale".</returns>
    public Interval Inflate(float scale)
    {
        return new Interval(Range * scale, Center, DecimalPlaceAccuracy);
    }
    public static Interval operator +(Interval i, float amount)
    {
        return new Interval(i.Range, i.Center + amount, i.DecimalPlaceAccuracy);
    }
    public static Interval operator -(Interval i, float amount)
    {
        return new Interval(i.Range, i.Center - amount, i.DecimalPlaceAccuracy);
    }
    public static Interval operator *(Interval i, float amount)
    {
        return new Interval(i.Start * amount, (i.End - i.RoundedEpsilon) * amount, true, false, true, i.DecimalPlaceAccuracy);
    }
    public static Interval operator /(Interval i, float amount)
    {
        return new Interval(i.Start / amount, (i.End - i.RoundedEpsilon) / amount, true, false, true, i.DecimalPlaceAccuracy);
    }

    public override int GetHashCode()
    {
        return (Start.GetHashCode() + End.GetHashCode()) * DecimalPlaceAccuracy;
    }
    public override bool Equals(object obj)
    {
        Interval i = obj as Interval;

        return i != null &&
               Range == i.Range &&
               Center == i.Center &&
               DecimalPlaceAccuracy == i.DecimalPlaceAccuracy;
    }
    public override string ToString()
    {
        return "An interval with inclusive start: " + Start + " and inclusive end: " + (End - RoundedEpsilon) + " precise within " + RoundedEpsilon;
    }
}
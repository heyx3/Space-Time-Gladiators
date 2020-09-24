using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
    /// Puts an "X" shape into the region.
    /// </summary>
public class XPattern : FillPattern
{
    public const float MinArea = 5 * 5;

    public Suitability MaxSuitability { get; set; }

    public Interval AllowableXToYRatioRange
    {
        get { return new Interval(2 * halfRangeInterval.Range, halfRangeInterval.End - halfRangeInterval.RoundedEpsilon, halfRangeInterval.DecimalPlaceAccuracy); }
        set
        {
            halfRangeInterval = new Interval(value.Start, value.Center, true, value.DecimalPlaceAccuracy);
        }
    }

    public Interval halfRangeInterval;
    public XPattern(Interval allowableXToYRatioRange)
    {
        AllowableXToYRatioRange = allowableXToYRatioRange;
        MaxSuitability = Suitability.Very;
    }

    public Suitability GetSuitability(Region r)
    {
        //Make sure the region isn't totally unacceptable.
        if (r.Area < MinArea || r.Width % 2 == 1 || r.Height % 2 == 1) return Suitability.Not;

        //Use the ratio of width to height.
        float ratio = (float)r.Width / r.Height;
        if (AllowableXToYRatioRange.Inside(ratio))
            return MaxSuitability;
        else return Suitability.Not;
    }

    public FilledRegion Apply(FillData data)
    {
        Region clearArea = new Region(data.BeingFilled.Center.Left.Above, data.BeingFilled.Center.Right.Below);

        //Fill the inner ring.
        //Move from each corner of the X to the center.
        Location counter;
        Location center = data.BeingFilled.Center;
        bool moveHorizontally;
        bool horizontalBias = data.BeingFilled.Width > data.BeingFilled.Height;
        //Fill in the center.
        data.FillRegion(true, new Region(clearArea.TopLeft.Above.Left, clearArea.BottomRight.Below.Right));
        for (int i = 0; i < 4; ++i)
        {
            //Get the point to start from.
            switch (i)
            {
                case 0:
                    counter = data.BeingFilled.TopLeft.Right.Below;
                    data.SetMapAt(counter, false);
                    break;
                case 1:
                    counter = data.BeingFilled.TopRight.Left.Below;
                    data.SetMapAt(counter, false);
                    break;
                case 2:
                    counter = data.BeingFilled.BottomLeft.Right.Above;
                    data.SetMapAt(counter, false);
                    break;
                case 3:
                    counter = data.BeingFilled.BottomRight.Left.Above;
                    data.SetMapAt(counter, false);
                    break;

                default: throw new InvalidOperationException();
            }

            //Clear a path to the center.
            while (!clearArea.Touches(counter, true, true, true))
            {
                //Get which direction to move in.
                float relativeDistX = Math.Abs(counter.X - center.X) / (float)data.BeingFilled.Width;
                float relativeDistY = Math.Abs(counter.Y - center.Y) / (float)data.BeingFilled.Height;
                if (relativeDistX > relativeDistY)
                    moveHorizontally = true;
                else if (relativeDistX < relativeDistY)
                    moveHorizontally = false;
                else moveHorizontally = horizontalBias;

                //Move that direction, add the region, and fill part of the X after moving.
                if (moveHorizontally)
                {
                    int amount = Math.Sign(center.X - counter.X);

                    counter.X += amount;
                    data.SetMapAt(counter, false);

                    data.SetMapAt(new Location(counter.X + amount, counter.Y), true);
                    if (counter.Y > 1 && counter.Y < data.BeingFilled.Bottom - 1)
                        data.SetMapAt(new Location(counter.X, counter.Y + Math.Sign(counter.Y - center.Y)), true);
                }
                else
                {
                    int amount = Math.Sign(center.Y - counter.Y);

                    counter.Y += amount;
                    data.SetMapAt(counter, false);

                    data.SetMapAt(new Location(counter.X, counter.Y + amount), true);
                    if (counter.X > 1 && counter.X < data.BeingFilled.Right - 1)
                        data.SetMapAt(new Location(counter.X + Math.Sign(counter.X - center.X), counter.Y), true);
                }
            }

            //Keep the center clear.
            data.FillRegion(false, clearArea);
        }

        //Just to be safe, clear the perimeter at the end.
        data.FillPerimeter(false, data.BeingFilled);

        return new XRegion(data.BeingFilled, data.Map);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents a horizontal or vertical line.
/// </summary>
public class Line
{
    /// <summary>
    /// The different ways a line can point.
    /// </summary>
    public enum Orientation
    {
        Horizontal,
        Vertical,
    }
    /// <summary>
    /// The direction this line points in.
    /// </summary>
    public Orientation Dir;

    /// <summary>
    /// If this line is horizontal, this represents the line's range of x values.
    /// If vertical, this represents the line's range of y values.
    /// </summary>
    public Interval LineRange;

    /// <summary>
    /// If horizontal, this is this line's y value.
    /// If vertical, this is this line's x value.
    /// </summary>
    public float ConstValue;

    /// <summary>
    /// Creates a new Line.
    /// </summary>
    /// <param name="direction">Is this Line horizontal or vertical?</param>
    /// <param name="range">The range of x or y values for this hor. or vert. line, respectively.</param>
    /// <param name="coordinate">The y or x value for this hor. or vert. line, respectively</param>
    public Line(Orientation direction, Interval range, float coordinate)
    {
        Dir = direction;
        LineRange = range;
        ConstValue = coordinate;
    }

    /// <summary>
    /// If this line is horizontal, finds whether the given range of x values intersects this line's x values.
    /// If this line is vertical, finds whether the given range of y values intersects this line's y values.
    /// </summary>
    public bool Touching(Interval range)
    {
        return LineRange.Touches(range);
    }
	
	public Vector2 Endpoint1 {
		get {
			if (Dir == Orientation.Horizontal)
				return new Vector2(LineRange.Start, ConstValue);
			else return new Vector2(ConstValue, LineRange.Start);
		}
	}
	public Vector2 Endpoint2 {
		get {
			if (Dir == Orientation.Horizontal)
				return new Vector2(LineRange.End - LineRange.RoundedEpsilon, ConstValue);
			else return new Vector2(ConstValue, LineRange.End - LineRange.RoundedEpsilon);
		}
	}
	
	/// <summary>
	/// Finds whether or not the given lines together make a bigger unbroken Line.
	/// </summary>
	public static bool Connected(Line one, Line two) {
		
		const float error = 0.01f;
		
		return one.Dir == two.Dir &&
			   StateMachine.WithinError (one.ConstValue, two.ConstValue, error) &&
			   one.LineRange.Touches(two.LineRange);
			   //(WithinError (one.LineRange.Start, two.LineRange.End - two.LineRange.RoundedEpsilon, error) ||
			   // WithinError(one.LineRange.End - one.LineRange.RoundedEpsilon, two.LineRange.Start, error));
	}
	/// <summary>
	/// Takes two lines already known to be Connected and connects them.
	/// </summary>
	public static Line Connect(Line one, Line two) {
		
		float min = Mathf.Min(one.LineRange.Start, two.LineRange.Start);
		float max = Mathf.Max (one.LineRange.End - one.LineRange.RoundedEpsilon, two.LineRange.End - two.LineRange.RoundedEpsilon);
		
		return new Line(one.Dir, new Interval(min, max, true, one.LineRange.DecimalPlaceAccuracy), one.ConstValue);
	}
	
    /// <summary>
    /// Gets the wall bounds for a wall at the given spot in the level map.
    /// </summary>
    private static RecBounds GetBounds(Vector2 mapLoc)
    {
        return new RecBounds(mapLoc, new Vector2(1.0f, 1.0f));
    }
    /// <summary>
    /// Gets the wall bounds for a wall at the given spot in the level map.
    /// </summary>
    private static RecBounds GetBounds(Location mapLoc)
    {
        return new RecBounds(new Vector2(mapLoc.X, mapLoc.Y), new Vector2(1.0f, 1.0f));
    }

    /// <summary>
    /// Gets all continuous top/bottom Lines from the given level using all the walls in the given row.
    /// The Lines are indexed by all wall bounds that make up part of that indexed Line.
    /// </summary>
    public static void GetHorizontalLines(FillData data, WallToLines associations, int row)
    {
        int startingX, currentX;
        Interval temp;
        ColType current;
        bool bottomRow = (row == data.Map.GetLength(1) - 1);

        //Do the algorithm for both above and below the row.
        int currentRow;
        for (int dir = -1; dir < 2; dir += 2)
        {
            currentRow = row + dir;
            current = (currentRow < row ? ColType.Bottom : ColType.Top);

            //There is one special case: the whole row is a valid line.
            bool rowFree = true;
            for (int i = 0; i < data.Map.GetLength(0); ++i)
                if (data.GetMapAt(new Location(i, currentRow)) ||
                    !data.GetMapAt(new Location(i, row)))
                {
                    rowFree = false;
                    break;
                }

            //If the whole row is one line, just use that.
            if (rowFree)
                //If the level wraps around, use a large line.
                if (data.WrapX)
                    associations.AddReferences(GetBounds(new Line(Orientation.Horizontal, new Interval(0, data.Map.GetLength(0) - 1, true, 2), row)),
                                               current,
                                               new Line(Orientation.Horizontal, new Interval(-data.Map.GetLength(0), 2.0f * data.Map.GetLength(0), true, 2), (row + currentRow) * 0.5f));
                else associations.AddReferences(GetBounds(new Line(Orientation.Horizontal, new Interval(0, data.Map.GetLength(0) - 1, true, 2), row)),
                                                current,
                                                new Line(Orientation.Horizontal, new Interval(0, data.Map.GetLength(0) - 1, true, 2), row));

            //Otherwise, go through one wall at a time and build individual collections of Lines.
            else
            {
                //An edge is a whole Line if it spans an unbroken row of walls with no walls covering the edge.

                //Use a counter to run through the whole row in groups of unbroken rows.
                startingX = 0;
                currentX = 0;
                temp = null;
                while (startingX < data.Map.GetLength(0))
                {
                    //Get the next valid spot to start from.
                    while ((!data.GetMapAt(new Location(startingX, row)) ||
                            data.GetMapAt(new Location(startingX, currentRow))) &&
                           startingX < data.Map.GetLength(0))
                    {
                        currentX += 1;
                        startingX = currentX;
                    }
                    if (startingX >= data.Map.GetLength(0)) break;

                    //Keep the counter going as long as the next spot is valid (i.e.
                    //  there is a wall in this row but not above/underneath).
                    while (data.GetMapAt(new Location(currentX + 1, row)) &&
                           !data.GetMapAt(new Location(currentX + 1, currentRow)) &&
                           currentX < data.Map.GetLength(0))
                        currentX += 1;

                    //Now make the line.
                    temp = new Interval(startingX, currentX, true, 2);
                    associations.AddReferences(GetBounds(new Line(Orientation.Horizontal, temp, row)),
                                                         current,
                                                         new Line(Orientation.Horizontal, temp, (row + currentRow) * 0.5f));

                    //If this is near the left, wrap it around to the right.
                    if (data.WrapX && temp.Start < 2.0f)
                        associations.AddReferences(GetBounds(new Line(Orientation.Horizontal, temp + data.WorldSize.X, row)),
                                                   current,
                                                   new Line(Orientation.Horizontal, temp + data.WorldSize.X, (row + currentRow) * 0.5f));

                    //Move to the next spot in the row.
                    startingX = currentX + 1;
                    currentX = startingX;
                }

                //If the level wraps around in the X, take the last Line and wrap it around.
                if (temp != null && data.WrapX)
                {
                    associations.AddReferences(GetBounds(new Line(Orientation.Horizontal, temp - data.WorldSize.X, row)),
                                               current,
                                               new Line(Orientation.Horizontal, temp - data.WorldSize.X, (row + currentRow) * 0.5f));
                }
            }
        }
    }
    /// <summary>
    /// Gets all continuous left/right Lines from the given level using all the walls in the given column.
    /// The Lines are indexed by all wall bounds that make up part of that indexed Line.
    /// </summary>
    public static void GetVerticalLines(FillData data, WallToLines associations, int col)
    {
        int startingY, currentY;
        Interval temp;
        ColType current;
        bool rightCol = (col == data.Map.GetLength(0) - 1);

        //Do the algorithm for both the left and the right of the column.
        int currentCol;
        for (int dir = -1; dir < 2; dir += 2)
        {
            currentCol = col + dir;
            current = (currentCol > col ? ColType.Right : ColType.Left);

            //Take care of the edge case where the whole row is a valid line.
            bool colFree = true;
            for (int j = 0; j < data.Map.GetLength(1); ++j)
                if (data.GetMapAt(new Location(currentCol, j)) ||
                    !data.GetMapAt(new Location(col, j)))
                {
                    colFree = false;
                    break;
                }

            //If the whole column is one line, just use that.
            if (colFree)
                //If the level wraps around, use a large line.
                if (data.WrapY)
                    associations.AddReferences(GetBounds(new Line(Orientation.Vertical, new Interval(0, data.Map.GetLength(1) - 1, true, 2), col)),
                                               current,
                                               new Line(Orientation.Vertical, new Interval(-data.Map.GetLength(1), 2.0f * data.Map.GetLength(1), true, 2), (col + currentCol) * 0.5f));
                else associations.AddReferences(GetBounds(new Line(Orientation.Vertical, new Interval(0, data.Map.GetLength(1) - 1, true, 2), col)),
                                                current,
                                                new Line(Orientation.Vertical, new Interval(0, data.Map.GetLength(1) - 1, true, 2), col));

            //Otherwise, go through one wall at a time and build individual collections of Lines.
            else
            {
                //An edge is a whole Line if it spans an unbroken column of walls with no walls covering the edge.

                //Use a counter to run through the whole column in groups of unbroken walls.
                startingY = 0;
                currentY = 0;
                temp = null;
                while (startingY < data.Map.GetLength(1))
                {
                    //Get the next valid spot to start from.
                    while ((!data.GetMapAt(new Location(col, startingY)) ||
                            data.GetMapAt(new Location(currentCol, startingY))) &&
                           startingY < data.Map.GetLength(1))
                    {
                        currentY += 1;
                        startingY = currentY;
                    }
                    if (startingY >= data.Map.GetLength(1)) break;

                    //Keep the counter going as long as the next spot is valid (i.e.
                    //  there is a wall to the side but not in the way).
                    while (data.GetMapAt(new Location(col, currentY + 1)) &&
                           !data.GetMapAt(new Location(currentCol, currentY + 1)) &&
                           currentY < data.Map.GetLength(1))
                        currentY += 1;

                    //Now make the line.
                    temp = new Interval(startingY, currentY, true, 2);
                    associations.AddReferences(GetBounds(new Line(Orientation.Vertical, temp, col)),
                                                         current,
                                                         new Line(Orientation.Vertical, temp, (col + currentCol) * 0.5f));

                    //If this is near the top, wrap it around to the bottom.
                    if (data.WrapY && temp.Start < 2.0f)
                        associations.AddReferences(GetBounds(new Line(Orientation.Vertical, temp + data.WorldSize.Y, col)),
                                                   current,
                                                   new Line(Orientation.Vertical, temp + data.WorldSize.Y, (col + currentCol) * 0.5f));

                    //Move to the next spot in the column.
                    startingY = currentY + 1;
                    currentY = startingY;
                }

                //If the level wraps around in the Y, take the last Line and wrap it around.
                if (temp != null && data.WrapY)
                {
                    associations.AddReferences(GetBounds(new Line(Orientation.Vertical, temp - data.WorldSize.Y, col)),
                                               current,
                                               new Line(Orientation.Vertical, temp - data.WorldSize.Y, (col + currentCol) * 0.5f));
                }
            }
        }
    }
	
    /// <summary>
    /// Gets all wall bounds along the line, with no space in between them.
    /// </summary>
    private static IEnumerable<RecBounds> GetBounds(Line values)
    {
        const float error = 0.01f;

        for (float p = values.LineRange.Start;
             (p < values.LineRange.End - values.LineRange.RoundedEpsilon) || (Math.Abs(p - (values.LineRange.End - values.LineRange.RoundedEpsilon)) < error);
             ++p)
        {
            if (values.Dir == Orientation.Horizontal)
                yield return GetBounds(new Vector2(p, values.ConstValue));
            else yield return GetBounds(new Vector2(values.ConstValue, p));
        }
    }
	
    public static bool operator ==(Line one, Line two)
    {
        return one.Dir == two.Dir &&
               one.LineRange.Center == two.LineRange.Center &&
               one.LineRange.Range == two.LineRange.Range &&
               one.ConstValue == two.ConstValue;
    }
    public static bool operator !=(Line one, Line two)
    {
        return one.Dir != two.Dir ||
               one.LineRange.Center != two.LineRange.Center ||
               one.LineRange.Range != two.LineRange.Range ||
               one.ConstValue != two.ConstValue;
    }

    public override bool Equals(object obj)
    {
        Line l = obj as Line;
        return !ReferenceEquals(l, null) &&
               l.Dir == Dir &&
               l.ConstValue == ConstValue &&
               l.LineRange.Equals(LineRange);
    }
    public override int GetHashCode()
    {
        return LineRange.GetHashCode() + (7529 * ConstValue.GetHashCode()) + (7919 * (int)Dir);
    }
	
	public override string ToString ()
	{
		if (Dir == Orientation.Horizontal)
			return "A line from {" + LineRange.Start + ", " + ConstValue + "} to {" + (LineRange.End - LineRange.RoundedEpsilon) + ", " + ConstValue + "}";
		else 
			return "A line from {" + ConstValue + ", " + LineRange.Start + "} to {" + ConstValue + ", " + (LineRange.End - LineRange.RoundedEpsilon) + "}";
	}
}
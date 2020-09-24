using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//A system implementing Graph, Edge, and Node to represent a rectangular grid in 2D space.

/// <summary>
    /// A Graph representing a rectangular grid in 2-dimensional space.
    /// </summary>
public class RectangularGrid : Graph<PositionalNode>
{
    public float GridSpacingX, GridSpacingY;
    public bool IncludeDiagonals;
    public int DecimalPlaces;

    public RectangularGrid(float gridSpacingX, float gridSpacingY, bool includeDiagonals)
        : this(gridSpacingX, gridSpacingY, includeDiagonals, 3) { }
    public RectangularGrid(float gridSpacingX, float gridSpacingY, bool includeDiagonals, int decimalPlaces)
    {
        GridSpacingX = gridSpacingX;
        GridSpacingY = gridSpacingY;
        IncludeDiagonals = includeDiagonals;
    }

    public IEnumerable<Edge<PositionalNode>> GetConnections(PositionalNode starting)
    {
        float[] start = starting.Coordinate;

        //If diagonals are allowed, this is easy.
        if (IncludeDiagonals)
        {
            for (int i = -1; i <= 1; ++i)
                for (int j = -1; j <= 1; ++j)
                    yield return new PositionalEdge(start,
                                                    new float[2]
                                                        {
                                                            start[0] + (i * GridSpacingX),
                                                            start[1] + (j * GridSpacingY),
                                                        }, DecimalPlaces);
        }
        //Otherwise, manually get the four adjacent squares.
        else
        {
            yield return new PositionalEdge(start,
                                            new float[2]
                                                {
                                                    start[0] - GridSpacingX,
                                                    start[1],
                                                }, DecimalPlaces);
            yield return new PositionalEdge(start,
                                            new float[2]
                                                {
                                                    start[0] + GridSpacingX,
                                                    start[1],
                                                }, DecimalPlaces);
            yield return new PositionalEdge(start,
                                            new float[2]
                                                {
                                                    start[0],
                                                    start[1] - GridSpacingY,
                                                }, DecimalPlaces);
            yield return new PositionalEdge(start,
                                            new float[2]
                                                {
                                                    start[0],
                                                    start[1] + GridSpacingY,
                                                }, DecimalPlaces);
        }
    }

    public IEnumerable<PositionalNode> AllNodes(PositionalNode starting)
    {
        for (float f = starting.Coordinate[0]; true; f += GridSpacingX)
            for (float f2 = starting.Coordinate[1]; true; f2 += GridSpacingY)
                yield return new PositionalNode(new float[2] { f, f2 }, DecimalPlaces);
    }
}
/// <summary>
/// Identical to RectangularGrid, but with a limit on the bounds.
/// </summary>
public class LimitedRectangularGrid : Graph<PositionalNode>
{
    public Interval RegionX, RegionY;
    public bool WrapX, WrapY;

    public RectangularGrid Wrapping;

    public LimitedRectangularGrid(RectangularGrid wrapped,
                                  bool wrapX, bool wrapY,
                                  Interval allowableX, Interval allowableY)
    {
        WrapX = wrapX;
        WrapY = wrapY;

        RegionX = allowableX;
        RegionY = allowableY;

        Wrapping = wrapped;
    }

    public IEnumerable<Edge<PositionalNode>> GetConnections(PositionalNode starting)
    {
        float[] newEndCoords;

        //Go through each edge that would normally be returned.
        foreach (Edge<PositionalNode> e in Wrapping.GetConnections(starting))
        {
            newEndCoords = e.End.Coordinate.ToArray();

            //If it's outside the X bounds, either wrap it or discard it.
            if (!RegionX.Inside(newEndCoords[0]))
                if (WrapX)
                    newEndCoords[0] = RegionX.Wrap(newEndCoords[0]);
                else continue;

            //Do the same for the Y bounds.
            if (!RegionY.Inside(newEndCoords[1]))
                if (WrapY)
                    newEndCoords[1] = RegionY.Wrap(newEndCoords[1]);
                else continue;

            //Return the value.
            yield return new PositionalEdge(e.Start.Coordinate, newEndCoords, e.End.DecimalPlaces);
        }
    }

    public IEnumerable<PositionalNode> AllNodes(PositionalNode starting)
    {
        for (float x = starting.Coordinate[0]; x < RegionX.End; x += Wrapping.GridSpacingX)
            for (float y = starting.Coordinate[1]; y < RegionY.End; y += Wrapping.GridSpacingY)
                yield return new PositionalNode(new float[2] { x, y }, Wrapping.DecimalPlaces);
    }
}


/// <summary>
    /// Reprsents an edge connecting two points in space.
    /// The points are represented as float arrays;
    ///     any size array can be passed in to make points with any number of dimensions.
    /// </summary>
public class PositionalEdge : Edge<PositionalNode>
{
    /// <summary>
    /// Gets the distance between two points.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if "one" and "two" don't have the same length.</exception>
    private static float Dist(float[] one, float[] two)
    {
        SanityCheck(one, two, "Can't check the distance of two points in different coordinate spaces!");

        //Use an abstracted form of the distance formula.
        float insideSquareRoot = 0.0f;
        for (int i = 0; i < one.Length; ++i)
            insideSquareRoot += (float)System.Math.Pow(one[i] - two[i], 2.0f);

        return (float)System.Math.Sqrt(insideSquareRoot);
    }
    /// <summary>
    /// Gets the square of the distance between two points.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if "one" and "two" don't have the same length.</exception>
    private static float DistSquared(float[] one, float[] two)
    {
        SanityCheck(one, two, "Can't check the distance of two points in different coordinate spaces!");

        //Use an abstracted form of the distance formula, with no square root.
        float insideSquareRoot = 0.0f;
        for (int i = 0; i < one.Length; ++i)
            insideSquareRoot += (float)System.Math.Pow(one[i] - two[i], 2.0f);

        return insideSquareRoot;
    }

    /// <summary>
    /// Throws an ArgumentException if the two arrays have a different number of elements.
    /// </summary>
    private static void SanityCheck(float[] one, float[] two, string errorMessage)
    {
        if (one.Length != two.Length)
            throw new ArgumentException(errorMessage);
    }

    public PositionalEdge(float[] start, float[] end)
        : this(start, end, 3) { }
    public PositionalEdge(float[] start, float[] end, int decimalPlaces)
        : base(new PositionalNode(start), new PositionalNode(end))
    {
        SanityCheck(start, end, "The start and end nodes have differing dimensions!");
    }

    public override float Cost(PathFinder<PositionalNode> finder)
    {
        //Use a heuristic if the finder has a specific destination.
        if (finder.HasSpecificEnd)
            return Dist(Start.Coordinate, End.Coordinate) + DistSquared(End.Coordinate, finder.End.Coordinate);
        //Otherwise, use basic distance.
        else return Dist(Start.Coordinate, End.Coordinate);
    }
    public override float SearchCost(PathFinder<PositionalNode> finder)
    {
        //The search cost is just the distance between the nodes.
        return Dist(Start.Coordinate, End.Coordinate);
    }
}


/// <summary>
    /// A Node on a graph representing a Coordinate in space.
    /// </summary>
public class PositionalNode : Node
{
    public int DecimalPlaces;
    public float[] Coordinate;

    public PositionalNode(float[] coord)
        : this(coord, 3) { }
    public PositionalNode(float[] coord, int decimalPlaces)
    {
        Coordinate = coord.ToArray();
        DecimalPlaces = decimalPlaces;
        for (int i = 0; i < Coordinate.Length; ++i)
            Coordinate[i] = (float)System.Math.Round(Coordinate[i], DecimalPlaces);
    }

    public override bool Equals(object obj)
    {
        PositionalNode other = obj as PositionalNode;

        if (other == null) return false;
        if (other.Coordinate.Length != Coordinate.Length) return false;

        for (int i = 0; i < Coordinate.Length; ++i)
            if (Coordinate[i] != other.Coordinate[i])
                return false;

        return true;
    }

    public override int GetHashCode()
    {
        int prime = 7, hash = 0;
        for (int i = 0; i < Coordinate.Length; ++i)
        {
            hash += prime * (int)(Coordinate[i] * 100);
            //Get another prime: 2^n - 1 is a prime if n is a prime.
            prime = (int)((System.Math.Pow(2, prime) - 1) % Int32.MaxValue);
        }

        return hash;
    }

    public override string ToString()
    {
        string s = "{" + Coordinate[0].ToString();
        for (int i = 1; i < Coordinate.Length; ++i)
            s += ", " + Coordinate[i].ToString();
        return s + "}";
    }
}
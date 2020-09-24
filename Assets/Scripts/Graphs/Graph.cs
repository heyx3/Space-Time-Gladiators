using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//The basic building blocks of a graph space.

/// <summary>
    /// Represents a collection of Nodes connected by Edges.
    /// </summary>
    /// <typeparam name="N">A type representing a Node.</typeparam>
public interface Graph<N> where N : Node
{
    IEnumerable<Edge<N>> GetConnections(N starting);
    IEnumerable<N> AllNodes(N starting);
}

/// <summary>
    /// Represents the edge between two Nodes.
    /// </summary>
public abstract class Edge<N> where N : Node
{
    public N Start;
    public N End;

    public Edge(N start, N end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// The cost of traversing this Edge. Used in pathfinding algorithms.
    /// </summary>
    public abstract float Cost(PathFinder<N> finder);
    /// <summary>
    /// The cost of searching this Edge. Used to limit the reach of pathfinding algorithms.
    /// </summary>
    public abstract float SearchCost(PathFinder<N> finder);
}

/// <summary>
    /// Represents a specific spot on a graph.
    /// </summary>
public abstract class Node
{
    public override abstract bool Equals(object obj);
    public override abstract int GetHashCode();
}

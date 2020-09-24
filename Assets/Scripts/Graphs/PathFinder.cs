using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
    /// Uses A* to find a path through a graph.
    /// </summary>
    /// <typeparam name="N">The type representing a single node in the graph.</typeparam>
public class PathFinder<N> where N : Node
{
    //Basic path-finding data.

    public Graph<N> Graph;

    public N Start, End, DefaultVal;
    public bool HasSpecificEnd { get { return !End.Equals(DefaultVal); } }

    public Func<N, N, Edge<N>> MakeEdge;

    public bool FinishedCalculatingTree = false;

    /// <summary>
    /// Creates a new PathFinder.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="defaultN">The Node value representing a "null" value.</param>
    /// <param name="makeEdge">Converts a start and end node (in that order) into an edge.</param>
    public PathFinder(Graph<N> graph, N defaultN, Func<N, N, Edge<N>> makeEdge)
    {
        Graph = graph;

        DefaultVal = defaultN;
        Start = DefaultVal;
        End = DefaultVal;

        MakeEdge = makeEdge;
    }

    //Data for calculation. This data is stored as member variables and not returned because
    //  this way the calculation function can be kicked off in a different thread.

    public List<N> Considered = new List<N>();
    public List<N> PathEnds = new List<N>();

    public Dictionary<N, float> GetToNodeCost = new Dictionary<N, float>();
    public Dictionary<N, float> GetToNodeSearchCost = new Dictionary<N, float>();

    public Dictionary<N, N> EndToStart = new Dictionary<N, N>();

    /// <summary>
    /// An indexed priority queue holding the Edges to search. The "cost" for this IPQ is the cost of traversing the Edge.
    /// </summary>
    public IndexedPriorityQueue<Edge<N>> NodesToSearch = new IndexedPriorityQueue<Edge<N>>(true, new List<Edge<N>>(), new List<int>());

    public void ResetFinder()
    {
        FinishedCalculatingTree = false;

        Considered.Clear();
        PathEnds.Clear();

        GetToNodeCost.Clear();
        GetToNodeSearchCost.Clear();

        NodesToSearch.Clear();

        EndToStart.Clear();

    }

    /// <summary>
    /// Builds a search tree moving outward from Start.
    /// If there is a specific End Node, search towards that and stop when it is found.
    /// Otherwise, just search the entire graph space starting from Start.
    /// </summary>
    /// <param name="maxSearchCost">The maximum Edge search cost this pathfinder can search from start.
    /// This can be used to limit the graph search space.</param>
    public void CalculatePathTree(float maxSearchCost)
    {
        ResetFinder();

        //We're using A* algorithm.
        KeyValuePair<float, Edge<N>> closest;
        N tempCenter = DefaultVal;
        N closestN = DefaultVal;
        float closestCost, tempCost,
              closestSearchCost, tempSearchCost;


        //Start searching from the source node.
        AddNodeToSearchSpace(MakeEdge(DefaultVal, Start), 0.0f, 0.0f);

        //Keep track of the possible destination Nodes (in case the actual destination is too far away).
        List<N> possibleDestNodes = new List<N>();
        bool goesOn, noBranches;

        //While the search frontier is not empty, keep grabbing the nearest Node to search.
        while (!NodesToSearch.IsEmpty)
        {
            //Get the closest Node.
            closest = NodesToSearch.Pop();
            closestN = closest.Value.End;
            closestCost = closest.Key;
            closestSearchCost = GetToNodeSearchCost[closestN];

            //Put it into the Path.
            //If it was already in the Path, then a shorter route to it has already been found.
            if (!EndToStart.ContainsKey(closestN))
                EndToStart.Add(closestN, closest.Value.Start);

            //If the target has been found, or the graph search distance has been exceeded, exit.
            if ((HasSpecificEnd && closestN.Equals(End)) || GetToNodeSearchCost[closestN] >= maxSearchCost)
                break;

            //Now process all the connected nodes.
            //"goesOn" holds whether or not there is a usable Node connected to the current one.
            goesOn = false;
            //"noBranches" holds whether or not there exists a usable Node
            //  connected to the current one that can be added to the search fronter
            //  (e.x. a connected Node with a search cost above the threshold
            //    would set goesOn to true, and noBranches to true).
            noBranches = true;
            foreach (Edge<N> e in Graph.GetConnections(closestN))
                //Add the cell to the search space if it hasn't been processed already,
                //   and if the cell is the pathing destination.
                if (!Considered.Contains(e.End))
                {
                    //We know the path goes on.
                    goesOn = true;

                    //Get the search/traversal costs.
                    tempCost = closestCost + e.Cost(this);
                    tempSearchCost = closestSearchCost + e.SearchCost(this);

                    //If the search cost is small enough, add the edge to the search space.
                    if (tempSearchCost <= maxSearchCost)
                    {
                        noBranches = false;
                        AddNodeToSearchSpace(e, tempCost, tempSearchCost);
                    }
                }

            //If there are usable nodes pointing out of the current one, but they are all too far away,
            //   check this Node as a possible destination Node in case the real destination is too far away.
            if (goesOn && noBranches && HasSpecificEnd)
                PathEnds.Add(closestN);
        }

        FinishedCalculatingTree = true;
    }
    private void AddNodeToSearchSpace(Edge<N> toDest, float totalCost, float totalSearchCost)
    {
        NodesToSearch.Push(toDest, totalCost);
        GetToNodeCost.Add(toDest.End, totalCost);
        GetToNodeSearchCost.Add(toDest.End, totalSearchCost);
        Considered.Add(toDest.End);
    }

    /// <summary>
    /// Gets the path from Start to End, assuming the path tree has been computed.
    /// </summary>
    /// <returns>A list of the Nodes from Start to End, with the first element being Start
    /// and the last element being the closest to End the search tree could get.</returns>
    public List<N> CalculatePath()
    {
        if (End.Equals(DefaultVal))
            throw new ArgumentException("There is no set End for the path!");

        List<N> path = new List<N>();
        //Build backwards, since that's how the path was stored.
        N counter = End;
        while (!counter.Equals(Start))
        {
            path.Add(counter);
            counter = EndToStart[counter];
        }
        //Add in the start node.
        path.Add(Start);

        //Reverse the list to put it in the right order and return it.
        path.Reverse();
        return path;
    }
    /// <summary>
    /// Gets the End node, or the closest Node to End in the search space if End wasn't found.
    /// Assumes that End exists.
    /// </summary>
    private N GetDest()
    {
        if (EndToStart.ContainsKey(End))
            return End;

        //Couldn't find End, so get the Node closest to End.

        if (PathEnds.Count == 0)
            throw new InvalidOperationException("No possible search path ends to use as a destination!");

        N bestEnd = DefaultVal;
        float bestDist = Single.PositiveInfinity;
        float tempDist;

        foreach (N n in PathEnds)
        {
            tempDist = MakeEdge(n, End).Cost(this);
            if (tempDist < bestDist)
            {
                bestDist = tempDist;
                bestEnd = n;
            }
        }

        return bestEnd;
    }
}
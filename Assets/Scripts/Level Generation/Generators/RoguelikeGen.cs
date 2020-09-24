using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// The settings for a Roguelike level generator.
/// </summary>
public class RoguelikeGenSettings : GeneratorSettings
{
    public Location NumberOfNodes;

    public bool WrapAroundX, WrapAroundY;

    public override bool WrapX { get { return WrapAroundX; } set { WrapAroundX = value; } }
    public override bool WrapY { get { return WrapAroundY; } set { WrapAroundY = value; } }

    public Location RoomDimensions;
    public Interval RoomXVariance, RoomYVariance;
    public Location MaxRoomSize
    {
        get
        {
            return new Location(RoomDimensions.X + (int)(RoomXVariance.End),
                                RoomDimensions.Y + (int)(RoomYVariance.End));
        }
    }
    /// <summary>
    /// The X component is the X offset for a vertical corridor.
    /// The Y component is the Y offset for a horizontal corridor.
    /// </summary>
    public Location JunctionOffset
    {
        get
        {
            return new Location((MaxRoomSize.X - TunnelThickness.Y) / 2,
                                (MaxRoomSize.Y - TunnelThickness.X) / 2 + 1);
        }
    }

    /// <summary>
    /// The X component is the corresponding property for a horizontal corridor;
    /// the Y component is the corresponding property for a vertical one.
    /// </summary>
    public Location TunnelThickness;
    /// <summary>
    /// The X component is the corresponding property for a horizontal corridor;
    /// the Y component is the corresponding property for a vertical one.
    /// </summary>
    public Location TunnelLength;

    public float PercentRooms;

    public RoguelikeGenSettings()
        : base()
    {
        NumberOfNodes = new Location(4, 4);

        WrapAroundX = false;
        WrapAroundY = false;

        RoomDimensions = new Location(5, 5);
        RoomXVariance = new Interval(-3, 3, true, 0);
        RoomYVariance = new Interval(-3, 3, true, 0);

        TunnelThickness = new Location(3, 2);
        TunnelLength = new Location(5, 3);

        PercentRooms = 0.5f;

        //Fill patterns.

        FillPatterns.Add(new BlankRegionPattern(6));

        FillPatterns.Add(new CirclePattern(1));
        FillPatterns.Add(new CirclePattern(2));

        FillPatterns.Add(new PlatformsPattern(2, 1, 0.08f, 15, new Interval(0.6f, 1.6667f, true, 4)));
        ((PlatformsPattern)FillPatterns[FillPatterns.Count - 1]).MaxSuitability = Suitability.Moderate;

        FillPatterns.Add(new PlatformsPattern(1, 1, 0.6f, 15, new Interval(0.6f, 1.6667f, true, 4)));
        ((PlatformsPattern)FillPatterns[FillPatterns.Count - 1]).MaxSuitability = Suitability.Little;

        FillPatterns.Add(new ConcentricSquaresPattern());

        FillPatterns.Add(new AlternatingStepsPattern());

        FillPatterns.Add(new ZipperPattern(new PlateauFixedWidthAndSpace(2, 1, 1), new Interval(0.3f, 3.0f, true, 2), 20));
        FillPatterns.Add(new ZipperPattern(new PlateauFixedNumbAndSpace(4, 1, 1), new Interval(0.3f, 3.0f, true, 2), 20));
    }
}

/// <summary>
/// Generates a series of rooms connected by straight tunnels.
/// </summary>
public class RoguelikeGen : Generator
{
	public string Description { get; set; }
	
    //Basic level data.
    public bool[,] Map { get; set; }
    public List<Location> Holes { get; set; }
    public List<Region> Regions { get; set; }
    public List<FilledRegion> FilledRegions { get; set; }

    public FillData FillData { get; private set; }

    //Settings.
    public RoguelikeGenSettings Settings;
    public GeneratorSettings GenSettings { get { return Settings; } }
    public void SetSettings(GeneratorSettings s)
    {
        Settings = s as RoguelikeGenSettings;
    }
    public Location MaxRoomSize { get { return Settings.MaxRoomSize; } }

    //The graph/pathfinding.
    LimitedRectangularGrid Graph;
    PathFinder<PositionalNode> GraphPather;

    //Track the state of each Node and the size of all rooms.
    public enum NodeState { Untouched, TunnelJunction, Room }
    public Dictionary<PositionalNode, NodeState> NodeStates;
    public Dictionary<PositionalNode, Region> NodeAreas;

    //Basic constructor.
    public RoguelikeGen() { Description = ""; }

    public void FlipYsForGenSpecificData()
    {
        //The size of the cell grid is found by combining each node
        //   (assumed to have the largest-possible room size) with each connecting tunnel.
        Location cells = new Location(((Settings.NumberOfNodes.X - 1) * Settings.TunnelLength.X) +
                                      (Settings.NumberOfNodes.X * MaxRoomSize.X) + 2,
                                      ((Settings.NumberOfNodes.Y - 1) * Settings.TunnelLength.Y) +
                                      (Settings.NumberOfNodes.Y * MaxRoomSize.Y) + 2);

        //Set up the graph/pather.
        Graph = new LimitedRectangularGrid(new RectangularGrid(Settings.TunnelLength.X + MaxRoomSize.X,
                                                               Settings.TunnelLength.Y + MaxRoomSize.Y,
                                                               false),
                                           false, false,
                                           new Interval(0, cells.X - 1, true, 0),
                                           new Interval(0, cells.Y - 1, true, 0));
        GraphPather = new PathFinder<PositionalNode>(Graph,
                                                     new PositionalNode(new float[2] { Single.NaN, Single.NaN }, 0),
                                                     (n, n2) => new PositionalEdge(n.Coordinate, n2.Coordinate, 0));

        //Rebuild the node areas and states.
        Dictionary<PositionalNode, Region> newNodeAreas = new Dictionary<PositionalNode, Region>();
        Dictionary<PositionalNode, NodeState> newNodeStates = new Dictionary<PositionalNode, NodeState>();
        PositionalNode oldNode;
        Region oldNodeArea;

        //First get all Y coordinates for rooms/tunnels.
        List<float> nodeYs = new List<float>();
        foreach (PositionalNode n in NodeStates.Keys)
        {
            if (!nodeYs.Contains(n.Coordinate[1]))
            {
                nodeYs.Add(n.Coordinate[1]);
            }
        }

        //Go through every node and flip its Y value.
        foreach (PositionalNode n in Graph.AllNodes(new PositionalNode(new float[2] { 1.0f, 1.0f })))
        {
            int indexOf = nodeYs.IndexOf(n.Coordinate[1]);
            int flippedIndexOf = nodeYs.Count - 1 - indexOf;

            oldNode = new PositionalNode(new float[2] { n.Coordinate[0], nodeYs[flippedIndexOf] });
            oldNodeArea = NodeAreas[oldNode];

            newNodeStates.Add(n, NodeStates[oldNode]);
            newNodeAreas.Add(n, new Region(oldNodeArea.X, Map.GetLength(1) - 1 - oldNodeArea.Y, oldNodeArea.Width, -oldNodeArea.Height, true));
        }

        NodeAreas = newNodeAreas;
        NodeStates = newNodeStates;
    }

    //Base generation (i.e. the rooms/junctions).

    public void InitializeBase()
    {
        //The size of the cell grid is found by combining each node
        //   (assumed to have the largest-possible room size) with each connecting tunnel.
        Location cells = new Location(((Settings.NumberOfNodes.X - 1) * Settings.TunnelLength.X) +
                                      (Settings.NumberOfNodes.X * MaxRoomSize.X) + 2,
                                      ((Settings.NumberOfNodes.Y - 1) * Settings.TunnelLength.Y) +
                                      (Settings.NumberOfNodes.Y * MaxRoomSize.Y) + 2);
        //Set up the graph/pather.
        Graph = new LimitedRectangularGrid(new RectangularGrid(Settings.TunnelLength.X + MaxRoomSize.X,
                                                               Settings.TunnelLength.Y + MaxRoomSize.Y,
                                                               false),
                                           false, false,
                                           new Interval(0, cells.X - 1, true, 0),
                                           new Interval(0, cells.Y - 1, true, 0));
        GraphPather = new PathFinder<PositionalNode>(Graph,
                                                     new PositionalNode(new float[2] { Single.NaN, Single.NaN }, 0),
                                                     (n, n2) => new PositionalEdge(n.Coordinate, n2.Coordinate, 0));

        //Set up the room map. Initial value is a fully-filled room.
        Map = new bool[cells.X, cells.Y];
        for (int i = 0; i < Map.GetLength(0); ++i)
            for (int j = 0; j < Map.GetLength(1); ++j)
                Map[i, j] = true;
        
        //Initialize the room nodes.
        NodeAreas = new Dictionary<PositionalNode, Region>();
        NodeStates = new Dictionary<PositionalNode, NodeState>();
        foreach (PositionalNode n in Graph.AllNodes(new PositionalNode(new float[2] { 1.0f, 1.0f })))
        {
            NodeStates.Add(n, NodeState.Untouched);
        }

        //Initialize other data.
        Regions = new List<Region>();
        FilledRegions = new List<FilledRegion>();
        Holes = new List<Location>();

        //Initialize counter to iterate through base room generation.
        DoneBaseGen = false;
        listVersionBGIC = NodeStates.Keys.ToList();
        baseGenIterateCounter = listVersionBGIC.GetEnumerator();
        baseGenIterateCounter.MoveNext();

        //Set up the fill data.
        FillData = new FillData(Map, new Region(-1, -1, -1, -1), Regions, Holes, GenSettings.WrapX, GenSettings.WrapY, true);
    }
    private IEnumerator<PositionalNode> baseGenIterateCounter;
    private List<PositionalNode> listVersionBGIC;
    private int count = 0;
    public bool DoneBaseGen { get; private set; }
    public void IterateBase()
    {
        if (DoneBaseGen) return;

        count += 1;

        PositionalNode current = baseGenIterateCounter.Current;

        //Make either a room or a tunnel junction at the given spot.
        if (GeneratorSettings.R.NextDouble() < Settings.PercentRooms)
        {
            //Room.

            Location size = new Location(Settings.RoomDimensions.X + (int)Settings.RoomXVariance.Random(),
                                         Settings.RoomDimensions.Y + (int)Settings.RoomYVariance.Random());
            size = new Location(System.Math.Max(size.X, Settings.TunnelThickness.X),
                                System.Math.Max(size.Y, Settings.TunnelThickness.Y));
            Region room = new Region((int)current.Coordinate[0],
                                     (int)current.Coordinate[1],
                                     size.X - 1, size.Y - 1);
            if (!room.SubRegionOf(new Region(0, 0, Map.GetLength(0), Map.GetLength(1))))
            {
                DoneBaseGen = !baseGenIterateCounter.MoveNext();
                NodeStates.Remove(current);
                return;
            }

            //Move the room to the center.
            Location offset = new Location((int)Math.Round((MaxRoomSize.X - room.Width - 1) / 2.0f, 0), (int)Math.Round((MaxRoomSize.Y - room.Height - 1) / 2.0f, 0));
            room = new Region(room.TopLeft + offset, room.BottomRight + offset);

            //Clear the room area.
            Regions.Add(room);
            FillData.FillRegion(false, room);
            //Add the room data.
            NodeStates[current] = NodeState.Room;
            NodeAreas.Add(current, room);
        }
        else
        {
            //Tunnel junction.

            Location size = MaxRoomSize;
            Region space = new Region((int)current.Coordinate[0],
                                     (int)current.Coordinate[1],
                                     size.X - 1, size.Y - 1);

            space = new Region(space.Left + Settings.JunctionOffset.X,
                               space.Top + Settings.JunctionOffset.Y,
                               Settings.TunnelThickness.Y - 1,
                               Settings.TunnelThickness.X - 1);
            if (!space.SubRegionOf(new Region(0, 0, Map.GetLength(0), Map.GetLength(1))))
            {
                DoneBaseGen = !baseGenIterateCounter.MoveNext();
                NodeStates.Remove(current);
                return;
            }

            Regions.Add(space);
            FillData.FillRegion(false, space);

            //Add the room data.
            NodeStates[current] = NodeState.TunnelJunction;
            NodeAreas.Add(current, space);
        }

        //Prepare for the next iteration.
        DoneBaseGen = !baseGenIterateCounter.MoveNext();
    }
    public void GenerateBase()
    {
        InitializeBase();

        while (NotFinishedBase())
        {
            InitializeBase();
            while (!DoneBaseGen) IterateBase();
        }

        //Initialize counter to iterate through fill pattern generation.
        NeedToApply = NodeStates.Keys.ToList().GetEnumerator();
        DoneFillPatterns = false;
        NeedToApply.MoveNext();
    }
    /// <summary>
    /// Returns true if there exists no nodes representing a room
    /// or at least one node that hasn't been set to a room or junction yet.
    /// </summary>
    public bool NotFinishedBase()
    {
        bool foundRoom = false;

        foreach (KeyValuePair<PositionalNode, NodeState> kvp in NodeStates)
            switch (kvp.Value)
            {
                case NodeState.Room:
                    foundRoom = true;
                    continue;
                case NodeState.TunnelJunction:
                    continue;
                case NodeState.Untouched:
                    return true;
            }

        return !foundRoom;
    }

    //Tunnel generation.

    private int HorizontalTunnelY(int nodeY) { return nodeY + Settings.JunctionOffset.Y; }
    private int VerticalTunnelX(int nodeX) { return nodeX + Settings.JunctionOffset.X; }

    public void BeforeFillPatterns()
    {
        FillTunnels();
        CombineTunnels();
    }
    private void FillTunnels()
    {
        //Go through all nodes and make tunnels,
        //  with special edge tunnels around the perimeter (if the level wraps around).
        Location lastNodePos = (Settings.TunnelLength + MaxRoomSize) * (Settings.NumberOfNodes - new Location(1, 1));
        Location topLeft, nextTopLeft;
        Region beingCleared;

        foreach (PositionalNode n in NodeStates.Keys)//Graph.AllNodes(new PositionalNode(new float[2] { 1.0f, 1.0f }, 0)))
        {
            //Fill in the tunnel to the right and down from this cell.

            //Down.
            if (n.Coordinate[1] < lastNodePos.Y)
            {
                topLeft = new Location(VerticalTunnelX((int)n.Coordinate[0]), NodeAreas[n].Bottom);
                if (NodeStates[n] == NodeState.Room)
                    topLeft = topLeft.Below;
                nextTopLeft = NodeAreas[new PositionalNode(new float[2] { n.Coordinate[0], n.Coordinate[1] + MaxRoomSize.Y + Settings.TunnelLength.Y }, 0)].TopLeft;

                beingCleared = new Region(topLeft, new Location(topLeft.X + Settings.TunnelThickness.Y - 1, nextTopLeft.Y - 1));
                FillData.FillRegion(false, beingCleared);
                Regions.Add(beingCleared);

                for (int x = beingCleared.Left; x <= beingCleared.Right; ++x)
                {
                    Holes.Add(new Location(x, beingCleared.Top));
                    Holes.Add(new Location(x, beingCleared.Bottom));
                }
            }
            //Right.
            if (n.Coordinate[0] < lastNodePos.X)
            {
                topLeft = new Location(NodeAreas[n].Right, HorizontalTunnelY((int)n.Coordinate[1])).Right;
                nextTopLeft = NodeAreas[new PositionalNode(new float[2] { n.Coordinate[0] + MaxRoomSize.X + Settings.TunnelLength.X, n.Coordinate[1] }, 0)].TopLeft;

                beingCleared = new Region(topLeft, new Location(nextTopLeft.X - 1, topLeft.Y + Settings.TunnelThickness.X - 1));
                FillData.FillRegion(false, beingCleared);
                Regions.Add(beingCleared);

                for (int y = beingCleared.Top; y <= beingCleared.Bottom; ++y)
                {
                    Holes.Add(new Location(beingCleared.Left, y));
                    Holes.Add(new Location(beingCleared.Right, y));
                }
            }

            //Border cases.

            //Left border.
            if (Settings.WrapAroundX && n.Coordinate[0] == 1.0f)
            {
                topLeft = new Location(0, HorizontalTunnelY((int)n.Coordinate[1]));
                nextTopLeft = NodeAreas[n].TopLeft.Left;

                beingCleared = new Region(topLeft, new Location(nextTopLeft.X, topLeft.Y + Settings.TunnelThickness.X - 1));
                FillData.FillRegion(false, beingCleared);
                Regions.Add(beingCleared);

                for (int y = beingCleared.Top; y <= beingCleared.Bottom; ++y)
                    Holes.Add(new Location(beingCleared.Right, y));
            }
            //Top border.
            if (Settings.WrapAroundY && n.Coordinate[1] == 1.0f)
            {
                topLeft = new Location(VerticalTunnelX((int)n.Coordinate[0]), 0);
                nextTopLeft = NodeAreas[n].TopLeft.Above;

                beingCleared = new Region(topLeft, new Location(topLeft.X + Settings.TunnelThickness.Y - 1, nextTopLeft.Y));
                FillData.FillRegion(false, beingCleared);
                Regions.Add(beingCleared);

                for (int x = beingCleared.Left; x <= beingCleared.Right; ++x)
                    Holes.Add(new Location(x, beingCleared.Bottom));
            }
            //Right border.
            if (Settings.WrapAroundX && n.Coordinate[0] == lastNodePos.X + 1)
            {
                topLeft = new Location(NodeAreas[n].Right, HorizontalTunnelY((int)n.Coordinate[1])).Right;

                beingCleared = new Region(topLeft, new Location(Map.GetLength(0) - 1, topLeft.Y + Settings.TunnelThickness.X - 1));
                FillData.FillRegion(false, beingCleared);
                Regions.Add(beingCleared);

                for (int y = beingCleared.Top; y <= beingCleared.Bottom; ++y)
                    Holes.Add(new Location(beingCleared.Left, y));
            }
            //Bottom border.
            if (Settings.WrapAroundY && n.Coordinate[1] == lastNodePos.Y + 1)
            {
                topLeft = new Location(VerticalTunnelX((int)n.Coordinate[0]), NodeAreas[n].Bottom).Below;

                beingCleared = new Region(topLeft, new Location(topLeft.X + Settings.TunnelThickness.Y - 1, Map.GetLength(1) - 1));
                FillData.FillRegion(false, beingCleared);
                Regions.Add(beingCleared);

                for (int x = beingCleared.Left; x <= beingCleared.Right; ++x)
                    Holes.Add(new Location(x, beingCleared.Top));
            }
        }
    }
    private void CombineTunnels()
    {
        //Get all the regions not representing a room.
        List<Region> regions = new List<Region>();
        //Get all Regions on this map that aren't a room.
        PositionalNode temp;
        foreach (Region r in Regions)
        {
            temp = GetKey(r);
            if (temp != null)
                switch (NodeStates[temp])
                {
                    case NodeState.Room:
                        continue;
                    case NodeState.TunnelJunction:
                        regions.Add(r);
                        break;
                    default: throw new InvalidOperationException("A node wasn't set to a room or junction!");
                }
            else regions.Add(r);
        }

        //Now combine all those regions together.
        regions = Region.Combine(regions);

        //Now create a new Regions list: with these combined regions and all the room regions.
        Regions = regions;
        foreach (PositionalNode key in NodeStates.Keys)
        {
            //Ignore any regions that are part of the junction.
            //Add any regions that are part of a room.
            switch (NodeStates[key])
            {
                case NodeState.Room:
                    Regions.Add(NodeAreas[key]);
                    break;
                case NodeState.TunnelJunction:
                    continue;

                default: throw new InvalidOperationException("A node wasn't set to a room or junction!");
            }
        }
    }
    /// <summary>
    /// Gets the first key from NodeAreas that has the given value, or "null" if none exists.
    /// </summary>
    private PositionalNode GetKey(Region value)
    {
        foreach (KeyValuePair<PositionalNode, Region> kvp in NodeAreas)
            if (kvp.Value == value)
                return kvp.Key;

        return null;
    }

    //Fill patterns.

    public IEnumerator<PositionalNode> NeedToApply;
    public bool DoneFillPatterns { get; private set; }
    public void IterateFillPattern()
    {
        if (DoneFillPatterns) return;

        //Get the current node.
        PositionalNode n = NeedToApply.Current;
        //If it is a junction, ignore it for now.
        //FilledRegions covering junctions/tunnels will be
        //   added after fill patterns are applied to rooms.
        if (NodeStates[n] == NodeState.TunnelJunction)
        {
            DoneFillPatterns = !NeedToApply.MoveNext();
            return;
        }
        
        //Get the best fill pattern for it and apply it if one exists.
        FillPattern p = Settings.MostSuitable(NodeAreas[n]);
        FilledRegion tempF;
        if (p != null)
        {
            FillData.BeingFilled = NodeAreas[n];
            tempF = p.Apply(FillData);
            //If the area has no team spawns, add one at the top.
            if (tempF.PotentialSpawns[Spawns.Team].Count == 0 && tempF.Covering.Width + 1 >= 4)
            {
                tempF.PotentialSpawns[Spawns.Team].Add(new Region(tempF.Covering.Left, tempF.Covering.Top, tempF.Covering.Width, 0));
            }
            FilledRegions.Add(tempF);

        }
        else FilledRegions.Add(new NoRegion(NodeAreas[n]));

        //Continue the iteration.
        DoneFillPatterns = !NeedToApply.MoveNext();
    }
    public void ApplyFillPatterns()
    {
        //Keep going until all patterns have been applied.
        while (!DoneFillPatterns) IterateFillPattern();
    }

    //Making FilledRegions for tunnels.
    public void AfterFillPatterns()
    {
        //First, see if there are enough team base spawns just going by the rooms.
        //If there are, then we won't use any team spawns in tunnels.

        const int maxNumbTeams = 8;
        int teamSpawns = 0;

        foreach (FilledRegion f in FilledRegions)
            teamSpawns += f.PotentialSpawns[Spawns.Team].Count;
        bool enoughSpawns = teamSpawns >= maxNumbTeams;
        
        //Find all tunnel regions and make a FilledRegion for them.

        TunnelRegion temp;
        List<Region> rooms = new List<Region>();
        foreach (PositionalNode key in NodeAreas.Keys)
            if (NodeStates[key] == NodeState.Room)
                rooms.Add(NodeAreas[key]);
        foreach (Region r in Regions) if (!rooms.Contains(r))
        {
            temp = new TunnelRegion(r, Map);
            if (enoughSpawns) temp.PotentialSpawns[Spawns.Team].Clear();

            FilledRegions.Add(temp);
        }
    }

    //The full generation method.
    public void FullGenerate()
    {
        GenerateBase();

        BeforeFillPatterns();

        ApplyFillPatterns();

        AfterFillPatterns();
    }
}
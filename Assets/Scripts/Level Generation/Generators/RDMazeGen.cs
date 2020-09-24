using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// The settings for a Recursive-Division Maze Generator.
/// </summary>
public class RDMazeGenSettings : GeneratorSettings
{
    public float RemoveTrailingEdgesChance;

    public Location Size { get; set; }

    public override bool WrapX { get; set; }
    public override bool WrapY { get; set; }
    
    /// <summary>
    /// Gets a number from 0-1 indicating how likely it is for the given region to be left empty during the basic maze generation.
    /// </summary>
    public Func<Region, double> IgnoreRegionChance;
    /// <summary>
    /// Gets a function for the IgnoreRegionChance member that returns (from 0-1) how close the given region is to the given ideal region.
    /// </summary>
    /// <param name="idealSize">The ideal region to be ignored.</param>
    /// <param name="minimumSize">The largest possible region that should still return a 0 in the function.</param>
    /// <param name="ignoreMinimum">Any regions this size or smaller will definitely be ignored.</param>
    public static Func<Region, double> IdealRegionFunc(Location idealSize, Location minimumSize, Location ignoreMinimum)
    {
        if (idealSize.X < minimumSize.X)
            throw new ArgumentException("The ideal width is smaller than the minimum width!");
        if (idealSize.Y < minimumSize.Y)
            throw new ArgumentException("The ideal height is smaller than the minimum height!");

        double minimumToIdealDist = Math.Sqrt(Math.Pow(idealSize.X - minimumSize.X, 2) + Math.Pow(idealSize.Y - minimumSize.Y, 2));

        return r =>
            {
                if (r.Width <= ignoreMinimum.X || r.Height <= ignoreMinimum.Y) return 1.0f;

                //Give up if the region is too small.
                double dist = Math.Sqrt(Math.Pow(idealSize.X - r.Width, 2) + Math.Pow(idealSize.Y - r.Height, 2));
                if (dist > minimumToIdealDist)
                    return 0.0;

                return 1.0 - (dist / minimumToIdealDist);
            };
    }
    /// <summary>
    /// Gets a function for the IgnoreRegionChance member that returns (from 0-1) how close the given region is to the given ideal region.
    /// </summary>
    /// <param name="idealArea">The ideal region to be ignored.</param>
    /// <param name="minimumArea">The smallest allowable area that should still have a chance of being ignored.</param>
    /// <param name="ignoreMinimum">Any regions this size or smaller will definitely be ignored.</param>
    public static Func<Region, double> IdealRegionFunc(int idealArea, int minimumArea, int ignoreMinimum)
    {
        if (idealArea < minimumArea) throw new ArgumentException("The ideal region is smaller than the minimum area!");

        double minToIdealDist = idealArea - minimumArea;

        return r =>
            {
                if (r.Area <= ignoreMinimum) return 1.0f;

                //Give up if the region is too small or large.
                double dist = Math.Abs(idealArea - r.Area);
                if (dist > minToIdealDist) return 0.0;

                return 1.0 - (dist / minToIdealDist);
            };
    }
    /// <summary>
    /// Creates a function where the chance of a region being ignored goes to 1.0 as the ratio of its dimensions approaches a given ratio.
    /// </summary>
    /// <param name="idealRatio">The "ideal" ratio of width/height for a region to be ignored.</param>
    /// <param name="farthestRatio">The ratio farthest from the ideal which should still have some tiny chance of being ignored.</param>
    public static Func<Region, double> IdealDimensionsFunc(float idealRatio, float farthestRatio)
    {
        double minToIdeal = Math.Abs(idealRatio - farthestRatio);

        return r =>
            {
                double toIdeal = Math.Abs(idealRatio - ((double)r.X / (double)r.Y));

                if (toIdeal < minToIdeal) return 0.0f;

                return 1.0 - (toIdeal / minToIdeal);
            };
    }

    /// <summary>
    /// Takes the length of a line splitting a region and gives how many holes in the line there should be.
    /// </summary>
    public Func<int, int> HolesInLine;
    /// <summary>
    /// A function that always returns 1 hole in a line regardless of its size.
    /// </summary>
    public static Func<int, int> OneHoleInLine = ConstantHolesInLine(1);
    /// <summary>
    /// A function that always returns a certain constant number of holes in a line regardless of size.
    /// </summary>
    public static Func<int, int> ConstantHolesInLine(int holes) { return i => holes; }
    /// <summary>
    /// Takes some data and returns a function that takes the length of a line splitting a region and gives a large number of holes for a medium-size line and a few number of holes for a large or small line.
    /// </summary>
    /// <param name="maxHoles">The maximum number of holes to return.</param>
    /// <param name="maxSize">The largest possible line size.</param>
    public static Func<int, int> ManyHolesForMediumLine(int maxHoles, int maxSize)
    {
        int minHoles = 1;
        int minSize = 2;

        //Oscillate between few and many holes. The oscillation should start at the minimum holes for a line of length 1, and end at the minimum holes for a line of length [maxSize].
        Func<float, float> sin = MathF.GetSine(new Interval(minHoles, maxHoles, true, 3), maxSize - minSize, minHoles);
        return i => (int)Math.Round(sin(i), 0);
    }

    /// <summary>
    /// Takes the width/height of a region and returns true if it should split horizontally and false if it should split vertically.
    /// </summary>
    public Func<Location, bool> ShouldSplitHorizontally;
    /// <summary>
    /// Gets a function that always returns "true" with the same probability regardless of the region being split.
    /// </summary>
    /// <param name="chanceHorizontal">The chance of the split being horizontal.</param>
    public static Func<Location, bool> ConstantSplitChance(float chanceHorizontal)
    {
        return l => R.NextDouble() < chanceHorizontal;
    }
    /// <summary>
    /// Gets a function that says to split horizontally if the width is smaller than the height, else split vertically.
    /// </summary>
    public static Func<Location, bool> SplitLargerDimension()
    {
        return l =>
            {
                if (l.X > l.Y) return false;
                else return true;
            };
    }

    /// <summary>
    /// Takes (in order) the start and end values of a region to be split (if horizontal, the y values; if vertical, the x values), and returns the location to place the line split.
    /// </summary>
    public Func<int, int, int> SplitLocation;
    /// <summary>
    /// Gets a function that puts the split line at a completely random location.
    /// </summary>
	public static Func<int, int, int> RandomSplitLocation() {
		return RandomSplitLocation(-1);
	}
    /// <summary>
    /// Gets a function that puts the split line at a completely random location.
    /// </summary>
    /// <param name="border">If a positive number, this function will attempt to keep the split line at least "border" distance away from the region edge.</param>
    public static Func<int, int, int> RandomSplitLocation(int border)
    {
        if (border > -1)
            return (i, j) =>
                {
                    if ((j - i) / 2 >= border)
                        return R.Next(i + border, j - border + 1);
                    return (j + i) / 2;
                };
        return (i, j) => R.Next(i, j + 1);
    }
    /// <summary>
    /// Gets a function that splits the region proportionally to its size: the smaller the region, the more centered the split.
    /// </summary>
    /// <param name="maxSize">The largest possible side of a region being split.</param>
    /// <param name="variation">The maximum-allowable random variance in the hole placement.</param>
    /// <param name="reverse">If true, the region will be split more towards the center as it GROWS in size.</param>
    public static Func<int, int, int> CenterSplitAsRegionShrinks(int maxSize, int variation, bool reverse)
    {
        //The smallest possible region being split will span two lengths minimum.
        const int minSize = 2;
        //To get this function, represent it as a line.

        if (reverse)
        {
            //More centered with bigger regions.
            float slope = -1.0f / (maxSize - minSize);
            float yIntercept = (float)maxSize / (maxSize - minSize);
            return (min, max) =>
            {
                float middle = (min + max) * 0.5f;
                int size = max - min;

                float offset = ((slope * size) + yIntercept) * size * 0.5f;

                //Randomly choose between going above the middle and below the middle.
                return Clamp(min, max, R.Next(-variation, variation + 1) + (int)Math.Round((R.NextDouble() < 0.5) ? (middle + offset) : (middle - offset), 0));
            };
        }
        else
        {
            //More centered with smaller regions.
            float slope = 1.0f / (maxSize - minSize);
            float yIntercept = -(float)minSize / (maxSize - minSize);
            return (min, max) =>
            {
                float middle = (min + max) * 0.5f;
                int size = max - min;

                float offset = ((slope * size) + yIntercept) * size * 0.5f;

                //Randomly choose between going above the middle and below the middle.
                return Clamp(min, max, R.Next(-variation, variation + 1) + (int)Math.Round((R.NextDouble() < 0.5) ? (middle + offset) : (middle - offset), 0));
            };
        }
    }
    private static int Clamp(int low, int high, int i)
    {
        if (i < low) return low;
        if (i > high) return high;
        return i;
    }

    public RDMazeGenSettings(Location worldSize)
        : base()
    {
        Size = worldSize;

        FillPatterns.Add(new BlankRegionPattern());

        FillPatterns.Add(new SteppedHallwayPattern(new PlateauFixedNumbAndSpace(4, 2, 2), 0.3f));
        FillPatterns.Add(new SteppedHallwayPattern(new PlateauFixedWidthAndSpace(2, 2, 2), 0.3f));
        FillPatterns.Add(new SteppedHallwayPattern(new PlateauFixedWidthAndSpace(5, 1, 2), 0.3f));

        FillPatterns.Add(new ThickHurdlePattern(new PlateauFixedNumbAndSpace(4, 2, 2), 0.6f, 0.3f));
        FillPatterns.Add(new ThickHurdlePattern(new PlateauFixedWidthAndSpace(2, 2, 2), 0.1f, 0.0f));
        FillPatterns.Add(new ThickHurdlePattern(new PlateauFixedWidthAndSpace(5, 1, 2), 0.8f, 0.15f));

        FillPatterns.Add(new CarvedPlusPattern());

        FillPatterns.Add(new XPattern(new Interval(0.46f, 2.174f, true, 3)));

        FillPatterns.Add(new CirclePattern(2));
        FillPatterns.Add(new CirclePattern(3));

        PlatformsPattern temp = new PlatformsPattern(2, 2, 0.3f, 15, new Interval(0.8f, 1.25f, true, 3));
        temp.MaxSuitability = Suitability.Little;
        FillPatterns.Add(temp);

        temp = new PlatformsPattern(2, 1, 0.2f, 15, new Interval(0.8f, 1.25f, true, 3));
        temp.MaxSuitability = Suitability.Little;
        FillPatterns.Add(temp);

        FillPatterns.Add(new ConcentricSquaresPattern());

        FillPatterns.Add(new LinePattern());

        FillPatterns.Add(new AlternatingStepsPattern());
    }
}

/// <summary>
/// A map generator that uses a modified Recursive-Division algorithm to make a 2D maze, then uses that maze as the map.
/// </summary>
public class RDMazeGen : Generator
{
    //TOOD: If a region is empty after a pattern is applied, replace its FilledRegion object with a BlankRegion.
	
	public string Description { get; set; }
	
    public bool[,] Map { get; set; }
    public List<Region> Regions { get; set; }
    public List<FilledRegion> FilledRegions { get; set; }
    public List<Location> Holes { get; set; }

    public FillData FillData { get; private set; }

    /// <summary>
    /// Gets all holes between regions touching the edge of the given area.
    /// </summary>
    public List<Location> GetHolesOnPerimeter(Region area)
    {
        List<Location> ret = new List<Location>();

        foreach (Location l in Holes)
            if (area.OnEdge(l))
                ret.Add(l);

        return ret;
    }

    public Location Size
    {
        get
        {
            Location l = Settings.Size;

            if (Settings.WrapX)
            {
                l.X += 1;
            }
            if (Settings.WrapY)
            {
                l.Y += 1;
            }

            return l;
        }
    }

    public RDMazeGenSettings Settings;
    public GeneratorSettings GenSettings { get { return Settings; } }

    public RDMazeGen() { Description = ""; }

    public void FlipYsForGenSpecificData() { }

    public void SetSettings(GeneratorSettings s)
    {
        Settings = s as RDMazeGenSettings;
    }

    public Stack<Region> RegionsToSplit;
    private Region current;
    public void InitializeBase()
    {
        Regions = new List<Region>();
        RegionsToSplit = new Stack<Region>();
        FilledRegions = new List<FilledRegion>();
        Holes = new List<Location>();

        Map = new bool[Size.X, Size.Y];
        for (int i = 0; i < Map.GetLength(0); ++i)
        {
            for (int j = 0; j < Map.GetLength(1); ++j)
            {
                if ((!Settings.WrapX && (i == 0 || i == Map.GetLength(0) - 1)) ||
                    (!Settings.WrapY && (j == 0 || j == Map.GetLength(1) - 1)))
                {
                    Map[i, j] = true;
                }
                else
                {
                    Map[i, j] = false;
                }
            }
        }

        if (!Settings.WrapX && !Settings.WrapY)
        {
            RegionsToSplit.Push(new Region(1, 1, Size.X - 3, Size.Y - 3));
        }
        else if (!Settings.WrapX)
        {
            RegionsToSplit.Push(new Region(1, 0, Size.X - 3, Size.Y - 1));
        }
        else if (!Settings.WrapY)
        {
            RegionsToSplit.Push(new Region(0, 1, Size.X - 1, Size.Y - 3));
        }
        else
        {
            RegionsToSplit.Push(new Region(0, 0, Size.X - 1, Size.Y - 1));
        }

        iterateFillPatternsRegionCount = 0;

        FillData = new FillData(Map, new Region(-1, -1, -1, -1), Regions, Holes, Settings.WrapX, Settings.WrapY, false);
    }
    public void IterateBase()
    {
        if (RegionsToSplit.Count > 0)
        {
            current = RegionsToSplit.Pop();

            //Possibly ignore the region.
            if (MathF.R.NextDouble() < Settings.IgnoreRegionChance(current))
            {
                Regions.Add(current);
                //Re-iterate so that this iteration doesn't appear to do nothing.
                IterateBase();
                return;
            }

            //Figure out how to split the region: horizontally or vertically.
            bool horizontal = Settings.ShouldSplitHorizontally(new Location { X = current.Width, Y = current.Height });
            if (current.Width < 2) horizontal = true;
            if (current.Height < 2) horizontal = false;

            //Split.
            if (horizontal)
            {
                int i = current.Top, j = current.Bottom;

                //If there's only one or two possible spots, just pick one.
                if (j - i < 3) SplitHorizontalAt(RegionsToSplit, current, (j + i) / 2);
                //Otherwise use the function, but make sure the outer two lines won't be picked.
                else SplitHorizontalAt(RegionsToSplit, current, Settings.SplitLocation(i + 1, j - 1));
            }
            else
            {
                int i = current.Left, j = current.Right;

                //If there's only one or two possible spots, just pick one.
                if (j - i < 3) SplitVerticalAt(RegionsToSplit, current, (j + i) / 2);
                //Otherwise use the function, but make sure the outer two lines won't be picked.
                else SplitVerticalAt(RegionsToSplit, current, Settings.SplitLocation(i + 1, j - 1));
            }
        }
    }
    public void GenerateBase()
    {
        InitializeBase();

        //Keep going until we run out of regions.
        while (RegionsToSplit.Count > 0)
            IterateBase();
    }
    private void SplitVerticalAt(Stack<Region> regionsToSplit, Region beingSplitted, int splitX)
    {
        //Get the number of holes.
        int holes = Settings.HolesInLine(beingSplitted.Height + 1);
        if (holes < 1) holes = 1;
        if (holes >= beingSplitted.Height + 1)
        {
            Regions.Add(beingSplitted);
            return;
        }

        //First make the line.
        for (int j = beingSplitted.Top; j <= beingSplitted.Bottom; ++j)
            Map[splitX, j] = true;
        //Make sure the line isn't blocking a hole at the edge of the region.
        if (beingSplitted.Top > 0 && (!Map[splitX, beingSplitted.Top - 1] || Holes.Contains(new Location(splitX, beingSplitted.Top - 1))))
        {
            Map[splitX, beingSplitted.Top] = false;
            Holes.Add(new Location(splitX, beingSplitted.Top));
        }
        if (beingSplitted.Bottom < Map.GetLength(1) - 1 && (!Map[splitX, beingSplitted.Bottom + 1] || Holes.Contains(new Location(splitX, beingSplitted.Bottom + 1))))
        {
            Map[splitX, beingSplitted.Bottom] = false;
            Holes.Add(new Location(splitX, beingSplitted.Bottom));
        }

        //Now make holes.

        //Get the possible cells to put a hole in.
        List<int> cellYs = new List<int>(beingSplitted.Height + 1);
        for (int i = 0; i < beingSplitted.Height + 1; ++i)
            cellYs.Add(i + beingSplitted.Top);

        //Randomly pick from them.
        int randomIndex;
        for (int i = 0; i < holes; ++i)
        {
            randomIndex = MathF.R.Next(0, cellYs.Count);

            Map[splitX, cellYs[randomIndex]] = false;
            Holes.Add(new Location(splitX, cellYs[randomIndex]));

            cellYs.RemoveAt(randomIndex);
        }

        //Now split the region into two regions (assuming both regions are large enough). Move the regions around a bit to make sure the line we just made is not part of the regions.
        Region one = new Region(beingSplitted.TopLeft, new Location { X = splitX - 1, Y = beingSplitted.Bottom });
        Region two = new Region(new Location { X = splitX + 1, Y = beingSplitted.Top }, beingSplitted.BottomRight);

        if (one.Width >= 0 && one.Height >= 0 && one.Left >= 0 && one.Right < Map.GetLength(0) && one.Top >= 0 && one.Bottom < Map.GetLength(1)) regionsToSplit.Push(one);
        if (two.Width >= 0 && two.Height >= 0 && two.Left >= 0 && two.Right < Map.GetLength(0) && two.Top >= 0 && two.Bottom < Map.GetLength(1)) regionsToSplit.Push(two);
    }
    private void SplitHorizontalAt(Stack<Region> regionsToSplit, Region beingSplitted, int splitY)
    {
        //Get the number of holes.
        int holes = Settings.HolesInLine(beingSplitted.Width + 1);
        if (holes < 1) holes = 1;
        if (holes >= beingSplitted.Width + 1)
        {
            Regions.Add(beingSplitted);
            return;
        }

        //First make the line.
        for (int i = beingSplitted.Left; i <= beingSplitted.Right; ++i)
            Map[i, splitY] = true;
        //Make sure the line isn't blocking a hole at the edge of the region.
        if (beingSplitted.Left > 0 && (!Map[beingSplitted.Left - 1, splitY] || Holes.Contains(new Location(beingSplitted.Left - 1, splitY))))
        {
            Map[beingSplitted.Left, splitY] = false;
            Holes.Add(new Location(beingSplitted.Left, splitY));
        }
        if (beingSplitted.Right < Map.GetLength(1) - 1 && (!Map[beingSplitted.Right + 1, splitY] || Holes.Contains(new Location(beingSplitted.Right + 1, splitY))))
        {
            Map[beingSplitted.Right, splitY] = false;
            Holes.Add(new Location(beingSplitted.Right, splitY));
        }
        //Now make holes.

        //Get the possible cells to put a hole in.
        List<int> cellXs = new List<int>(beingSplitted.Width + 1);
        for (int i = 0; i < beingSplitted.Width + 1; ++i)
            cellXs.Add(i + beingSplitted.Left);

        //Randomly pick from them.
        int randomIndex;
        for (int i = 0; i < holes; ++i)
        {
            randomIndex = MathF.R.Next(0, cellXs.Count);

            Map[cellXs[randomIndex], splitY] = false;
            Holes.Add(new Location(cellXs[randomIndex], splitY));

            cellXs.RemoveAt(randomIndex);
        }

        //Now split the region into two regions (assuming both regions are large enough). Move the regions around a bit to make sure the line we just made is not part of the regions.
        Region one = new Region(beingSplitted.TopLeft, new Location { X = beingSplitted.Right, Y = splitY - 1 });
        Region two = new Region(new Location { X = beingSplitted.Left, Y = splitY + 1 }, beingSplitted.BottomRight);

        if (one.Width >= 0 && one.Height >= 0 && one.Left >= 0 && one.Right < Map.GetLength(0) && one.Top >= 0 && one.Bottom < Map.GetLength(1)) regionsToSplit.Push(one);
        if (two.Width >= 0 && two.Height >= 0 && two.Left >= 0 && two.Right < Map.GetLength(0) && two.Top >= 0 && two.Bottom < Map.GetLength(1)) regionsToSplit.Push(two);
    }

    public void BeforeFillPatterns()
    {
        TrimTrailingEdges();
        MergeRegions();
        RemoveHolesTouchingRegions();
    }
    private void TrimTrailingEdges()
    {

    }
    private void MergeRegions()
    {

    }
    private void RemoveHolesTouchingRegions()
    {
        foreach (Region r in Regions)
            for (int i = 0; i < Holes.Count; ++i)
                if (r.Touches(Holes[i], true, true, true))
                    Holes.RemoveAt(i--);
    }

    private int iterateFillPatternsRegionCount = 0;
    public void ApplyFillPatterns()
    {
        FilledRegions.Clear();
        while (iterateFillPatternsRegionCount < Regions.Count)
            IterateFillPattern();
        return;
    }
    public void IterateFillPattern()
    {
        //Already done?
        if (iterateFillPatternsRegionCount >= Regions.Count) return;

        //Run an iteration.

        FilledRegion temp;
        Region r = Regions[iterateFillPatternsRegionCount];
        iterateFillPatternsRegionCount++;

        //Pick a random applicable pattern (assuming one exists) and apply it.
        FillPattern p = Settings.MostSuitable(r);
        if (p != null)
        {
            //Clear the space, and apply the pattern.
            FillData.BeingFilled = r;
            FillData.FillRegion(false, r);
            temp = p.Apply(FillData);

            //If the filled region is blank, replace it with a BlankRegion.
            bool blank = true;
            for (int i = temp.Covering.Left; i <= temp.Covering.Right; ++i)
            {
                for (int j = temp.Covering.Top; j <= temp.Covering.Bottom; ++j)
                {
                    if (FillData.GetMapAt(new Location(i, j)))
                    {
                        blank = false;
                        break;
                    }
                }
                if (!blank)
                {
                    break;
                }
            }
            if (blank)
            {
                temp = new BlankRegion(temp.Covering);
            }

            //Now add it.
            FilledRegions.Add(temp);
            Region r2;

            //Make sure the spawn points are all valid.
            foreach (Spawns s in FilledRegions[FilledRegions.Count - 1].PotentialSpawns.Keys)
            {
                for (int i = 0; i < FilledRegions[FilledRegions.Count - 1].PotentialSpawns[s].Count; ++i)
                {
                    r2 = FilledRegions[FilledRegions.Count - 1].PotentialSpawns[s][i];

                    if (r2.Width < 0 || r2.Height < 0)
                        FilledRegions[FilledRegions.Count - 1].PotentialSpawns[s].RemoveAt(i--);
                }
            }
        }
        //If no regions fit, add a "NoRegion".
        else
        {
            FilledRegions.Add(new NoRegion(r));
        }
    }

    public void AfterFillPatterns()
    {

    }

    public void FullGenerate()
    {
        GenerateBase();
        BeforeFillPatterns();
        ApplyFillPatterns();
        AfterFillPatterns();
    }
}
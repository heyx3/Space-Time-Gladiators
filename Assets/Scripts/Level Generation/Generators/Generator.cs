using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
    /// Handles generation of a map.
    /// </summary>
    /// <typeparam name="Settings">The object type that holds the settings this kind of Generator needs to generate a map.</typeparam>
public interface Generator
{
	//TODO: Limiting number of teams/players.
	
	string Description { get; set; }
	
    bool[,] Map { get; set; }
    List<Location> Holes { get; }
    List<Region> Regions { get; }
    List<FilledRegion> FilledRegions { get; }

    FillData FillData { get; }

    GeneratorSettings GenSettings { get; }
    void SetSettings(GeneratorSettings s);

    /// <summary>
    /// Tells this generator to flip the y coordinates of any data
    /// specific to a certain type of generator (e.x. RoguelikeGenerator).
    /// </summary>
    void FlipYsForGenSpecificData();

    /// <summary>
    /// Fully generates a map.
    /// </summary>
    void FullGenerate();

    /// <summary>
    /// Prepares the base map to be generated.
    /// </summary>
    void InitializeBase();
    /// <summary>
    /// Runs the base map generation through one iteration.
    /// </summary>
    void IterateBase();
    /// <summary>
    /// Creates the basic starting generation of the map, before it is modified.
    /// </summary>
    void GenerateBase();
    /// <summary>
    /// Modifications to the map that should be made after the base map is generated but before fill patterns are applied.
    /// </summary>
    void BeforeFillPatterns();
    /// <summary>
    /// Applies fill patterns to the map.
    /// </summary>
    void ApplyFillPatterns();
    /// <summary>
    /// Applies a single fill pattern.
    /// </summary>
    void IterateFillPattern();
    /// <summary>
    /// Modifications to the map that should be done after fill patterns are applied.
    /// </summary>
    void AfterFillPatterns();
}

/// <summary>
    /// The basic settings a map generator will need to generate a map.
    /// </summary>
public abstract class GeneratorSettings
{
    public static Random R = new Random();

    /// <summary>
    /// Reflects the y values of the given generator's map/data.
    /// </summary>
    public static void FlipYs(Generator g)
    {
        int height = g.Map.GetLength(1);

        //Fix the map.
        bool[,] fixedMap = new bool[g.Map.GetLength(0), g.Map.GetLength(1)];
        for (int i = 0; i < fixedMap.GetLength(0); ++i)
        {
            for (int j = 0; j < fixedMap.GetLength(1); ++j)
            {
                fixedMap[i, j] = g.Map[i, fixedMap.GetLength(1) - 1 - j];
            }
        }
        g.Map = fixedMap;
        //Fix the filled regions.
        foreach (FilledRegion fr in g.FilledRegions)
        {
            fr.FlipY(g.Map.GetLength(1));
        }
        //Fix the holes.
        for (int i = 0; i < g.Holes.Count; ++i)
        {
            g.Holes[i] = new Location(g.Holes[i].X,
                                      fixedMap.GetLength(1) - 1 - g.Holes[i].Y);
        }
        //Fix the regions.
        for (int i = 0; i < g.Regions.Count; ++i)
        {
            g.Regions[i] = new Region(g.Regions[i].X,
                                      fixedMap.GetLength(1) - 1 - g.Regions[i].Y,
                                      g.Regions[i].Width, -g.Regions[i].Height, true);
        }

        //Do the same for the fill data.
        g.FillData.Map = fixedMap;
        g.FillData.CurrentRegions = g.Regions;
        g.FillData.Holes = g.Holes;

        //Now flip generator-specific data.
        g.FlipYsForGenSpecificData();
    }

    public abstract bool WrapX { get; set; }
    public abstract bool WrapY { get; set; }

    public List<FillPattern> FillPatterns;

    public GeneratorSettings(params FillPattern[] patterns)
    {
        FillPatterns = patterns.ToList();
    }
    public GeneratorSettings(List<FillPattern> patterns)
    {
        FillPatterns = patterns;
    }

    public FillPattern MostSuitable(Region r)
    {
        //Get the most suitable regions.
        List<FillPattern> suitables = new List<FillPattern>();
        Suitability max = Suitability.Not;
        Suitability temp;

        foreach (FillPattern fp in FillPatterns)
        {
            temp = Suitability.Min(fp.MaxSuitability, fp.GetSuitability(r));

            //If the current pattern is MORE suitable than the current max,
            //   start a new collection with this pattern.
            if (temp.GreaterThan(max))
            {
                suitables.Clear();
                suitables.Add(fp);
                max = temp;
            }
            //If the current pattern has the same suitability as the current max,
            //   just add it to the collection.
            else if (!max.EqualTo(Suitability.Not) && temp.EqualTo(max))
                suitables.Add(fp);
        }

        //Randomly choose from the most suitable patterns.
        if (suitables.Count == 0) return null;
        return suitables[R.Next(0, suitables.Count)];
    }
}
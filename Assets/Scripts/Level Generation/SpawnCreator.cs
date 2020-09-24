using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Generates spawns from a generated level
/// in a way that fairly distributes them as best as possible.
/// </summary>
public class SpawnCreator
{
    //Constants for the scoring algorithm.
    private const float UsedScale = 0.8f;
    private const double DistLogScale = 1.1;
    private const double StandardDeviationPower = 2.0f;

    public Generator LevelGen;

    /// <summary>
    /// The number of times each Location has been used for a spawn.
    /// </summary>
    public Dictionary<Location, int> Uses;

    /// <summary>
    /// The spawns created so far (not including team spawns).
    /// </summary>
    public Dictionary<Spawns, List<Location>> OtherSpawnsCreated;

    /// <summary>
    /// The team spawns created so far (each element is the spawns for a single team).
    /// </summary>
    public List<List<Location>> TeamSpawns;

    /// <summary>
    /// The type of enemy spawned at the given created enemy spawn position.
    /// </summary>
    public Dictionary<Location, Enemies> EnemySpawnType;
    /// <summary>
    /// The type of special wall spawned at the given created special-wall spawn position.
    /// </summary>
    public Dictionary<Location, SpecialWalls> SpecWallSpawnType;

    /// <summary>
    /// The possible range for the number of spawns.
    /// </summary>
    public Interval NumbPowerups, NumbEnemies, NumbWaypoints, NumbSpecialWalls;

    public SpawnCreator(Generator gen, int teams)
    {
        NumbPowerups = new Interval(4, 8, true, 0);
        NumbEnemies = new Interval(2, 5, true, 0);
        NumbSpecialWalls = new Interval(2, 5, true, 0);
        NumbWaypoints = new Interval(5, 10, true, 0);

        Initialize(teams, gen);
    }
    public void Initialize(int teams, Generator gen)
    {
        LevelGen = gen;

        //Initialize the dictionaries.

        Uses = new Dictionary<Location, int>(gen.Map.GetLength(0) * gen.Map.GetLength(1));
        for (int i = 0; i < gen.Map.GetLength(0); ++i)
            for (int j = 0; j < gen.Map.GetLength(1); ++j)
                Uses.Add(new Location(i, j), 0);

        OtherSpawnsCreated = new Dictionary<Spawns, List<Location>>();
        OtherSpawnsCreated.Add(Spawns.Enemy, new List<Location>());
        OtherSpawnsCreated.Add(Spawns.Powerup, new List<Location>());
        OtherSpawnsCreated.Add(Spawns.SpecialWall, new List<Location>());
        OtherSpawnsCreated.Add(Spawns.TeamSpecialWall, new List<Location>());
        OtherSpawnsCreated.Add(Spawns.Waypoint, new List<Location>());

        TeamSpawns = new List<List<Location>>(teams);

        EnemySpawnType = new Dictionary<Location, Enemies>();
        SpecWallSpawnType = new Dictionary<Location, SpecialWalls>();

        done = false;
    }

    /// <summary>
    /// Finds the highest-scoring location in the given collection of locations,
    /// or "null" if the collection has no Locations.
    /// </summary>
    /// <param name="filter">A predicate to filter the locations that will be used from the Region.</param>
    public Location? BestScore(List<KeyValuePair<Location, FilledRegion>> potentialSpawns, Predicate<Location> filter, List<Location> scoreAgainst)
    {
        Location? lowest = null;
        float largest = Single.MinValue;
        float temp;

        foreach (KeyValuePair<Location, FilledRegion> pair in potentialSpawns)
                if (filter(pair.Key))
                {
                    temp = ScoreLocation(pair.Key, pair.Value, scoreAgainst);
                    if (temp > largest)
                    {
                        largest = temp;
                        lowest = pair.Key;
                    }
                }

        return lowest;
    }
    /// <summary>
    /// Scores a location's fairness in terms of its distance to other spawns
    /// (being farther away and equidistant gives a higher score)
    /// and the number of times it has been used already (fewer uses means a higher score).
    /// </summary>
    /// <returns>A "score" for the location that increases as the location is:
    /// 1) Farther away on average from the other locations,
    /// 2) Closer to perfectly equidistant from each other location, and
    /// 3) Used fewer times in the past to spawn something.</returns>
    public float ScoreLocation(Location l, FilledRegion region, List<Location> scoreAgainst)
    {
        if (scoreAgainst.Count == 0) return 100.0f;

        //Get the distance to each base. There are multiple distances to a base because the level might wrap around.
        List<float>[] dists = new List<float>[scoreAgainst.Count];
        for (int i = 0; i < dists.Length; ++i)
            dists[i] = new List<float>();
        float[] minDists = new float[scoreAgainst.Count];

        //Get the distances to each base (including all possible wrap-around distances).
        Location size = new Location(LevelGen.Map.GetLength(0), LevelGen.Map.GetLength(1));
        for (int i = 0; i < scoreAgainst.Count; ++i)
        {
            dists[i].Add(Location.DistanceSquared(l, scoreAgainst[i]));

            if (LevelGen.GenSettings.WrapX)
            {
                dists[i].Add(Location.DistanceSquared(l, new Location(scoreAgainst[i].X - size.X, scoreAgainst[i].Y)));
                dists[i].Add(Location.DistanceSquared(l, new Location(scoreAgainst[i].X + size.X, scoreAgainst[i].Y)));
            }

            if (LevelGen.GenSettings.WrapY)
            {
                dists[i].Add(Location.DistanceSquared(l, new Location(scoreAgainst[i].X, scoreAgainst[i].Y - size.Y)));
                dists[i].Add(Location.DistanceSquared(l, new Location(scoreAgainst[i].X, scoreAgainst[i].Y + size.Y)));
            }

            //Extra diagonal distances.
            if (LevelGen.GenSettings.WrapX && LevelGen.GenSettings.WrapY)
            {
                dists[i].Add(Location.DistanceSquared(l, new Location(scoreAgainst[i].X - size.X, scoreAgainst[i].Y - size.Y)));
                dists[i].Add(Location.DistanceSquared(l, new Location(scoreAgainst[i].X + size.X, scoreAgainst[i].Y - size.Y)));
                dists[i].Add(Location.DistanceSquared(l, new Location(scoreAgainst[i].X - size.X, scoreAgainst[i].Y + size.Y)));
                dists[i].Add(Location.DistanceSquared(l, new Location(scoreAgainst[i].X + size.X, scoreAgainst[i].Y + size.Y)));
            }
        }

        //Get the average of the distances.
        float average = 0.0f, min;
        for (int i = 0; i < dists.Length; ++i)
        {
            //Get the smallest distance to this base.
            min = Single.MaxValue;
            foreach (float f in dists[i])
                min = Math.Min(f, min);
            
            //Use the logarithm of the distance, because the difference
            //   between two bases' distances from given "l" is more
            //   problematic if the bases are both closer to "l".
            minDists[i] = (float)Math.Log(Math.Sqrt(min) + 1.0f, DistLogScale);
            average += minDists[i];
        }
        average /= (float)dists.Length;

        //Get the standard deviation of the distances.
        float std = 0.0f, temp;
        float[] tempA = new float[dists.Length];
        for (int i = 0; i < dists.Length; ++i)
        {
            temp = minDists[i] - average;
            std += temp * temp;
        }
        std /= (float)dists.Length;
        std = (float)Math.Sqrt(std);

        //Combine the average, the standard deviation, and the number of times this location has been used before.
        //Raise the standard deviation to a power, and use a multiplier for the number of uses.
        try
        {
            return (average - (float)Math.Pow(std, StandardDeviationPower)) * (float)Math.Pow(UsedScale, Uses[l]);
        }
        catch (Exception e)
        {
            throw new ArgumentOutOfRangeException("The spawn for a " + region.ToString() + " region is outside the map!");
        }
    }

    #region Spawn Functions

    private bool done = false;
    /// <summary>
    /// Generates all spawns.
    /// </summary>
    /// <returns>True if spawn creation occurred succesfully, false if there weren't enough spawns available.</returns>
    public bool GenerateSpawns()
    {
        if (done)
            throw new InvalidOperationException("Must call Initialize() before generating spawns again!");

        if (!GeneratePlayers())
            return false;
        if (!GeneratePowerups())
            return false;
        if (!GenerateEnemies())
            return false;
        if (!GenerateTeamSpecialWalls())
            return false;
        if (!GenerateSpecialWalls())
            return false;
        if (!GenerateWaypoints())
            return false;

        done = true;

        return true;
    }

    private bool GeneratePlayers()
    {
        //Index possible spawn regions by their centers.
        Dictionary<Location, KeyValuePair<FilledRegion, Region>> centerToRegion = new Dictionary<Location, KeyValuePair<FilledRegion, Region>>();
        List<Region> usedSpawns = new List<Region>();
        
        //Get all possible spawn regions.
        foreach (FilledRegion fr in LevelGen.FilledRegions)
            foreach (Region r in fr.PotentialSpawns[Spawns.Team])
                centerToRegion.Add(r.Center, new KeyValuePair<FilledRegion, Region>(fr, r));

        //Go through each team in the match and make a spawn for them.
        Region teamSpawnTemp;
        List<Location> tempSpawn;
        int teams = TeamSpawns.Capacity;
        List<KeyValuePair<Location, FilledRegion>> tempCenters;
        for (int i = 0; i < teams; ++i)
        {
            //Get the highest-scoring region that hasn't been used already.

            tempCenters = new List<KeyValuePair<Location, FilledRegion>>();
            foreach (KeyValuePair<Location, KeyValuePair<FilledRegion, Region>> pair in centerToRegion)
            {
                tempCenters.Add(new KeyValuePair<Location, FilledRegion>(pair.Key, pair.Value.Key));
            }

            Location? best = BestScore(tempCenters,//centerToRegion.Keys.ToList(),
                                       l => !usedSpawns.ConvertAll(r => r.Center).Contains(l),
                                       TeamSpawns.ConvertAll(ls => Location.Average(ls)));

            if (!best.HasValue) return false;

            //Use it as a spawn.
            teamSpawnTemp = centerToRegion[best.Value].Value;
            usedSpawns.Add(teamSpawnTemp);
            tempSpawn = new List<Location>();
            TeamSpawns.Add(tempSpawn);
            for (int x = teamSpawnTemp.Left; x <= teamSpawnTemp.Right; ++x)
			{
                for (int y = teamSpawnTemp.Top; y <= teamSpawnTemp.Bottom; ++y)
                {
                    tempSpawn.Add(new Location(x, y));
					try
					{
                    	Uses[new Location(x, y)]++;
					}
					catch (Exception e)
					{
						e = e;
					}
                }
			}
        }

        return true;
    }
    private bool GeneratePowerups()
    {
        int numb = (int)NumbPowerups.Random();

        //Choose spawn regions randomly.

        //First, build the list of all powerup spawns that don't touch team spawns.
        List<Region> chooseFrom = new List<Region>();
        foreach (FilledRegion fr in LevelGen.FilledRegions)
            foreach (Region r in fr.PotentialSpawns[Spawns.Powerup])
                if (TeamSpawns.All(ls =>
                {
                    for (int i = 0; i < ls.Count; ++i)
                        if (r.Touches(ls[i], true, true, true))
                            return false;
                    return true;
                }))
                    chooseFrom.Add(r);

        //Make sure there are enough spawn places for powerups.
        int spawnPlaces = 0;
        foreach (Region r in chooseFrom)
            spawnPlaces += (r.Width + 1) * (r.Height + 1);
        if (spawnPlaces < numb) return false;

        //Now choose randomly from these regions and choose a random position from that region.
        Region usingR;
        int index;
        for (int i = 0; i < numb; ++i)
        {
            index = MathF.R.Next(0, chooseFrom.Count);
            usingR = chooseFrom[index];
            if (usingR.Width < 0 || usingR.Height < 0)
            {
                --i;
                continue;
            }

            //Get an unused spawn location.
            Location tempL = new Location(MathF.R.Next(usingR.Left, usingR.Right + 1),
                                          MathF.R.Next(usingR.Top, usingR.Bottom + 1));
            while (OtherSpawnsCreated[Spawns.Powerup].Contains(tempL))
            {
                index = MathF.R.Next(0, chooseFrom.Count);
                usingR = chooseFrom[index];
                if (usingR.Width < 0 || usingR.Height < 0)
                {
                    --i;
                    continue;
                }

                tempL = new Location(MathF.R.Next(usingR.Left, usingR.Right + 1),
                                     MathF.R.Next(usingR.Top, usingR.Bottom + 1));
            }

            OtherSpawnsCreated[Spawns.Powerup].Add(tempL);
        }

        return true;
    }
    private bool GenerateEnemies()
    {
        int enemies = (int)NumbEnemies.Random();

        return true;
    }
    private int numbSpecialWalls, generatedSpecialWalls;
    private bool GenerateTeamSpecialWalls()
    {
        generatedSpecialWalls = 0;
        numbSpecialWalls = (int)NumbSpecialWalls.Random();

        return true;
    }
    private bool GenerateSpecialWalls()
    {
        int numb = numbSpecialWalls - generatedSpecialWalls;
        if (numb <= 0) return true;

        return true;
    }
    private bool GenerateWaypoints()
    {
        int numb = (int)NumbWaypoints.Random();

        //TODO: Rewrite the spawning algorithm for waypoints so that a counter steps through a graph representing the level and carves a "path" for the race.

        //For now just pick them randomly.

        //First, build the list of all waypoint spawns that don't touch team spawns.
        List<Region> chooseFrom = new List<Region>();
        foreach (FilledRegion fr in LevelGen.FilledRegions)
            foreach (Region r in fr.PotentialSpawns[Spawns.Waypoint])
                if (TeamSpawns.All(ls =>
                    {
                        for (int i = 0; i < ls.Count; ++i)
                            if (r.Touches(ls[i], true, true, true))
                                return false;
                        return true;
                    }))
                    chooseFrom.Add(r);

        //Make sure there are enough spawn places for waypoints.
        int spawnPlaces = 0;
        foreach (Region r in chooseFrom)
            spawnPlaces += (r.Width + 1) * (r.Height + 1);
        if (spawnPlaces < numb) return false;

        //Now choose randomly from these regions and choose a random position from that region.
        Region usingR;
        int index;
        for (int i = 0; i < numb; ++i)
        {
            index = MathF.R.Next(0, chooseFrom.Count);
            usingR = chooseFrom[index];
            if (usingR.Width < 0 || usingR.Height < 0)
            {
                --i;
                continue;
            }

            //Get an unused spawn location.
            Location tempL = new Location(MathF.R.Next(usingR.Left, usingR.Right + 1),
                                          MathF.R.Next(usingR.Top, usingR.Bottom + 1));
            while (OtherSpawnsCreated[Spawns.Waypoint].Contains(tempL))
            {
                index = MathF.R.Next(0, chooseFrom.Count);
                usingR = chooseFrom[index];
                if (usingR.Width < 0 || usingR.Height < 0)
                {
                    --i;
                    continue;
                }

                tempL = new Location(MathF.R.Next(usingR.Left, usingR.Right + 1),
                                     MathF.R.Next(usingR.Top, usingR.Bottom + 1));
            }

            OtherSpawnsCreated[Spawns.Waypoint].Add(tempL);
        }

        return true;
    }

    #endregion
}

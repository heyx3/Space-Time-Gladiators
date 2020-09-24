using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a region of space that was filled with a specific fill pattern.
/// Provides functionality for navigating through the region
/// and for finding possible spawn points for players/collectibles/bases/etc.
/// </summary>
public abstract class FilledRegion
{
    public Region Covering { get; protected set; }

    /// <summary>
    /// A collection of all the different possible spawn regions for different game elements
    /// inside the Region covered by this FilledRegion. Indexed by the type of element.
    /// </summary>
    public Dictionary<Spawns, List<Region>> PotentialSpawns;

    /// <summary>
    /// Given a special wall spawn region from "PotentialSpawns", gives the different kinds of special walls applicable to that spawn area.
    /// </summary>
    public Dictionary<Region, List<SpecialWalls>> ApplicableWallsToSpawn;
    /// <summary>
    /// Given an enemy spawn region from "PotentialSpawns", gives the different kinds of enemies applicable to that spawn area.
    /// </summary>
    public Dictionary<Region, List<Enemies>> ApplicableEnemiesToSpawn;

    public FilledRegion(Region covering)
    {
        Covering = covering;

        PotentialSpawns = new Dictionary<Spawns, List<Region>>()
        {
            { Spawns.Team, new List<Region>() },
            { Spawns.Enemy, new List<Region>() },
            { Spawns.Powerup, new List<Region>() },
            { Spawns.SpecialWall, new List<Region>() },
            { Spawns.TeamSpecialWall, new List<Region>() },
            { Spawns.Waypoint, new List<Region>() },
        };

        ApplicableEnemiesToSpawn = new Dictionary<Region, List<Enemies>>();
        ApplicableWallsToSpawn = new Dictionary<Region, List<SpecialWalls>>();
    }

    /// <summary>
    /// Given the level height (the number of spaces in the level map),
    /// flips the y value of all data related to this FilledRegion.
    /// </summary>
    public virtual void FlipY(int height)
    {
        //Flip the region.
        Covering = new Region(Covering.X, FlipY(height, Covering.Y), Covering.Width, -Covering.Height, true);

        //Flip the spawns.
        List<Region> temp;
		List<Spawns> keys1 = PotentialSpawns.Keys.ToList();
        foreach (Spawns s in keys1)
        {
            temp = new List<Region>(PotentialSpawns.Count);
            foreach (Region r in PotentialSpawns[s])
            {
                temp.Add(new Region(r.X, FlipY(height, r.Y), r.Width, -r.Height, true));
            }
            PotentialSpawns[s] = temp;
        }

        //Flip the special wall spawns.
        Dictionary<Region, List<SpecialWalls>> newAppWallToSpawn = new Dictionary<Region,List<SpecialWalls>>();
        List<Region> keys = ApplicableWallsToSpawn.Keys.ToList();
        foreach (Region r in keys)
        {
            newAppWallToSpawn.Add(new Region(r.X, FlipY(height, r.Y), r.Width, -r.Height, true), ApplicableWallsToSpawn[r]);
        }
        ApplicableWallsToSpawn = newAppWallToSpawn;

        //Flip the enemy spawns.
        Dictionary<Region, List<Enemies>> newAppEnemiesToSpawn = new Dictionary<Region, List<Enemies>>();
        keys = ApplicableEnemiesToSpawn.Keys.ToList();
        foreach (Region r in keys)
        {
            newAppEnemiesToSpawn.Add(new Region(r.X, FlipY(height, r.Y), r.Width, -r.Height, true), ApplicableEnemiesToSpawn[r]);
        }
        ApplicableEnemiesToSpawn = newAppEnemiesToSpawn;
    }
    protected int FlipY(int height, int y)
    {
        return height - 1 - y;
    }

    public abstract override string ToString();
}

/// <summary>
/// The different kinds of basic spawns.
/// </summary>
public enum Spawns
{
    Team,
    Powerup,
    Waypoint,
    Enemy,
    TeamSpecialWall,
    SpecialWall,
}

public enum SpecialWalls
{
    //TODO: Fill, then implement in FilledRegion's.
}
public enum Enemies
{
    //TODO: Fill then implement in FilledRegion's.
}

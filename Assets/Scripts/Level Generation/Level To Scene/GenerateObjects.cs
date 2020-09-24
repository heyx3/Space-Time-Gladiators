using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using Consts = WorldConstants;

public class GenerateObjects : MonoBehaviour
{
    //TODO: Create remote (online) players, make a PrefabCreator method for them, and put code here to account for them.
    
    public List<Vector2> PowerupSpawnLocations;
	
    public static Vector2 ToWorldPos(Location levelMapPos, MatchStartData matchData)
    {
        return new Vector2(new Interval(0.0f, matchData.GeneratedLevel.Map.GetLength(0), true, 3).Wrap(levelMapPos.X),
                           new Interval(0.0f, matchData.GeneratedLevel.Map.GetLength(1), true, 3).Wrap(levelMapPos.Y));
    }

    /// <summary>
    /// Generates all game objects (players, objectives/collectibles, etc).
    /// </summary>
    /// <param name="isPreview">If true, only creates objects useful for a preview of the world.</param>
    public void Generate(MatchStartData matchData, bool isPreview)
    {
		Generator level = matchData.GeneratedLevel;
		SpawnCreator spawns = matchData.Spawns;
		Dictionary<Color, List<byte>> playersOnTeams = matchData.PlayersOnTeams;
		Dictionary<byte, byte> localPlayersControls = matchData.PlayerControlSchemes;

        List<Camera> playerCameras = new List<Camera>();

        PrefabCreator prefCreator = WorldConstants.Creator;
        LevelManager levelManager = WorldConstants.MatchController.GetComponent<LevelManager>();

        #region Create players

        //Get the number of players on this machine.
        int localPlayers = 0;
        foreach (List<byte> bs in playersOnTeams.Values)
        {
            foreach (byte id in bs)
            {
                if (id <= 4)
                {
                    localPlayers++;
                }
            }
        }

        //Make sure there are a valid number of players in-game and on this machine.
        if (playersOnTeams.Keys.Count < 1 || playersOnTeams.Keys.Count > 8 ||
            localPlayersControls.Count < 1 || localPlayersControls.Count > 4)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        //Spawn teams.
        int spawnIndex = 0;
        List<Location> spawnAreas;
        Dictionary<Color, List<Location>> spawnsByTeam = new Dictionary<Color, List<Location>>();
        GameObject p;
        Location spawnLoc;
        foreach (KeyValuePair<Color, List<byte>> team in playersOnTeams)
        {
            spawnAreas = spawns.TeamSpawns[spawnIndex++];
            spawnsByTeam.Add(team.Key, spawnAreas);

            int count = 0;
            foreach (byte id in team.Value)
            {
                spawnLoc = spawnAreas[count];
                bool cantSpawn = false;
                while (level.Map[spawnLoc.X, spawnLoc.Y])
                {
                    if (count >= spawnAreas.Count - 1)
                    {
                        cantSpawn = true;
                        break;
                    }
                    spawnLoc = spawnAreas[++count];
                }

                if (cantSpawn)
                {
                    throw new UnityException();
                }
                else if (!isPreview)
                {
                    p = prefCreator.CreatePlayer(ToWorldPos(spawnAreas[count++], matchData), id, localPlayersControls[id], team.Key);
                    playerCameras.Add(prefCreator.CreatePlayerCamera(p, localPlayers).camera);
                }
            }
        }

        #endregion
		
		#region Set up view data

        prefCreator.CreateMinimapCamera(new Location(Mathf.RoundToInt(WorldConstants.Size.x), Mathf.RoundToInt(WorldConstants.Size.y)), localPlayers);

		//Calculate the bounds covering all areas a camera could ever see.

		int numbCams = playerCameras.Count;
		Vector2[] viewExtentsInWorld = new Vector2[numbCams];
		Camera ca;
		
		for (int i = 0; i < numbCams; ++i)
        {
			ca = playerCameras[i].camera;
			
			viewExtentsInWorld[i] = ca.ViewportToWorldPoint (new Vector3(1.0f, 1.0f, 0.0f)) - ca.ViewportToWorldPoint (Vector3.zero);
			viewExtentsInWorld[i] *= 0.5f;
		}
		
		//Get the largest view sizes.
		Vector2 maxViewExtentsInWorld;
		float maxX = System.Single.MinValue,
			  maxY = System.Single.MinValue;
		for (int i = 0; i < viewExtentsInWorld.Length; ++i)
		{
            if (viewExtentsInWorld[i].x > maxX)
            {
                maxX = viewExtentsInWorld[i].x;
            }
            if (viewExtentsInWorld[i].y > maxY)
            {
                maxY = viewExtentsInWorld[i].y;
            }
		}
		maxViewExtentsInWorld = new Vector2(maxX, maxY);
        //Give the view rect a bit of leeway.
        maxViewExtentsInWorld += Vector2.one;
		
		//Get a boundary representing all the areas a player could ever view.
		WorldConstants.MaxViewBounds = new RecBounds(WorldConstants.LevelBounds.center,
										 		     WorldConstants.LevelBounds.size);
        WorldConstants.MaxViewBounds.size += maxViewExtentsInWorld * 2.0f;
		
		#endregion
		
		#region Create parallax objects

        if (!isPreview)
        {
			float starsChance = WorldConstants.StarsChance;
			float specialsChance = WorldConstants.SpecialsChance;
			
			if (level.GenSettings.WrapX && level.GenSettings.WrapY)
			{
				starsChance /= 8;
			}
			else if (level.GenSettings.WrapX || level.GenSettings.WrapY)
			{
				starsChance /= 3;
			}

            starsChance /= localPlayers;

            for (int i = 0; i < level.Map.GetLength(0); ++i)
            {
                for (int j = 0; j < level.Map.GetLength(1); ++j)
                {
                    if (!level.Map[i, j])
                    {
                        if (Random.value <= starsChance)
                        {
                            prefCreator.CreateStars(new Vector2(i, j));
                        }
                        else if (Random.value <= specialsChance)
                        {
                            prefCreator.CreateSpecial(new Vector2(i, j));
                        }
                    }
                }
            }
        }
		
		#endregion
		
        #region Create flags/teams

        if (isPreview || levelManager.MatchRules.CTF != null)
		{
            foreach (Color c in playersOnTeams.Keys)
            {
                //Get a spot that has a floor underneath it.

                bool foundFloor = false;

                foreach (Location l in spawnsByTeam[c])
                {
                    //If a spot was found, put the flag there.
                    //Using "above" instead of "below" is intentional --
                    //   much of this framework was tested in XNA, which uses a flipped Y axis.
                    if (level.FillData.GetMapAt(l.Above) && !level.FillData.GetMapAt(l))
                    {
                        if (isPreview)
                        {
                            prefCreator.CreateMinimapIcon(ToWorldPos(l, matchData), Animations.MM_Team, c, prefCreator.MinimapIconScaling.Team, prefCreator.MinimapIconZPoses.Team);
                        }
                        else
                        {
                            prefCreator.CreateFlagAndFlagBase(ToWorldPos(l, matchData), c);
                        }
                        foundFloor = true;
                        break;
                    }
                }

                //If no spawn location has a floor under it, just put the flag in the air.
                if (!foundFloor)
				{
                    foreach (Location l in spawnsByTeam[c])
                    {
                        //If a spot is empty, put the flag there.
                        if (!level.FillData.GetMapAt(l))
                        {
                            if (isPreview)
                            {
                                prefCreator.CreateMinimapIcon(ToWorldPos(l, matchData), Animations.MM_Team, c, prefCreator.MinimapIconScaling.Team, prefCreator.MinimapIconZPoses.Team);
                            }
                            else
                            {
                                prefCreator.CreateFlagAndFlagBase(ToWorldPos(l, matchData), c);
                            }
                            break;
                        }
                    }
				}
            }
		}

        #endregion

		#region Create a waypoint

        //Get areas inside a wall.
        List<int> toRemove = new List<int>();
        Location tempL;
        for (int i = 0; i < spawns.OtherSpawnsCreated[Spawns.Waypoint].Count; ++i)
        {
            tempL = spawns.OtherSpawnsCreated[Spawns.Waypoint][i];
            if (level.Map[tempL.X, tempL.Y])
            {
                Debug.Log("Found a spawn spot inside a wall!");
                toRemove.Add(i);
            }
        }

        //Remove areas that were inside the wall.
        //Start from the end so there's no issue with indices changing after an object is deleted.
        for (int i = toRemove.Count - 1; i >= 0; --i)
        {
            spawns.OtherSpawnsCreated[Spawns.Waypoint].RemoveAt(toRemove[i]);
        }

        //Generate a waypoint.
		if (!isPreview && levelManager.MatchRules.WaypointFight != null)
		{
			List<Location> waypointSpawns = spawns.OtherSpawnsCreated[Spawns.Waypoint];
			Location l = waypointSpawns[Random.Range (0, waypointSpawns.Count - 1)];
			prefCreator.CreateWaypoint(ToWorldPos(l, matchData));
		}
		
		#endregion
		
        #region Create powerup spawns

        if (!isPreview)
        {
            PowerupSpawnLocations = spawns.OtherSpawnsCreated[Spawns.Powerup].ToList().ConvertAll<Vector2>(l => ToWorldPos(l, matchData));
        }

        #endregion
    }
}
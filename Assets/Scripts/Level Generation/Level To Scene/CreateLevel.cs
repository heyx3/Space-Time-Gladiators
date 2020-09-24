using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Consts = WorldConstants;

/// <summary>
/// Creates wall GameObjects in the world given a boolean map of them.
/// </summary>
public class CreateLevel : MonoBehaviour
{
    public PrefabCreator Creator { get { return WorldConstants.Creator; } }
    public CollisionTracker Tracker { get { return WorldConstants.ColTracker; } }

    public Generator LevelGen { get; private set; }
    public void SetGenerationData(Generator generatedLevel, SpawnCreator generatedSpawns)
    {
        LevelGen = generatedLevel;
		SpawnGenerator = generatedSpawns;
    }

    public SpawnCreator SpawnGenerator;

	public Location GridSize;

    private List<WallSheetData> wallsToAnimate = new List<WallSheetData>();
    void Update()
    {
        if (wallsToAnimate.Count == 0)
        {
            return;
        }

        foreach (WallSheetData wsd in wallsToAnimate)
        {
			if (wsd == null)
			{
				int x = 4;
			}
			else
			{
            	wsd.SetAnimation();
			}
        }

        wallsToAnimate.Clear();
    }

    /// <summary>
    /// Generates the level's walls and collision lines.
    /// </summary>
    /// <param name="isPreview">If true, then collision will not be calculated.</param>
	public void Generate(Vector2 wallOffset, bool isPreview)
    {
        Vector2 worldSize = new Vector2(LevelGen.Map.GetLength(0), LevelGen.Map.GetLength(1));
        WorldConstants.Size = worldSize;

		CreateWalls walls = MakeWalls (wallOffset, LevelGen.Map, LevelGen.GenSettings.WrapX, LevelGen.GenSettings.WrapY);

        //Get the wall container.
        GameObject wallsContainer = WorldConstants.WallContainer;
		wallsToAnimate = new List<WallSheetData>(); 

        //Create the wall objects.
        Creator.ChooseWallStyle();
        if (!isPreview)
        {
            foreach (GameObject tempG in Creator.CreateWalls(LevelGen))
            {
                wallsToAnimate.Add(tempG.GetComponent<WallSheetData>());
            }
        }

        //Create the wall minimaps and collision.
        foreach (RecBounds b in walls.WallBounds)
        {
            WorldConstants.ColTracker.AddWall(b);

            Creator.CreateMinimapWall(b);
        }

        //Set up the mirrored wall collision and (if necessary) collision lines.
        if (!isPreview)
        {
            foreach (RecBounds b in walls.MirroredWallBounds)
            {
                Tracker.AddWall(b);
            }
            Tracker.Lines = walls.Lines;
        }
	}

	/// <summary>
	/// Creates basic wall bounds from the given grid of walls.
	/// </summary>
    private CreateWalls MakeWalls(Vector2 offset, bool[,] map, bool wrapHorizontal, bool wrapVertical)
    {
        //First create the wall bounds.
        List<RecBounds> walls = new List<RecBounds>();
        //Go through and add in all the walls from the map.
        for (int i = 0; i < map.GetLength(0); ++i)
        {
            for (int j = 0; j < map.GetLength(1); ++j)
            {
                if (map[i, j])
                {
                    walls.Add(new RecBounds(new Vector3(i + offset.x, j + offset.y, 0.0f),
                                            new Vector3(1.0f, 1.0f, 0.0f)));
                }
            }
        }

        CreateWalls cw = new CreateWalls(LevelGen.GenSettings.WrapX, LevelGen.GenSettings.WrapY);
        foreach (RecBounds b in walls)
            cw.AddWall(b);
        cw.FinalizeWalls(LevelGen.Map);

        return cw;
    }
}
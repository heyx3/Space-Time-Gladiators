using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Creates collision bounds and Lines for walls given a generated level map.
/// </summary>
public class CreateWalls
{
    private RecBounds levelBounds
    { 
		get
        {
			return WorldConstants.LevelBounds;
		}
	}

    /// <summary>
    /// All walls inside the level bounds.
    /// </summary>
    public List<RecBounds> WallBounds { get; private set; }
    /// <summary>
    /// Wrapped-around duplicates of walls that are just outside the level bounds that game elements might collide with.
    /// </summary>
    public List<RecBounds> MirroredWallBounds { get; private set; }
    /// <summary>
    /// The collection of all lines representing the entirety of a side/floor/ceiling,
    /// indexed by any of the wall bounds from a wall making up part of that line.
    /// </summary>
    public WallToLines Lines { get; private set; }

    public bool WrapX { get; private set; }
    public bool WrapY { get; private set; }

    public enum InitializeWallsState
    {
        TakingWalls,
        CombiningWalls,
        Done,
    }
    public InitializeWallsState currentWallsState;
    
    public CreateWalls(bool wrapX, bool wrapY)
    {
        Initialize(wrapX, wrapY);
    }

    /// <summary>
    /// Prepares this object to take in wall bounds.
    /// </summary>
    public void Initialize(bool wrapX, bool wrapY)
    {
        WrapX = wrapX;
        WrapY = wrapY;

        currentWallsState = InitializeWallsState.TakingWalls;

        WallBounds = new List<RecBounds>();
        MirroredWallBounds = new List<RecBounds>();
    }

    /// <summary>
    /// Adds the given wall boundaries to the collection of walls to create.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this object is no longer taking walls to create.</exception>
    public void AddWall(RecBounds b)
    {
        if (currentWallsState != InitializeWallsState.TakingWalls)
            throw new System.InvalidOperationException("This object is no longer taking walls!");

        WallBounds.Add(b);
		
        //If the wall is close enough to the edge and the level wraps around horizontally, duplicate it.
        if (WrapX)
        {
            float left = b.center.x - b.extents.x,
                  right = b.center.x + b.extents.x;
			
            //If the wall is near the left edge, put a wall on the right.
            if (Mathf.Abs(left - (levelBounds.center.x - levelBounds.extents.x)) <= 1.0f)
            {
                MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(WorldConstants.Size.x, 0.0f), b.size));
                //If the level also wraps vertically, put the wall above and below.
                if (WrapY)
                {
                    MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(WorldConstants.Size.x, -WorldConstants.Size.y), b.size));
                    MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(WorldConstants.Size.x, WorldConstants.Size.y), b.size));
                }
            }
            //If the wall is near the right edge, put a wall on the left.
            if (Mathf.Abs(right - (levelBounds.center.x + levelBounds.extents.x)) <= 1.0f)
            {
                MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(-WorldConstants.Size.x, 0.0f), b.size));
                //If the level also wraps vertically, put the wall above and below.
                if (WrapY)
                {
                    MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(-WorldConstants.Size.x, -WorldConstants.Size.y), b.size));
                    MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(-WorldConstants.Size.x, WorldConstants.Size.y), b.size));
                }
            }
        }
        //Do the same for vertical wrapping.
        if (WrapY)
        {
            float bottom = b.center.y - b.extents.y,
                  top = b.center.y + b.extents.y;
            //If the wall is near the bottom edge, put a wall on the top.
            if (Mathf.Abs(bottom - (levelBounds.center.y - levelBounds.extents.y)) <= 1.0f)
            {
                MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(0.0f, WorldConstants.Size.y), b.size));
                //If the level also wraps horizontally, put the wall to the left and right.
                if (WrapX)
                {
                    MirroredWallBounds.Add(new RecBounds(b.center - new Vector2(-WorldConstants.Size.x, WorldConstants.Size.y), b.size));
                    MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(WorldConstants.Size.x, WorldConstants.Size.y), b.size));
                }
            }
            //If the wall is near the top edge, put a wall on the bottom.
            if (Mathf.Abs(top - (levelBounds.center.y + levelBounds.extents.y)) <= 1.0f)
            {
                MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(0.0f, -WorldConstants.Size.y), b.size));
                //If the level also wraps horizontally, put the wall to the left and right.
                if (WrapX)
                {
                    MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(WorldConstants.Size.x, -WorldConstants.Size.y), b.size));
                    MirroredWallBounds.Add(new RecBounds(b.center + new Vector2(-WorldConstants.Size.x, -WorldConstants.Size.y), b.size));
                }
            }
        }
    }

    /// <summary>
    /// Should be called when no more walls will be added.
    /// Combines all the walls and then creates them.
    /// </summary>
    public void FinalizeWalls(bool[,] levelMap)
    {
        if (currentWallsState != InitializeWallsState.TakingWalls)
        {
            throw new InvalidOperationException("Invalid call!");
        }

        currentWallsState = InitializeWallsState.CombiningWalls;

        CombineWalls();
        CalculateLines(levelMap);
		
        currentWallsState = InitializeWallsState.Done;
    }

    /// <summary>
    /// Combines WallBounds and MirroredWallBounds.
    /// </summary>
    private void CombineWalls()
    {
        CombineBoundsList(WallBounds);
        CombineBoundsList(MirroredWallBounds);
    }
    /// <summary>
    /// Combines the given list of bounds so that any adjacent bounds that can be combined ARE combined.
    /// </summary>
    private void CombineBoundsList(List<RecBounds> bs)
    {
        //Go through every pair of bounds and try to combine them.
        float left1, left2,
              right1, right2,
              top1, top2,
              bottom1, bottom2,
              left, right, top, bottom;

        for (int i = 0; i < bs.Count; ++i)
        {
            //Get the bound edges.
            left1 = bs[i].center.x - bs[i].extents.x;
            right1 = bs[i].center.x + bs[i].extents.x;
            bottom1 = bs[i].center.y - bs[i].extents.y;
            top1 = bs[i].center.y + bs[i].extents.y;

            for (int j = i + 1; j < bs.Count; ++j)
            {
                //Get the bound edges.
                left2 = bs[j].center.x - bs[j].extents.x;
                right2 = bs[j].center.x + bs[j].extents.x;
                bottom2 = bs[j].center.y - bs[j].extents.y;
                top2 = bs[j].center.y + bs[j].extents.y;

                /* The bounds are adjacent if either:
                 * 1) They share the same top/bottom and the left side
                 *      of one equals the right side of the other
                 * 2) They share the same left/right and the top side of
                 *      one equals the bottom side of the other */
                if ((left1 == left2 && right1 == right2 &&
                     (top1 == bottom2 || top2 == bottom1)) ||
                    (top1 == top2 && bottom1 == bottom2 &&
                     (left1 == right2 || left2 == right1)))
                {
                    //Get the bounds covering both bounds.
                    left = Mathf.Min(left1, left2);
                    right = Mathf.Max(right1, right2);
                    bottom = Mathf.Min(bottom1, bottom2);
                    top = Mathf.Max(top1, top2);

                    //Remove the two adjacent bounds.
                    bs.RemoveAt(j);
                    bs.RemoveAt(i);

                    //Add the new bounds.
                    bs.Add(new RecBounds(new Vector3(left + ((right - left) * 0.5f),
                                                  bottom + ((top - bottom) * 0.5f),
                                                  0.0f),
                                      new Vector3(right - left, top - bottom, 0.0f)));

                    //Back up the counter and continue the search.
                    --i;
                    break;
                }
            }
        }
    }
	
    /// <summary>
    /// Calculates all Lines making up the given level and puts them into the WallLines dictionary.
    /// </summary>
    private void CalculateLines(bool[,] levelMap)
    {
        Lines = new WallToLines();

        Generator g = WorldConstants.MatchWrapper.GetComponent<CreateLevel>().LevelGen;
        int max = Math.Max(levelMap.GetLength(0), levelMap.GetLength(1));

        //Create the lines in each row and column.
        for (int n = 0; n < max; ++n)
        {
            if (n < levelMap.GetLength(0))
                Line.GetVerticalLines(g.FillData, Lines, n);
            if (n < levelMap.GetLength(1))
                Line.GetHorizontalLines(g.FillData, Lines, n);
        }
		
        //Lengthen each end by half a wall size.
		foreach (Line l in Lines.GetAllLines ()) {
			l.LineRange.Range += 1.0f;
		}
		
		//Combine any duplicated lines.
		Lines.CombineReferences();
    }
}
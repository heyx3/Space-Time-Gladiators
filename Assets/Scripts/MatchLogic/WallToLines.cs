using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Encapsulates the ability to store collision lines indexed by
/// any walls that are part of the line and by the line's direction (ceiling, floor, etc).
/// Essentially, this is a Dictionary with two keys (bounds of a wall connected to the Line, and the wall side that the Line is a part of),
/// and one value (the Line).
/// NOTE: Any lookups that don't have a corresponding Line will return "null", NOT throw an exception.
/// </summary>
public class WallToLines
{
    private Dictionary<RecBounds, Dictionary<ColType, Line>> references;

    public WallToLines()
    {
        references = new Dictionary<RecBounds, Dictionary<ColType, Line>>();
    }

    /// <summary>
    /// Finds if the given wall exists in this system.
    /// </summary>
    public bool WallExists(RecBounds wall)
    {
        return references.ContainsKey(wall);
    }
    /// <summary>
    /// Finds if the given side of the given wall exists in this system.
    /// </summary>
    public bool SideExists(RecBounds wall, ColType side)
    {
        return references.ContainsKey(wall) &&
               references[wall].ContainsKey(side);
    }
    /// <summary>
    /// Gets the line that is in part made up of the given wall on the given side.
    /// </summary>
    public Line GetLine(RecBounds wall, ColType side)
    {
        if (references.ContainsKey(wall))
            if (references[wall].ContainsKey(side))
                return references[wall][side];
            else throw new ArgumentException("No collision on the " + CollisionManager.ToString (side) + " side of the wall (" + wall.ToString () + ") exists as a key!");
        else throw new ArgumentException("The given wall: " + wall.ToString () + " does not exist as a key!");
    }

    /// <summary>
    /// Associates the given wall's given side with the given line.
    /// If another Line was already given that association, the original association will be replaced with this one.
    /// </summary>
    /// <param name="wall">The wall.</param>
    /// <param name="side">The side of the wall that is part of the given line.</param>
    /// <param name="line">The line that is in part made up of the given side of the given wall.</param>
    public void AddReference(RecBounds wall, ColType side, Line line)
    {
        if (!references.ContainsKey(wall))
            references.Add(wall, new Dictionary<ColType, Line>());

        if (!references[wall].ContainsKey(side))
            references[wall].Add(side, line);
        else references[wall][side] = line;
    }
    /// <summary>
    /// Makes the given associations between the walls, a side, and a Line.
    /// </summary>
    /// <param name="walls">A collection of walls that all share a common Line on one edge.</param>
    /// <param name="side">The edge the walls share a Line on.</param>
    /// <param name="line">The Line that all the walls share.</param>
    public void AddReferences(IEnumerable<RecBounds> walls, ColType side, Line line)
    {
        foreach (RecBounds b in walls)
            AddReference(b, side, line);
    }
	
	/// <summary>
	/// Goes through and combines any two Lines that together make a longer unbroken Line.
	/// </summary>
	public void CombineReferences() {
		
		Line lNew;
		
		//Go through every pair of Lines.
		foreach (Line l1 in GetAllLines())
			foreach (Line l2 in GetAllLines ())
				if (l1 != l2)
				{
					//If the two Lines are connected, connect them.
					if (Line.Connected(l1, l2))
					{
						lNew = Line.Connect (l1, l2);
						
						//Change all the Line references for these two Lines to the new combined Line.
						foreach (RecBounds b in GetWalls ())
							foreach (ColType c in GetSides (b))
								if (references[b][c] == l1 || references[b][c] == l2)
								{
									references[b][c] = lNew;
									//No other sides of this wall will use this line, so don't bother.
									break;
								}
					}
				}
	}
	
    /// <summary>
    /// Gets all walls (in no particular order) that together make up the given line.
    /// </summary>
    public List<RecBounds> AllReferences(Line l)
    {
        List<RecBounds> ret = new List<RecBounds>();

        foreach (RecBounds b in references.Keys)
            if (references[b].ContainsValue(l))
                ret.Add(b);

        return ret;
    }

    /// <summary>
    /// Gets all lines referenced by this system.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Line> GetAllLines()
    {		
		List<Line> ret = new List<Line>();
		
		foreach (RecBounds b in GetWalls ())
			foreach (ColType c in GetSides (b))
				if (!ret.Contains (references[b][c]))
					ret.Add (references[b][c]);

		return ret;
    }
    /// <summary>
    /// Gets all wall bounds used as keys for this system.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<RecBounds> GetWalls()
    {
		return references.Keys.ToList ();
    }
    /// <summary>
    /// Gets all Lines that the given wall is a part of.
    /// </summary>
    public IEnumerable<Line> GetLines(RecBounds wall)
    {
		return references[wall].Values.ToList ();
    }
    /// <summary>
    /// Gets all sides of the givne wall that represent part of a Line.
    /// </summary>
    /// <param name="wall"></param>
    /// <returns></returns>
    public IEnumerable<ColType> GetSides(RecBounds wall)
    {
		return references[wall].Keys.ToList ();
    }
}
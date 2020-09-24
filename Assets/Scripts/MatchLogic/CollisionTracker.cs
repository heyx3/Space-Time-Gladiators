using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Consts = WorldConstants;

/// <summary>
/// Tracks collisions between Actors and everything else.
/// Also will combine adjacent walls to save on processing/rendering cost,
/// once all walls have been added to the tracker.
/// Sends the following messages:
/// 	"CollideWithActor(StateMachine)"
/// 	"CollideWithOther(GameObject)"
/// 	"CollideWithWall(RecBounds)"
/// </summary>
public class CollisionTracker : MonoBehaviour
{
    public WallToLines Lines;
	
	public bool DrawLines = false;
	public bool DrawWalls = false;

    void Awake() {

        Actors = new List<StateMachine>();
        actorBounds = new List<RecBounds>();

        Others = new List<GameObject>();
        otherBounds = new List<RecBounds>();

        wallBounds = new List<RecBounds>();
    }

    //Actors.
    public List<StateMachine> Actors;
    private List<RecBounds> actorBounds;
    public void AddActor(StateMachine g) {
        Actors.Add(g);
        actorBounds.Add(new RecBounds(g.collider.bounds));
    }
    public void RemoveActor(StateMachine g) {
		
        int i = Actors.IndexOf(g);
		if (i == -1) return;
		
        Actors.RemoveAt(i);
        actorBounds.RemoveAt(i);
    }

    //Collectibles.
    public List<GameObject> Others;
    private List<RecBounds> otherBounds;
    public void AddOther(GameObject g) {
        Others.Add(g);
        otherBounds.Add(new RecBounds(g.collider.bounds));
    }
    public void RemoveOther(GameObject g) {
		
        int i = Others.IndexOf(g);
		if (i == -1) return;
		
        Others.RemoveAt(i);
        otherBounds.RemoveAt(i);
    }

    //Walls.
    private List<RecBounds> wallBounds;
    public IEnumerable<RecBounds> WallBounds { get { for (int i = 0; i < wallBounds.Count; ++i) yield return wallBounds[i]; } }
	/// <summary>
    /// Adds a static wall to be tracked.
	/// </summary>
	public void AddWall(RecBounds wall) {
		wallBounds.Add (wall);
	}
	/// <summary>
	/// Removes a wall from being tracked.
	/// </summary>
	public void RemoveWall(RecBounds wallB) {
		
		int index = wallBounds.IndexOf(wallB);
		if (index == -1) return;
		
		wallBounds.RemoveAt (index);
	}

    void FixedUpdate() {

		//Rebuild bounds.
		actorBounds.Clear();
		otherBounds.Clear();
        foreach (StateMachine a in Actors)
        {
            actorBounds.Add(a.ActorBounds);
        }
        foreach (GameObject g in Others)
        {
            otherBounds.Add(new RecBounds(g.collider.bounds));
        }
		
        //Otherwise, check for actor collisions.
        for (int i = 0; i < Actors.Count; ++i)
        {
            //TODO: If the actor moves at least 0.25 per fixed update, check halfway between his old and new bounds.

            //With other actors.
            for (int j = i + 1; j < Actors.Count; ++j)
            {
                if (actorBounds[i].Intersects(actorBounds[j]))
                {
                    Actors[i].SendMessage("CollideWithActor", Actors[j], SendMessageOptions.DontRequireReceiver);
                    Actors[j].SendMessage("CollideWithActor", Actors[i], SendMessageOptions.DontRequireReceiver);
                }
            }

            //With collectibles.
            for (int j = 0; j < Others.Count; ++j)
            {
                if (actorBounds[i].Intersects(otherBounds[j]))
                {
                    Actors[i].SendMessage("CollideWithOther", Others[j], SendMessageOptions.DontRequireReceiver);
                    Others[j].SendMessage("CollideWithActor", Actors[i], SendMessageOptions.DontRequireReceiver);
                }
            }

            //With walls.
            for (int j = 0; j < wallBounds.Count; ++j)
            {
				if (DrawWalls)
                	StateMachine.DrawBounds(wallBounds[j], Color.white);

                if (actorBounds[i].Intersects(wallBounds[j]))
                {
                    Actors[i].SendMessage("CollideWithWall", wallBounds[j], SendMessageOptions.DontRequireReceiver);
                }
            }
        }
		
		//Now check for objective/collectible collisions with other objectives/collectibles.
        for (int i = 0; i < Others.Count; ++i)
        {
            for (int j = i + 1; j < Others.Count; ++j)
            {
                if (otherBounds[i].Intersects(otherBounds[j]))
                {
                    Others[i].SendMessage("CollideWithOther", Others[j], SendMessageOptions.DontRequireReceiver);
                    Others[j].SendMessage("CollideWithOther", Others[i], SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        if (!DrawLines)
        {
            return;
        }

        foreach (Line l in Lines.GetAllLines())
        {
            Debug.DrawLine(l.Endpoint1, l.Endpoint2, Color.red);
        }
		
		StateMachine.DrawBounds(new RecBounds(WorldConstants.Size * 0.5f, WorldConstants.Size), Color.yellow);
    }
}
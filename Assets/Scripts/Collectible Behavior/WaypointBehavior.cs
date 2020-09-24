using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class WaypointBehavior : MonoBehaviour {
	
	LevelManager manager;
	CollisionTracker colTracker;
	
	Dictionary<Color, List<StateMachine>> teams;
	Dictionary<Color, List<StateMachine>> collidedWith;
	
	void Start()
	{
		manager = WorldConstants.MatchController.GetComponent<LevelManager>();
        colTracker = WorldConstants.ColTracker;
	}
	
	private bool initialized = false;
	void Update()
	{
		if (initialized || colTracker.Actors.Count == 0) return;
		
		initialized = true;
		
		//Get the players by team.
		collidedWith = new Dictionary<Color, List<StateMachine>>();
		teams = new Dictionary<Color, List<StateMachine>>();
		foreach (StateMachine st in colTracker.Actors)
		{
			if (!teams.ContainsKey(st.ActorData.Team))
			{
				teams.Add(st.ActorData.Team, new List<StateMachine>());
				collidedWith.Add (st.ActorData.Team, new List<StateMachine>());
			}
			teams[st.ActorData.Team].Add (st);
		}
	}
	
	void CollideWithActor(StateMachine st)
	{
		if (!initialized)
		{
			collidedWith = new Dictionary<Color, List<StateMachine>>();
			collidedWith.Add (st.ActorData.Team, new List<StateMachine>());
		}
		
		if (!collidedWith[st.ActorData.Team].Contains (st))
		{
			collidedWith[st.ActorData.Team].Add (st);
		}
	}
	
	void FixedUpdate()
	{
		if (!initialized) return;
		
		//See if any teams have captured the waypoint.
		foreach (Color c in collidedWith.Keys)
		{
			if (teams[c].Count == collidedWith[c].Count)
			{
                //Give the waypoint capture to the first player that entered the waypoint.
                teams[c][0].OwnerStats.WaypointsCaptured += 1;

                WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.FourMessages(WorldConstants.ActorConsts.WaypointCapturedMessages, teams[c][0], null),
                                                                     st => st.ActorData.Team == c, 
                                                                     WorldConstants.Creator.CreateWaypointFloatingText);
				WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.WaypointCaptured);
				
				GameObject.Destroy (gameObject);
				return;
			}
		}
		
		//Now check to see which players aren't colliding anymore.
		List<Color> keys = collidedWith.Keys.ToList();
		List<StateMachine> temp;
		RecBounds thisB = new RecBounds(collider.bounds);
		foreach (Color c in keys)
		{
			temp = collidedWith[c].ToList ();
			foreach (StateMachine st in temp)
			{
				if (!st.ActorBounds.Intersects(thisB))
				{
					collidedWith[c].Remove (st);
				}
			}
		}
	}
}

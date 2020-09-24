using UnityEngine;
using System.Collections;

using Msgs = PrefabCreator.PlayerMessages;

public class ActorConstants : MonoBehaviour
{
    //Stats.
    /// <summary>
    /// The range of possible speed stats for players.
    /// </summary>
    public Interval SpeedInterval = new Interval(0.5f, 1.5f, true, 2);
    /// <summary>
    /// The range of possible strength stats for players.
    /// </summary>
    public Interval StrengthInterval = new Interval(2.0f, 4.0f, true, 2);
    /// <summary>
    /// This player's mass. Used for acceleration and momentum calculations.
    /// </summary>
	public float Mass = 2.5f;
    /// <summary>
    /// The player's normal horizontal acceleration.
    /// </summary>
    public float Acceleration = 10.0f;

    //Collision.
    /// <summary>
    /// The largest possible multiplier an actor can get to his Pain dealt by being on top.
    /// </summary>
    public float MaxAdvantageMultiplier = 1.5f;
    /// <summary>
    /// The multiplier given to actors' Pain dealt if both are on the same team.
    /// </summary>
    public float SameTeamMultiplier = 0.5f;
    /// <summary>
    /// The multiplier for damage dealt given to the actor who lost a collision.
    /// </summary>
    public float LostCollisionMultiplier = 0.5f;

    /// <summary>
    /// The smallest-possible velocity an Actor can have and still register a collision with another Actor.
    /// </summary>
    public float MinimumCollisionVelocity = 4.37f;

    //Other.
    public static Color EnemiesTeam = Color.black;
    public Msgs FlagGrabbedMessages = new Msgs("You grabbed a flag!", "Ally grabbed a flag!", "Enemy grabbed a flag!", "Enemy grabbed your flag!");
    public Msgs FlagDroppedMessages = new Msgs("You dropped a flag!", "Ally dropped a flag!", "Enemy dropped a flag!", "Enemy dropped your flag!");
    public Msgs FlagCapturedMessages = new Msgs("You captured a flag!", "Ally captured a flag!", "Enemy capped a flag!", "Enemy capped your flag!");
    public Msgs FlagReturnedMessages = new Msgs("Your flag was returned!", "Your flag was returned!", "Enemy's flag was returned!", "Enemy's flag was returned!");
    public Msgs VIPChangedMessages = new Msgs("You are the VIP!", "An ally is the VIP!", "New VIP!", "New VIP!");
    public Msgs WaypointCapturedMessages = new Msgs("You captured the waypoint!", "", "Enemy captured a waypoint!", "Enemy captured a waypoint!");
    public Msgs WaypointSpawnedMessages = new Msgs("Waypoint appeared!", "Waypoint appeared!", "Waypoint appeared!", "Waypoint appeared!");
    public Msgs PowerupMultiplierMessages = new Msgs("Score multiplier!", "Ally score multiplier!", "Enemy score multiplier!", "Enemy score multiplier!");
    public Msgs PowerupThrewEnemiesMessages = new Msgs("Stunned enemies!", "Ally stunned enemies!", "Enemy stunned you!", "Enemy stunned you!");
    public Msgs PowerupConfusedEnemiesMessages = new Msgs("Confused enemies!", "Ally confused enemies!", "Enemy confused you!", "Enemy confused you!");
    public Msgs PowerupFinishedConfusingEnemiesMessages = new Msgs("No longer confused!", "", "", "");
    public Msgs PowerupHidWallsMessages = new Msgs("Enemies can't see walls!", "Enemies can't see walls!", "", "Walls are hidden!");
	public Msgs OneMinuteLeftMessages = new Msgs("One minute left!", "One minute left!", "One minute left!", "One minute left!");
	public Msgs AlmostWonMessages = new Msgs("You're about to win!", "You're about to win!", "Another team is about to win!", "Another team is about to win!");
    public Msgs GameOverMessages = new Msgs("You won!", "You won!", "Game Over!", "You lost!");

    void Awake()
    {
        WorldConstants.ActorConsts = this;
    }

    void OnDestroy()
    {
		GameObject consts = GameObject.Find ("Debugger");
		if (consts == null)
		{
			return;
		}
		
		ConstantsWriter cw = consts.GetComponent<ConstantsWriter>();
		if (cw == null)
		{
			return;
		}
		
        cw.WriteConstants();
    }
}

using UnityEngine;
using System.Collections;

public class PlayerConstants : MonoBehaviour
{
	#region Cosmetic Data
	
	public float RunningDirtEmitChance = 0.3f;
	public Interval WallslideDirtEmitChance = new Interval(0.1f, 0.43f, true, 2);
	public float WallslideDirtCutoffSpeed = 2.0f;
	public float CeilingSlideDirtEmitChance = 0.3f;

    public Vector2 FloatingTextOffset = new Vector2(0.0f, 0.3f);
	
	#endregion
	
    #region Other Data

    //Friction.
    /// <summary>
    /// Friction while moving along a wall's surface.
    /// </summary>
    public float MovingTouchingWallSlowdown = 0.999f;
    /// <summary>
    /// Friction while moving along a wall's surface in the opposite direction from the velocity.
    /// </summary>
    public float StoppingOnWallSlowdown = 0.05f;
    /// <summary>
    /// Friction while in the air.
    /// </summary>
    public float InAirXSlowdown = 0.99999f;

    //Animation.
    /// <summary>
    /// The minimum fall speed for a player to need to recoil from landing on the ground.
    /// </summary>
    public float MinimumFallSpeed = 10.5f;
    /// <summary>
    /// The amount of time before the player hits his apex in air at which
    /// the air-up-to-air-down transition animation plays.
    /// </summary>
    public float TimeTillApexTransition = 0.5f;

    #endregion

    #region Collision Data

	/// <summary>
    /// In a collision between two Actors, the losing Actor has his velocity multiplied by this value.
	/// </summary>
	public float MomentumDamp = 0.6f;
	
	/// <summary>
    /// In a collision between two Actors, the losing Actor has a portion of the winner's velocity added to his new velocity. These are the portions.
	/// </summary>
	//public Vector3 LoseCollisionMultiplier = new Vector3(0.5f, 0.4f, 0.0f);
	/// <summary>
    /// In a collision between two Actors, the winning Actor has this value added to his velocity (reflected in the X depending on his original X direction).
	/// </summary>
    //public Vector3 WinCollisionVelocityChange = new Vector3(3.0f, 4.0f, 0.0f);

    #endregion

    #region Running Data

    //TODO: Give the player a little upward bump when he runs off the edge of a ceiling.

    /// <summary>
	/// If an actor is trying to move in the opposite direction horizontally from their current velocity, their acceleration is multiplied by this value.
	/// </summary>
	public float ChangeDirAccelScale = 5.0f;

    /// <summary>
    /// The player's maximum-allowable speeds independently in both the X and Y direction.
    /// </summary>
    public Vector2 MaxSpeeds = new Vector2(20, 20);

    #endregion

    #region State Data

    //Wall-sliding data.
	/// <summary>
	/// The amount of time a player has to continuously hold down the movement key while wall-sliding before he pushes off of a wall.
	/// </summary>
	public float WallSlideEscapeTime = 0.3f;
    /// <summary>
    /// The player's y speed when he hits the ground-pound key while sliding down a wall.
    /// </summary>
	public float WallSlideGroundPoundYSpeed = -9.0f;
	
	//Ground-pounding data.
    /// <summary>
    /// This player's y speed after ground-pounding in mid-air.
    /// </summary>
	public float GroundPoundYSpeed = -10.0f;
    /// <summary>
    /// While ground-pounding, this player has a scaled-down acceleration.
    /// </summary>
	public float GroundPoundHorizontalMovementScale = 0.65f;
	
    //Hurt data.
	/// <summary>
	/// The amount of time a player stays Hurt.
	/// </summary>
	public float HurtTime = 0.769f;

    #endregion

    #region Jumping Data

    /// <summary>
    /// The amount of time between a player starting the jumping animation and pushing off the ground.
    /// </summary>
    public float JumpToAirTime = 1.0f / 10.0f;
    /// <summary>
    /// Identical to JumpToAirTime, but used when the player is landing.
    /// </summary>
    public float AirToGroundTime = 1.0f / 10.0f;
	/// <summary>
	/// The y speed of a player who just jumped.
	/// </summary>
	public float JumpYSpeed = 13.0f;
	/// <summary>
	/// Players can hold the jump button while moving upwards to exert a small force opposing gravity.
	/// </summary>
	public float JumpHoldGravityScale = 0.5f;
	/// <summary>
	/// Right after a player wall-jumps, his acceleration is dimmed for a short while.
	/// </summary>
	public float WallJumpAccelerationDimScale = 0.1f;
	/// <summary>
	/// Right after a player wall-jumps, his acceleration is dimmed for a short while.
	/// </summary>
	public float WallJumpAccelerationDimTime = 0.4f;
	/// <summary>
	/// The offset to a player's position when he pushes off of a wall.
	/// </summary>
    public float WallPushOffOffset = 0.3f;
	/// <summary>
	/// The offset to a player's position when he pushes off of a ceiling.
	/// </summary>
    public float CeilingPushOffOffset = 0.001f;
    /// <summary>
    /// If this player pushes off a wall, his velocity will be set to this (reflected in the X direction as necessary).
    /// If he pushes off a ceiling, his y velocity will be set to the opposite of this property's y component.
    /// </summary>
    public Vector3 WallSlidePushOff = new Vector3(6.0f, 10.0f, 0);
    /// <summary>
    /// The length of time the player "prepares" to push off the wall for.
    /// </summary>
    public float WallSlidePushOffTime = 1.0f / 10.0f;

    #endregion

    void Awake()
    {
        WorldConstants.PlayerConsts = this;
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

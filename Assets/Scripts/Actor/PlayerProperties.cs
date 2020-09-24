using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Stats))]
public class PlayerProperties : MonoBehaviour {
	
	#region Cosmetic Data
	
	/// <summary>
	/// The chance every update cycle that a running player will emit particles.
	/// </summary>
	public float RunningDirtEmitChance { get { return pConsts.RunningDirtEmitChance; } }
	/// <summary>
	/// The chance every update cycle that a wall-sliding player will emit particles.
	/// </summary>
	public Interval WallslideDirtEmitChance { get { return pConsts.WallslideDirtEmitChance; } }
	/// <summary>
	/// The minimum-allowable Y speed for a wall-sliding player to emit particles.
	/// </summary>
	public float WallslideDirtCutoffSpeed { get { return pConsts.WallslideDirtCutoffSpeed; } }
	/// <summary>
	/// The chance every update cycle that a ceiling-sliding player will emit particles.
	/// </summary>
	public float CeilingSlideDirtEmitChance { get { return pConsts.WallslideDirtCutoffSpeed; } }
	
	#endregion
	
    #region Other Data

    /// <summary>
    /// Friction while moving along a wall's surface.
    /// </summary>
    public float MovingTouchingWallSlowdown
    {
        get
        {
            return pConsts.MovingTouchingWallSlowdown;
        }
    }
    /// <summary>
    /// Friction while moving along a wall's surface in the opposite direction from the velocity.
    /// </summary>
    public float StoppingOnWallSlowdown
    {
        get
        {
            return pConsts.StoppingOnWallSlowdown;
        }
    }
    /// <summary>
    /// Friction while in the air.
    /// </summary>
    public float InAirXSlowdown
    {
        get
        {
            return pConsts.InAirXSlowdown;
        }
    }

    //Animation.
    /// <summary>
    /// The minimum fall speed for a player to need to recoil from landing on the ground.
    /// </summary>
    public float MinimumFallSpeed
    {
        get
        {
            return pConsts.MinimumFallSpeed;
        }
    }
    /// <summary>
    /// The amount of time before the player hits his apex in air at which
    /// the air-up-to-air-down transition animation plays.
    /// </summary>
    public float TimeTillApexTransition
    {
        get
        {
            return pConsts.TimeTillApexTransition;
        }
    }

    #endregion

    #region Collision Data

    /// <summary>
    /// In a collision between two Actors, the losing Actor has his velocity multiplied by this value.
    /// </summary>
    public float MomentumDamp
    {
        get
        {
            return IncreaseWithSpeed(pConsts.MomentumDamp, 0.3f);
        }
    }

    #endregion

    #region Running Data

    /// <summary>
    /// If an actor is trying to move in the opposite direction horizontally from their current velocity, their acceleration is multiplied by this value.
    /// </summary>
    public float ChangeDirAccelScale
    {
        get
        {
            return IncreaseWithSpeed(pConsts.ChangeDirAccelScale, 0.5f);
        }
    }

    /// <summary>
    /// The player's maximum-allowable speeds independently in both the X and Y direction.
    /// </summary>
    public Vector2 MaxSpeeds
    {
        get
        {
            return new Vector2(IncreaseWithStrength(pConsts.MaxSpeeds.x, 0.6f),
                               IncreaseWithStrength(pConsts.MaxSpeeds.y, 0.4f));
        }
    }

    #endregion

    #region State Data

    //Wall-sliding data.
    /// <summary>
    /// The amount of time a player has to continuously hold down the movement key while wall-sliding before he pushes off of a wall.
    /// </summary>
    public float WallSlideEscapeTime
    {
        get
        {
            return DecreaseWithSpeed(pConsts.WallSlideEscapeTime, 0.2f);
        }
    }
    /// <summary>
    /// The player's y speed when he hits the ground-pound key while sliding down a wall.
    /// </summary>
    public float WallSlideGroundPoundYSpeed
    {
        get
        {
            return IncreaseWithStrength(pConsts.WallSlideGroundPoundYSpeed, 0.3f);
        }
    }

    //Ground-pounding data.
    /// <summary>
    /// This player's y speed after ground-pounding in mid-air.
    /// </summary>
    public float GroundPoundYSpeed
    {
        get
        {
            return IncreaseWithStrength(pConsts.GroundPoundYSpeed, 0.5f);
        }
    }
    /// <summary>
    /// While ground-pounding, this player has a scaled-down acceleration.
    /// </summary>
    public float GroundPoundHorizontalMovementScale
    {
        get
        {
            return IncreaseWithSpeed(pConsts.GroundPoundHorizontalMovementScale, 0.5f);
        }
    }

    //Hurt data.
    /// <summary>
    /// The amount of time a player stays hurt.
    /// </summary>
    public float HurtTime
    {
        get
        {
            return IncreaseWithSpeed(pConsts.HurtTime, 0.5f);
        }
    }

    #endregion

    #region Jumping Data

    /// <summary>
    /// The amount of time between a player starting the jumping animation and pushing off the ground.
    /// </summary>
    public float JumpToAirTime
    {
        get
        {
            return IncreaseWithStrength(pConsts.JumpToAirTime, 0.5f);
        }
    }
    /// <summary>
    /// Identical to JumpToAirTime, but used when the player is landing.
    /// </summary>
    public float AirToGroundTime
    {
        get
        {
            return IncreaseWithSpeed(pConsts.AirToGroundTime, 0.5f);
        }
    }
    /// <summary>
    /// The y speed of a player who just jumped.
    /// </summary>
    public float JumpYSpeed
    {
        get
        {
            return IncreaseWithStrength(pConsts.JumpYSpeed, 0.5f);
        }
    }
    /// <summary>
    /// Players can hold the jump button while moving upwards to exert a small force opposing gravity.
    /// </summary>
    public float JumpHoldGravityScale
    {
        get
        {
            return pConsts.JumpHoldGravityScale;
        }
    }
    /// <summary>
    /// Right after a player wall-jumps, his acceleration is dimmed for a short while.
    /// </summary>
    public float WallJumpAccelerationDimScale
    {
        get
        {
            return pConsts.WallJumpAccelerationDimScale;
        }
    }
    /// <summary>
    /// Right after a player wall-jumps, his acceleration is dimmed for a short while.
    /// </summary>
    public float WallJumpAccelerationDimTime
    {
        get
        {
            return DecreaseWithSpeed(pConsts.WallJumpAccelerationDimTime, 0.6f);
        }
    }
    /// <summary>
    /// The offset to a player's position when he pushes off of a wall.
    /// </summary>
    public float WallPushOffOffset
    {
        get
        {
            return pConsts.WallPushOffOffset;
        }
    }
	/// <summary>
    /// The offset to a player's position when he pushes off of a ceiling.
    /// </summary>
    public float CeilingPushOffOffset
    {
        get
        {
            return pConsts.CeilingPushOffOffset;
        }
    }
    /// <summary>
    /// If this player pushes off a wall, his velocity will be set to this (reflected in the X direction as necessary).
    /// If he pushes off a ceiling, his y velocity will be set to the opposite of this property's y component.
    /// </summary>
    public Vector3 WallSlidePushOff
    {
        get
        {
            return new Vector3(IncreaseWithStrength(pConsts.WallSlidePushOff.x, 0.5f),
                               IncreaseWithStrength(pConsts.WallSlidePushOff.y, 0.5f));
        }
    }
    /// <summary>
    /// The length of time the player "prepares" to push off the wall for.
    /// </summary>
    public float WallSlidePushOffTime
    {
        get
        {
            return IncreaseWithStrength(pConsts.WallSlidePushOffTime, 0.5f);
        }
    }

    #endregion

    /// <summary>
    /// Gets the value of a property that increases with speed.
    /// This method scales "cnst" based on the player's speed
    /// and the given range of possible scales ("valRange").
    /// </summary>
    /// <param name="cnst">The "default" value for somebody with average speed.</param>
    /// <param name="valRange">"cnst" will be scaled by an amount based on the player's speed.
    /// This value is the range (centered at 1.0 for "average" speed) of possible scales.</param>
    /// <returns>Returns "cnst" multiplied by something in the interval centered at 1.0 with range "valRange",
    /// based on the player's speed (higher speed means larger result).</returns>
    private float IncreaseWithSpeed(float cnst, float valRange)
    {
        return cnst * aConsts.SpeedInterval.Map(new Interval(valRange, 1.0f,
                                                             aConsts.SpeedInterval.DecimalPlaceAccuracy),
                                                speedStat);
    }
    /// <summary>
    /// Gets the value of a property that decreases with speed.
    /// This method scales "cnst" based on the player's speed
    /// and the given range of possible scales ("valRange").
    /// </summary>
    /// <param name="cnst">The "default" value for somebody with average speed.</param>
    /// <param name="valRange">"cnst" will be scaled by an amount based on the player's speed.
    /// This value is the range (centered at 1.0 for "average" speed) of possible scales.</param>
    /// <returns>Returns "cnst" multiplied by something in the interval centered at 1.0 with range "valRange",
    /// based on the player's speed (higher speed means smaller result).</returns>
    private float DecreaseWithSpeed(float cnst, float valRange)
    {
        float inverse = aConsts.SpeedInterval.ReflectAroundCenter(speedStat);
        return cnst * aConsts.SpeedInterval.Map(new Interval(valRange, 1.0f,
                                                             aConsts.SpeedInterval.DecimalPlaceAccuracy),
                                                inverse);
    }
    /// <summary>
    /// Gets the value of a property that increases with strength.
    /// This method scales "cnst" based on the player's strength
    /// and the given range of possible scales ("valRange").
    /// </summary>
    /// <param name="cnst">The "default" value for somebody with average strength.</param>
    /// <param name="valRange">"cnst" will be scaled by an amount based on the player's strength.
    /// This value is the range (centered at 1.0 for "average" strength) of possible scales.</param>
    /// <returns>Returns "cnst" multiplied by something in the interval centered at 1.0 with range "valRange",
    /// based on the player's strength (higher strength means larger result).</returns>
    private float IncreaseWithStrength(float cnst, float valRange)
    {
        return cnst * aConsts.StrengthInterval.Map(new Interval(valRange, 1.0f,
                                                                aConsts.StrengthInterval.DecimalPlaceAccuracy),
                                                   strengthStat);
    }
    /// <summary>
    /// Gets the value of a property that decreases with strength.
    /// This method scales "cnst" based on the player's strength
    /// and the given range of possible scales ("valRange").
    /// </summary>
    /// <param name="cnst">The "default" value for somebody with average strength.</param>
    /// <param name="valRange">"cnst" will be scaled by an amount based on the player's strength.
    /// This value is the range (centered at 1.0 for "average" strength) of possible scales.</param>
    /// <returns>Returns "cnst" multiplied by something in the interval centered at 1.0 with range "valRange",
    /// based on the player's strength (higher strength means smaller result).</returns>
    private float DecreaseWithStrength(float cnst, float valRange)
    {
        float inverse = aConsts.StrengthInterval.ReflectAroundCenter(strengthStat);
        return cnst * aConsts.StrengthInterval.Map(new Interval(valRange, 1.0f,
                                                                aConsts.StrengthInterval.DecimalPlaceAccuracy),
                                                   inverse);
    }

    //Cached references.
    private Stats stats;
    private Rules matchRules;
    private PlayerConstants pConsts;
    private ActorConstants aConsts;

    private float speedStat { get { return stats.Speed; } }
    private float strengthStat { get { return stats.Strength; } }

    void Awake()
    {
        stats = GetComponent<Stats>();

        matchRules = WorldConstants.MatchController.GetComponent<LevelManager>().MatchRules;
        pConsts = WorldConstants.ConstantsOwner.GetComponent<PlayerConstants>();
        aConsts = WorldConstants.ConstantsOwner.GetComponent<ActorConstants>();
    }
}

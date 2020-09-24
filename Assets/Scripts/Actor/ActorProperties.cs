using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Stats))]
public class ActorProperties : MonoBehaviour {

    /// <summary>
    /// The largest possible multiplier an actor can get to his Pain dealt by being on top.
    /// </summary>
    public float MaxAdvantageMultiplier
    {
        get
        {
            return IncreaseWithStrength(aConsts.MaxAdvantageMultiplier, 0.3f);
        }
    }
    /// <summary>
    /// The multiplier given to actors' Pain dealt if both are on the same team.
    /// </summary>
    public float SameTeamMultiplier
    {
        get
        {
            return DecreaseWithSpeed(aConsts.SameTeamMultiplier, 0.2f);
        }
    }
    /// <summary>
    /// The multiplier for damage dealt given to the actor who lost a collision.
    /// </summary>
    public float LostCollisionMultiplier
    {
        get
        {
            return IncreaseWithStrength(aConsts.LostCollisionMultiplier, 0.3f);
        }
    }
    /// <summary>
    /// The smallest-possible velocity an Actor can have and still register a collision with another Actor.
    /// </summary>
    public float MinimumCollisionVelocity
    {
        get
        {
            return aConsts.MinimumCollisionVelocity;
        }
    }

    /// <summary>
    /// This actor's mass. Used for acceleration and momentum calculations.
    /// </summary>
    public float Mass
    {
        get
        {
            return IncreaseWithStrength(aConsts.Mass, 0.65f);
        }
    }
    /// <summary>
    /// The actor's normal horizontal acceleration.
    /// </summary>
    public float Acceleration
    {
        get
        {
            return IncreaseWithSpeed(aConsts.Acceleration, 0.4f);
        }
    }

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
    private ActorConstants aConsts;

    private float speedStat
    {
        get
        {
            float spd = stats.Speed;

            if (stats.CarryingFlag)
            {
                spd *= matchRules.CTF.FlagCarrySpeedScale;
            }

            return spd;
        }
    }
    private float strengthStat
    {
        get
        {
            float str = stats.Strength;

            if (stats.CarryingFlag)
            {
                str *= matchRules.CTF.FlagCarryStrengthScale;
            }

            return str;
        }
    }

    void Awake()
    {
        stats = GetComponent<Stats>();

        matchRules = WorldConstants.MatchController.GetComponent<LevelManager>().MatchRules;
        aConsts = WorldConstants.ConstantsOwner.GetComponent<ActorConstants>();
    }
}

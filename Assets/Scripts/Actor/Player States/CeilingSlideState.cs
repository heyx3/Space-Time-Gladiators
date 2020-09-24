using UnityEngine;
using System.Collections.Generic;

public class CeilingSlideState : ActorState
{
    //If "false", the running noise will be used instead.
    private const bool useSlidingNoise = false;

    /* Keep the player still, but still keep track of his projected y speed if the ceiling hadn't been there.
     * Once the projected speed reaches the opposite of the original speed (i.e. player is at the same height he hit the ceiling at),
     * release contact with the ceiling. */

    private double SimulatedGravity
    {
        get
        {
            //The player can hold "up" to prolong his time on the ceiling,
            //   just like he can hold "up" to prolong his time in the air.
            if (Owner.MovementKeyAxes.y > 0.0f)
            {
                return gravity * (1.0 - Owner.PlayerProps.JumpHoldGravityScale);
            }
            else return gravity;
        }
    }
    public double targetYSpeed;
    private double currentSimulatedYSpeed;
    private float y = System.Single.NaN;

    private float originalParticleYSpeed;

    private double gravity;

    private FadeLoopNoise slidingNoise;
    private float slidingNoiseMaxVolume;

    public override Vector3 ApplyFriction(Vector3 original)
    {
        return new Vector3(original.x * Owner.PlayerProps.MovingTouchingWallSlowdown, original.y, 0.0f);
    }

    public override void EnteringState()
    {
        Owner.Animator.CurrentAnimation = Animations.P_CeilingSlide;

        originalParticleYSpeed = Owner.RunningParts.WorldVelocity.y;
        Owner.RunningParts.WorldVelocity = new Vector3(Owner.RunningParts.WorldVelocity.x, 0.0f);

        RecBounds ab = Owner.ActorBounds;
        Owner.RunningParts.Position = new Vector3(ab.center.x, ab.top, Owner.RunningParts.Position.z);

        //Make the sliding noise.
        if (useSlidingNoise)
        {
            slidingNoise = new FadeLoopNoise(WorldConstants.PlayPhysNoises.GetNoise(PlayerPhysicsNoises.Events.Slide), "Sliding on Ceiling");
        }
        else
        {
            slidingNoise = new FadeLoopNoise(WorldConstants.PlayPhysNoises.GetNoise(PlayerPhysicsNoises.Events.Run), "Runnin on Ceiling");
        }

        slidingNoise.StartLoop();
        slidingNoiseMaxVolume = slidingNoise.Loop.GetComponent<ControlledNoise>().MaxVolume;
    }

    public override void ExitingState()
    {
        slidingNoise.EndLoop();
        GameObject.Destroy(slidingNoise.Loop);

        if (System.Single.IsNaN(y)) return;

        Owner.Animator.CurrentAnimation = Animations.P_CeilingSlide;

        Owner.RunningParts.WorldVelocity = new Vector3(Owner.RunningParts.WorldVelocity.x, originalParticleYSpeed);
    }

    public override void Sideways(float intensity)
    {
        //If the player is trying to move away from a wall, push him out just a bit.
        if (intensity > 0.0f && Owner.ColManager.WallSides[ColType.Right].Count > 0)
        {
            Owner.transform.position += new Vector3(0.01f, 0.0f, 0.0f);
        }
        if (intensity < 0.0f && Owner.ColManager.WallSides[ColType.Left].Count > 0)
        {
            Owner.transform.position += new Vector3(-0.01f, 0.0f, 0.0f);
        }

        //Emit running particles.
        if (!StateMachine.WithinError(intensity, 0.0f, 0.01f) &&
            Random.value < Owner.PlayerProps.CeilingSlideDirtEmitChance)
        {
            Vector3 v = Owner.RunningParts.WorldVelocity;
            Vector3 vOld = v;

            v.x = -StateMachine.Sign(intensity, 0.01f) * Mathf.Abs(v.x);
            v.x *= Mathf.Abs(Owner.Velocity.x / Owner.PlayerProps.MaxSpeeds.x);
            Owner.RunningParts.WorldVelocity = v;

            Owner.RunningParts.Emit();
            Owner.RunningParts.WorldVelocity = vOld;
        }

        base.Sideways(intensity);
    }

    /// <summary>
    /// Sets the data needed to calculate when the Player should let go of the ceiling:
    /// His y velocity at the time of collision, and the gravity pulling him (in the Y direction).
    /// </summary>
    public void SetTimingData(double velocityY, double gravity)
    {
        targetYSpeed = -velocityY;
        currentSimulatedYSpeed = velocityY;

        this.gravity = gravity;
    }

    public override void BeforeFixed()
    {
        if (System.Single.IsNaN(y))
        {
            y = Owner.transform.position.y;
        }
    }

    public override void AfterConstrainVelocity()
    {
        Owner.Velocity.y = 0.0f;
    }
    public override void AfterMove()
    {
        Vector3 pos = Owner.transform.position;
        pos.y = y;
        Owner.transform.position = pos;
    }

    public override void AfterFixed()
    {
        //Update sliding noise. The faster the player, the higher his volume.
        slidingNoise.UpdateLoop();
        if (useSlidingNoise)
        {
            slidingNoise.SetMaxVolume(Mathf.Lerp(0.0f, slidingNoiseMaxVolume,
                                                 Mathf.Abs(Owner.Velocity.x) / Owner.PlayerProps.MaxSpeeds.x));
        }

        //Update the correct timed stat.
        Owner.OwnerStats.TimeOnCeiling += Time.fixedDeltaTime;

        //Update the projected y speed.
        currentSimulatedYSpeed += SimulatedGravity * Time.fixedDeltaTime;

        //Let the player fall if he ran out of time, isn't in contact with a ceiling anymore, or hit a wall.
        if (currentSimulatedYSpeed <= targetYSpeed ||
            Owner.ColManager.WallSides[ColType.Bottom].Count == 0 ||
            Owner.ColManager.WallSides[ColType.Left].Count != 0 ||
            Owner.ColManager.WallSides[ColType.Right].Count != 0)
        {
            Owner.transform.position += new Vector3(0.0f, -Owner.PlayerProps.CeilingPushOffOffset, 0.0f);

            Owner.ChangeState(ScriptableObject.CreateInstance<InAirState>(), true);
            return;
        }

        //Check to see if he hit a wall.
        if (Owner.ColManager.WallSides[ColType.Left].Count > 0)
        {
            HitSide(Owner.ColManager.WallSides[ColType.Left][0], ColType.Left);
        }
        if (Owner.ColManager.WallSides[ColType.Right].Count > 0)
        {
            HitSide(Owner.ColManager.WallSides[ColType.Right][0], ColType.Right);
        }
    }

    public override void Up()
    {
        //Set the position and velocity.
        Owner.transform.position += new Vector3(0, -Owner.PlayerProps.CeilingPushOffOffset, 0);
        Owner.Velocity = new Vector3(Owner.Velocity.x, -Owner.PlayerProps.WallSlidePushOff.y, 0);

        //Set the state.
        InAirState state = ScriptableObject.CreateInstance<InAirState>();
        state.DimAcceleration();
        Owner.ChangeState(state, true);

        //Play the right noise.
        WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.SlidePushOff);
        WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.GroundPound);

    }
    public override void Down()
    {
        //Set the velocity.
        Owner.Velocity = new Vector3(0, -Owner.PlayerProps.WallSlidePushOff.y, 0);

        //Play the sound and make particles.
        WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.GroundPound);
        WorldConstants.Creator.CreateGroundpoundParticles(Owner);

        //Set the state.
        Owner.ChangeState(ScriptableObject.CreateInstance<GroundPoundState>(), true);
    }

    public override void HitSide(Line line, ColType c)
    {
        base.HitSide(line, c);

        //If the player wasn't already on a side, play the "landed" noise and emit particles.
        if (Owner.Velocity.x != 0.0f)
        {
            if (Mathf.Abs(Owner.Velocity.x) > Owner.PlayerProps.MinimumFallSpeed)
            {
                Owner.HardLanding.Position = new Vector3(line.ConstValue, Owner.ActorBounds.center.y, Owner.HardLanding.Position.z);
                Owner.HardLanding.Emit();
                WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.HeavyLand);
            }
            else
            {
                Owner.SoftLanding.Position = new Vector3(line.ConstValue, Owner.ActorBounds.center.y, Owner.SoftLanding.Position.z);
                Owner.SoftLanding.Emit();
                WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.SoftLand);
            }
        }

        Owner.Velocity.x = 0.0f;
        Owner.MoveToSideOfSurface(line, c);
    }

    public override string ToString()
    {
        return "Sliding along the ceiling";
    }
}
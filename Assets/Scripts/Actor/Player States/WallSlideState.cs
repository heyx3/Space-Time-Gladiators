using UnityEngine;
using System.Collections.Generic;

public class WallSlideState : ActorState
{
    public sbyte JumpDir;
    public ColType WallSide { get { return JumpDir == 1 ? ColType.Right : ColType.Left; } }

    private float tryBreakContactTime;

    private FadeLoopNoise slidingNoise;
    private float slidingNoiseMaxVolume;

    public override Vector3 Gravity
    {
        get
        {
            //If the player is moving down, use downward gravity.
            if (Owner.Velocity.y < 0)
            {
                return new Vector3(0.0f, WorldConstants.DownGravity, 0.0f);
            }

            //otherwise, if the player is moving up but holding down "up", use downward gravity and modify it.
            else if (Owner.MovementKeyAxes.y > 0)
            {
                return new Vector3(0.0f, WorldConstants.UpGravity * (1.0f - Owner.PlayerProps.JumpHoldGravityScale), 0.0f);
            }

            //Otherwise, just use upwards gravity.
            else
            {
                return new Vector3(0.0f, WorldConstants.UpGravity, 0.0f);
            }
        }
    }
    public override Vector3 ApplyFriction(Vector3 original)
    {
        return new Vector3(original.x,
                           original.y * Owner.PlayerProps.MovingTouchingWallSlowdown,
                           0.0f);
    }

    public override void EnteringState()
    {
        tryBreakContactTime = 0.0f;

        //Set the reflection direction.
        if (JumpDir > 0)
        {
            Owner.MeshController.SetDir(SetMeshFor2D.ReflectDir.Right);

            float oldVX = Owner.WallslidingParts.WorldVelocity.x;
            RecBounds a = Owner.ActorBounds;
            Owner.WallslidingParts.Position = new Vector3(a.left, a.center.y,
                                                          Owner.WallslidingParts.Position.z);
        }
        else
        {
            Owner.MeshController.SetDir(SetMeshFor2D.ReflectDir.Left);
            RecBounds a = Owner.ActorBounds;
            Owner.WallslidingParts.Position = new Vector3(a.right, a.center.y, Owner.WallslidingParts.Position.z);
        }

        //Set the animation.
        Owner.Animator.CurrentAnimation = Animations.P_WallSlide;

        //Keep the player flush with the wall.
        Owner.Velocity.x = 0.0f;

        //Make the sliding noise.
        slidingNoise = new FadeLoopNoise(WorldConstants.PlayPhysNoises.GetNoise(PlayerPhysicsNoises.Events.Slide), "Sliding on Wall");
        slidingNoise.StartLoop();
        slidingNoiseMaxVolume = slidingNoise.Loop.GetComponent<ControlledNoise>().MaxVolume;
    }
    public override void ExitingState()
    {
        slidingNoise.EndLoop();
        GameObject.Destroy(slidingNoise.Loop);
    }

    public override void SetReflection() { /* The player shouldn't change horizontal direction during a wallslide. */ }

    public override void Sideways(float intensity)
    {
        //If the player is trying to move off the wall, add onto the "break wall contact" time.
        if (Mathf.Sign(intensity) == Mathf.Sign(JumpDir) && intensity != 0.0f)
        {
            tryBreakContactTime += Time.fixedDeltaTime;
            if (tryBreakContactTime > Owner.PlayerProps.WallSlideEscapeTime)
            {
                Owner.transform.position += new Vector3(JumpDir * Owner.PlayerProps.WallPushOffOffset * 0.5f, 0.0f, 0.0f);

                Owner.ChangeState(ScriptableObject.CreateInstance<InAirState>(), true);
            }
        }
    }
    public override void Up()
    {
        WallSlidePushOffState ws = ScriptableObject.CreateInstance<WallSlidePushOffState>();
        ws.JumpDir = JumpDir;

        Owner.ChangeState(ws, false);
    }
    public override void Down()
    {
        if (Owner.Velocity.y > Owner.PlayerProps.WallSlideGroundPoundYSpeed)
        {
            Owner.Velocity = new Vector3(0, Owner.PlayerProps.WallSlideGroundPoundYSpeed, 0);
        }
    }

    public override void HitCeiling(Line line)
    {
        base.HitCeiling(line);
        if (Owner.Velocity.y > 0.0f)
        {
            Owner.Velocity = new Vector3(Owner.Velocity.x, 0.0f, 0.0f);
        }

        if (Mathf.Abs(Owner.Velocity.y) > Owner.PlayerProps.MinimumFallSpeed)
        {
            Owner.HardLanding.Position = new Vector3(Owner.ActorBounds.center.x, line.ConstValue, Owner.HardLanding.Position.z);
            Owner.HardLanding.Emit();
            WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.HeavyLand);
        }
        else
        {
            Owner.SoftLanding.Position = new Vector3(Owner.ActorBounds.center.x, line.ConstValue, Owner.SoftLanding.Position.z);
            Owner.SoftLanding.Emit();
            WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.SoftLand);
        }
    }
    public override void HitFloor(Line line)
    {
        base.HitFloor(line);
        Owner.Velocity.y = 0.0f;
        Owner.MoveAboveOrBelowSurface(line, ColType.Top);

        if (Mathf.Abs(Owner.Velocity.y) > Owner.PlayerProps.MinimumFallSpeed)
        {
            Owner.HardLanding.Position = new Vector3(Owner.ActorBounds.center.x, Owner.ActorBounds.bottom, Owner.HardLanding.Position.z);
            Owner.HardLanding.Emit();
            WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.HeavyLand);
        }
        else
        {
            Owner.SoftLanding.Position = new Vector3(Owner.ActorBounds.center.x, Owner.ActorBounds.bottom, Owner.SoftLanding.Position.z);
            Owner.SoftLanding.Emit();
            WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.SoftLand);
        }

        Owner.ChangeState(ScriptableObject.CreateInstance<WalkingState>(), false);
    }

    public override void AfterConstrainVelocity()
    {
        Owner.Velocity.x = 0.0f;
    }
    public override void AfterFixed()
    {
        //Update sliding noise. The faster the player, the higher his volume.
        slidingNoise.UpdateLoop();
        slidingNoise.SetMaxVolume(Mathf.Lerp(0.0f, slidingNoiseMaxVolume,
                                             Mathf.Abs(Owner.Velocity.y) / Owner.PlayerProps.MaxSpeeds.y));

        //See if there are any walls left on the correct side. If not, we are in the air.
        if (Owner.ColManager.WallSides[WallSide].Count == 0)
        {
            Owner.ChangeState(ScriptableObject.CreateInstance<InAirState>(), true);
        }

        //Update a stat.
        Owner.OwnerStats.TimeOnWall += Time.fixedDeltaTime;

        //Update/emit particles.

        Vector3 newV = Owner.WallslidingParts.WorldVelocity;
        newV.y = -(Owner.Velocity.y / Owner.PlayerProps.MaxSpeeds.y) *
                  StateMachine.Sign(Owner.WallslidingParts.WorldVelocity.y, 0.01f);
        Owner.WallslidingParts.WorldVelocity = newV;

        if (Random.value < Interval.ZeroToOneInterval.Map(Owner.PlayerProps.WallslideDirtEmitChance,
                                                          Mathf.Abs(Owner.Velocity.y) / Owner.PlayerProps.MaxSpeeds.y) &&
            Mathf.Abs(Owner.Velocity.y) >= Owner.PlayerProps.WallslideDirtCutoffSpeed)
        {
            Owner.WallslidingParts.Emit();
        }
    }

    public override string ToString()
    {
        return "Wall-sliding";
    }
}
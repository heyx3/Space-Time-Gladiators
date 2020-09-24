using UnityEngine;
using System.Collections;

public class HurtState : ActorState
{
    public double elapsed = 0.0;

    private AnimatedTextureExtendedUV animation;

    public override void EnteringState()
    {
        Owner.Animator.CurrentAnimation = Animations.P_Hurt;
        animation = Owner.GetComponent<AnimatedTextureExtendedUV>();
        animation.Blink = true;
    }
    public override void ExitingState()
    {
        animation.IsAnimating = true;
        animation.Blink = false;
    }

    public override void AnimationDone()
    {
        Owner.Animator.CurrentAnimation = Animations.P_Hurt;
        animation.IsAnimating = false;
        animation.ChangeFrame(animation.Frames - 1);
    }

    public override void SetReflection()
    {
        if (Owner.Velocity.x > 0)
        {
            Owner.MeshController.SetDir(SetMeshFor2D.ReflectDir.Right);
        }
        else if (Owner.Velocity.x < 0)
        {
            Owner.MeshController.SetDir(SetMeshFor2D.ReflectDir.Left);
        }
    }

    public override Vector3 ApplyFriction(Vector3 original)
    {
        return new Vector3(original.x * Owner.PlayerProps.InAirXSlowdown, original.y, 0.0f);
    }

    public override void Update()
    {
        //Update the clock.
        elapsed += Time.deltaTime;

        //If the player has been hurt for long enough, go back to normal.
        if (elapsed >= Owner.PlayerProps.HurtTime)
        {
            //The player will be either in the air or walking.
            if (Owner.ColManager.WallSides[ColType.Top].Count == 0)
            {
                Owner.ChangeState(ScriptableObject.CreateInstance<InAirState>(), false);
            }
            else
            {
                Owner.ChangeState(ScriptableObject.CreateInstance<WalkingState>(), false);
            }
        }
    }

    public override void AfterFixed()
    {
        //Update the correct timed stats.

        Owner.OwnerStats.TimeHurt += Time.fixedDeltaTime;

        if (Owner.ColManager.WallSides[ColType.Top].Count > 0)
        {
            Owner.OwnerStats.TimeOnGround += Time.fixedDeltaTime;
        }
        else
        {
            Owner.OwnerStats.TimeInAir += Time.fixedDeltaTime;
        }

        //If this player is still colliding with a side, raise the proper collision event again.

        if (Owner.ColManager.WallSides[ColType.Left].Count > 0)
        {
            HitSide(Owner.ColManager.WallSides[ColType.Left][0], ColType.Left);
        }

        if (Owner.ColManager.WallSides[ColType.Right].Count > 0)
        {
            HitSide(Owner.ColManager.WallSides[ColType.Right][0], ColType.Right);
        }

        if (Owner.ColManager.WallSides[ColType.Top].Count > 0)
        {
            HitFloor(Owner.ColManager.WallSides[ColType.Top][0]);
        }

        if (Owner.ColManager.WallSides[ColType.Bottom].Count > 0)
        {
            HitCeiling(Owner.ColManager.WallSides[ColType.Bottom][0]);
        }
    }

    public override void HitFloor(Line line)
    {
        base.HitFloor(line);

        Owner.Velocity.y = 0.0f;
        Owner.MoveAboveOrBelowSurface(line, ColType.Top);

        //If the player wasn't already on a floor, play the "landed" noise and emit particles.
        if (Owner.Velocity.y != 0.0f)
        {
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
        }

    }
    public override void HitCeiling(Line line)
    {
        base.HitCeiling(line);

        //If the player wasn't already on a ceiling, play the "landed" noise and emit particles.
        if (Owner.Velocity.y != 0.0f)
        {
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
        }

        Owner.Velocity.y = 0.0f;
        Owner.MoveAboveOrBelowSurface(line, ColType.Bottom);
    }
    public override void HitSide(Line line, ColType c)
    {
        base.HitSide(line, c);

        Owner.Velocity.x = 0.0f;
        Owner.MoveToSideOfSurface(line, c);

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
    }

    public override void Sideways(float intensity) { /* Do nothing. */ }

    public override Vector3 Gravity
    {
        get
        {
            //Don't apply gravity if the player is laying on the ground.
            if (Owner.ColManager.WallSides[ColType.Top].Count > 0) return Vector3.zero;
            return new Vector3(0, WorldConstants.DownGravity, 0);
        }
    }
    public override bool Hurtable { get { return false; } }

    public override string ToString()
    {
        return "Hurt";
    }
}
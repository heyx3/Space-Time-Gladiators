using UnityEngine;
using System.Collections;

public class GroundPoundState : ActorState
{
    public override void EnteringState()
    {
        //Set the player to the groundpound speed, assuming he isn't already going faster.
        if (-Owner.Velocity.y < Mathf.Abs(Owner.PlayerProps.GroundPoundYSpeed))
        {
            Owner.Velocity = new Vector3(Owner.Velocity.x, Owner.PlayerProps.GroundPoundYSpeed, 0);
        }

        Owner.Animator.CurrentAnimation = Animations.P_GroundPound;
    }

    public override Vector3 Gravity { get { return new Vector3(0.0f, WorldConstants.DownGravity, 0.0f); } }

    public override Vector3 ApplyFriction(Vector3 original)
    {
        return new Vector3(original.x * Owner.PlayerProps.InAirXSlowdown, original.y, 0.0f);
    }

    public override void Sideways(float intensity)
    {
        //If the player is against a wall and trying to move away from it, bump him away a bit.
        const float bumpAmount = 0.05f;
        if (intensity > 0.0f && Owner.ColManager.WallSides[ColType.Right].Count > 0)
        {
            Owner.transform.position += new Vector3(bumpAmount, 0, 0);
        }
        if (intensity < 0.0f && Owner.ColManager.WallSides[ColType.Left].Count > 0)
        {
            Owner.transform.position += new Vector3(-bumpAmount, 0, 0);
        }

        base.Sideways(intensity * Owner.PlayerProps.GroundPoundHorizontalMovementScale);
    }

    public override void HitFloor(Line line)
    {
        base.HitFloor(line);

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

        Owner.ChangeState(ScriptableObject.CreateInstance<LandingState>(), false);
    }
    public override void HitCeiling(Line line)
    {
        base.HitCeiling(line);
        Owner.Velocity.y = 0.0f;
    }
    public override void HitSide(Line line, ColType c)
    {
        base.HitSide(line, c);
        Owner.Velocity.x = 0.0f;
        Owner.MoveToSideOfSurface(line, c);
    }

    public override void AfterFixed()
    {
        //Update the correct timed stat.
        Owner.OwnerStats.TimeInAir += Time.fixedDeltaTime;

        //Check for continuous wall collisions.
        if (Owner.ColManager.WallSides[ColType.Left].Count > 0)
        {
            HitSide(Owner.ColManager.WallSides[ColType.Left][0], ColType.Left);
        }
        if (Owner.ColManager.WallSides[ColType.Right].Count > 0)
        {
            HitSide(Owner.ColManager.WallSides[ColType.Right][0], ColType.Right);
        }
    }

    public override string ToString() { return "Groundpounding"; }
}
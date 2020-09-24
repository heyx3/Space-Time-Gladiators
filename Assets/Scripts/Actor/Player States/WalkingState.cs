using UnityEngine;
using System.Collections;

public class WalkingState : ActorState
{
    float yPos;

    FadeLoopNoise runningNoise;

    public override Vector3 ApplyFriction(Vector3 original)
    {
        float newX = original.x;

        //If the player isn't accelerating sideways,
        //   or is trying to move in the opposite direction from his velocity,
        //   give him bigger friction.
        if (Owner.MovementKeyAxes.x == 0.0f ||
            StateMachine.Sign(Owner.MovementKeyAxes.x, 0.001f) == -StateMachine.Sign(Owner.Velocity.x, 0.001f))
        {
            newX *= Owner.PlayerProps.StoppingOnWallSlowdown;
        }
        else
        {
            newX *= Owner.PlayerProps.MovingTouchingWallSlowdown;
        }

        return new Vector3(newX, original.y, 0.0f);
    }

    public override void EnteringState()
    {
        Owner.Velocity.y = 0.0f;
        yPos = Owner.transform.position.y;

        SetAnim();

        RecBounds ab = Owner.ActorBounds;
        Owner.RunningParts.Position = new Vector3(ab.center.x, ab.bottom, Owner.RunningParts.Position.z);

        runningNoise = new FadeLoopNoise(WorldConstants.PlayPhysNoises.GetNoise(PlayerPhysicsNoises.Events.Run), "Running");
    }

    public override void ExitingState()
    {
        runningNoise.EndLoop();
        GameObject.Destroy(runningNoise.Loop);
    }

    public override void AnimationDone()
    {
        SetAnim();
    }
    private void SetAnim()
    {
        //If the player is moving slowly enough, he's standing.
        if (Mathf.Abs(Owner.Velocity.x) < WorldConstants.MinMovementSpeed)
        {
            Owner.Animator.CurrentAnimation = Animations.P_Stand;
        }
        //Otherwise, he's running.
        else
        {
            Owner.Animator.CurrentAnimation = Animations.P_Run;
        }
    }

    public override void Up()
    {
        Owner.ChangeState(ScriptableObject.CreateInstance<JumpingState>(), true);
    }

    public override void Sideways(float intensity)
    {
        //If the player is trying to move away from a wall, push him out just a bit.
        if (intensity > 0.0f && Owner.ColManager.WallSides[ColType.Right].Count > 0)
        {
            Vector3 newV = Owner.transform.position;
            newV.x += 0.01f;
            Owner.transform.position = newV;
        }
        if (intensity < 0.0f && Owner.ColManager.WallSides[ColType.Left].Count > 0)
        {
            Vector3 newV = Owner.transform.position;
            newV.x -= 0.01f;
            Owner.transform.position = newV;
        }

        //Emit running particles.
        if (!StateMachine.WithinError(intensity, 0.0f, 0.01f) &&
            Random.value < Owner.PlayerProps.RunningDirtEmitChance)
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

    private bool dontPlayEffects = false;
    public override void HitSide(Line line, ColType c)
    {
        if (!dontPlayEffects)
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

        base.HitSide(line, c);
        Owner.Velocity.x = 0.0f;
        Owner.MoveToSideOfSurface(line, c);
    }

    public override void AfterMove()
    {
        Owner.transform.position = new Vector3(Owner.transform.position.x, yPos, Owner.transform.position.z);
        if (Owner.Velocity.x == 0.0f && Owner.Animator.CurrentAnimation == Animations.P_Run)
        {
            Owner.Animator.CurrentAnimation = Animations.P_Stand;
        }
    }

    public override void AfterFixed()
    {
        //Update running sound.

        if (!StateMachine.WithinError(Owner.Velocity.x, 0.0f, WorldConstants.MinMovementSpeed))
        {
            runningNoise.StartLoop();
        }
        else
        {
            runningNoise.EndLoop();
        }

        if (runningNoise.Running)
        {
            runningNoise.UpdateLoop();
        }


        //Check current wall collisions.

        if (Owner.ColManager.WallSides[ColType.Top].Count == 0)
        {
            Owner.ChangeState(ScriptableObject.CreateInstance<InAirState>(), false);
        }

        if (Owner.ColManager.WallSides[ColType.Left].Count > 0)
        {
            dontPlayEffects = true;
            HitSide(Owner.ColManager.WallSides[ColType.Left][0], ColType.Left);
            dontPlayEffects = false;
        }

        if (Owner.ColManager.WallSides[ColType.Right].Count > 0)
        {
            dontPlayEffects = true;
            HitSide(Owner.ColManager.WallSides[ColType.Right][0], ColType.Right);
            dontPlayEffects = false;
        }


        //Update the correct timing stat.
        Owner.OwnerStats.TimeOnGround += Time.fixedDeltaTime;
    }

    public override string ToString()
    {
        return Owner.Velocity.x == 0.0f ? "Standing" : "Walking";
    }
}
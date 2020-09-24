using UnityEngine;
using System.Collections;

public class InAirState : ActorState
{
	private float elapsed;
	
    public override Vector3 ApplyFriction(Vector3 original)
    {
        return new Vector3(original.x * Owner.PlayerProps.InAirXSlowdown, original.y, 0.0f);
    }

	public override void EnteringState ()
    {
        elapsed = Owner.PlayerProps.WallJumpAccelerationDimTime;
		
		SetAnim();
	}
	
	private double TimeToApex
    {
		get {
			return -Owner.Velocity.y / Gravity.y;
		}
	}
	private void SetAnim()
    {
        if (Owner.Velocity.y > 0)
        {
            if (TimeToApex <= Owner.PlayerProps.TimeTillApexTransition)
            {
                Owner.Animator.CurrentAnimation = Animations.P_AirUpToDown;
            }
            else
            {
                Owner.Animator.CurrentAnimation = Animations.P_AirUp;
            }
        }
        else
        {
            Owner.Animator.CurrentAnimation = Animations.P_AirDown;
        }
	}
	
	public override void AnimationDone ()
    {
		SetAnim();
	}
	
	public void DimAcceleration() { elapsed = 0.0f; }
	
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
	
	public override void Sideways (float intensity)
	{
		//If coming off a wall-jump, dim the acceleration.
        if (elapsed < Owner.PlayerProps.WallJumpAccelerationDimTime)
        {
            base.Sideways(intensity * Owner.PlayerProps.WallJumpAccelerationDimScale);
        }
        //Otherwise, behave like normal.
        else
        {
            base.Sideways(intensity);
        }
	}
	
	public override void Down ()
    {
        //Play the sound and make particles.
        WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.GroundPound);
        WorldConstants.Creator.CreateGroundpoundParticles(Owner);

		Owner.ChangeState(ScriptableObject.CreateInstance<GroundPoundState>(), false);
	}

    public override void HitCeiling(Line line)
	{
        base.HitCeiling(line);

        WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.SoftLand);

		Owner.MoveAboveOrBelowSurface(line, ColType.Bottom);
		
        if (Mathf.Abs(Owner.Velocity.y) > Owner.PlayerProps.MinimumFallSpeed)
        {
            Owner.HardLanding.Position = new Vector3(Owner.ActorBounds.center.x, Owner.ActorBounds.top, Owner.HardLanding.Position.z);
            Owner.HardLanding.Emit();
            WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.HeavyLand);
        }
        else
        {
            Owner.SoftLanding.Position = new Vector3(Owner.ActorBounds.center.x, Owner.ActorBounds.top, Owner.SoftLanding.Position.z);
            Owner.SoftLanding.Emit();
            WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.SoftLand);
        }

        CeilingSlideState c = ScriptableObject.CreateInstance<CeilingSlideState>();
        c.SetTimingData(Owner.Velocity.y, WorldConstants.DownGravity);

		Owner.ChangeState (c, false);
	}
    public override void HitSide(Line line, ColType c)
    {
        base.HitSide(line, c);

        WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.SoftLand);

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

		WallSlideState ws = ScriptableObject.CreateInstance<WallSlideState>();
        ws.JumpDir = c == ColType.Left ? (sbyte)-1 : (sbyte)1;
		
		Owner.ChangeState (ws, false);
	}
    public override void HitFloor(Line line)
	{
        base.HitFloor(line);

        if (Mathf.Abs(Owner.Velocity.y) > Owner.PlayerProps.MinimumFallSpeed)
        {
            Owner.HardLanding.Position = new Vector3(Owner.ActorBounds.center.x, Owner.ActorBounds.bottom, Owner.HardLanding.Position.z);
            Owner.HardLanding.Emit();
            WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.HeavyLand);
            Owner.ChangeState(ScriptableObject.CreateInstance<LandingState>(), false);
        }
        else
        {
            Owner.SoftLanding.Position = new Vector3(Owner.ActorBounds.center.x, Owner.ActorBounds.bottom, Owner.SoftLanding.Position.z);
            Owner.SoftLanding.Emit();
            WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.SoftLand);
            Owner.ChangeState(ScriptableObject.CreateInstance<WalkingState>(), false);
        }
	}
	
	public override void Update ()
    {
		elapsed += Time.deltaTime;
	}
	
	//Skip an update cycle to give the collision engine time to update.
	private bool skipUpdate = true;
    public override void AfterFixed()
    {
		if (skipUpdate)
		{
			skipUpdate = false;
			return;
		}
		
        //Update the correct timed stat.
        Owner.OwnerStats.TimeInAir += Time.fixedDeltaTime;

        //If there are any walls touching the player, raise the correct event.
        if (Owner.ColManager == null)
        {
            return;
        }

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
	
	public override string ToString ()
	{
		return "In the air";
	}
}
using System;
using UnityEngine;

/// <summary>
/// The set of physics data for a Camera.
/// </summary>
[System.Serializable]
public class CameraPhysicsData
{
    public static CameraPhysicsData DefaultValues
    {
        get
        {
            return new CameraPhysicsData()
            {
                SnapToPlayerMaxDist = 10.0f,
				YSpeedDamp = 0.4f,
                Speed = DefaultCamMaxSpeedFunction,
                Acceleration = DefaultCamAccelFunction,
            };
        }
    }

    /// <summary>
    /// If this Camera is within this distance of the target position, it will snap to it.
    /// </summary>
    public float SnapToPlayerMaxDist;
	
	public float YSpeedDamp;
	
    /// <summary>
    /// Gets the Speed of the given camera.
    /// </summary>
    public System.Func<CameraFollowScript, float> Speed;
    /// <summary>
    /// Gets the maximum speed of the given camera.
    /// Default: returns four times the distance between the camera and its target.
    /// </summary>
    public static float DefaultCamMaxSpeedFunction(CameraFollowScript c)
    {
        return 4.0f * Vector2.Distance(c.ThisTransform.position, c.TargetPos);
    }

    /// <summary>
    /// Gets the Acceleration of the given camera.
    /// </summary>
    public System.Func<CameraFollowScript, float> Acceleration;
    /// <summary>
    /// Gets the maximum acceleration of the given camera.
    /// Default behavior: returns a flat acceleration of 2,000.
    /// </summary>
    public static float DefaultCamAccelFunction(CameraFollowScript c)
    {
        return 10;
    }

}

/// <summary>
/// Represents properties of a camera's shaking.
/// </summary>
[System.Serializable]
public class CameraShakeData
{
    public static CameraShakeData DefaultData
    {
        get
        {
            return new CameraShakeData()
            {
                TimeVariance = 18.0f,
                FadeSpeed = 110.0f,
                TimeScale = 300.0f,
                TimeYOffset = 100.0f,
            };
        }
    }

    /// <summary>
    /// Random variance in the shaking.
    /// </summary>
    public float TimeVariance;
    /// <summary>
    /// The rate of fluctuation of the shaking.
    /// </summary>
    public float TimeScale;
    /// <summary>
    /// An offset between the X and Y values for shaking; so the shaking won't appear as a regular circle.
    /// </summary>
    public float TimeYOffset;
    /// <summary>
    /// The rate of fade of the shaking strength per second.
    /// </summary>
    public float FadeSpeed;
}

///<summary>
/// Changes to the camera's behavior.
///</summary>
[System.Serializable]
public class CameraBehaviorData
{
	public static CameraBehaviorData DefaultValues
	{
		get
		{
			return new CameraBehaviorData();
		}
	}
	
	public float SpeedMultiplier = 1.0f;
	
	public bool ClampToPlayer = false;
	public bool GetMaxVelocityEveryStep = false;
	
	public bool UseStaticAccel = false;
	public float StaticAccel = 50.0f;
	
	public bool SnapToPlayer = true;
}
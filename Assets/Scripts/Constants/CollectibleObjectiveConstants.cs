using UnityEngine;
using System.Collections;

/// <summary>
/// Holes constant values relating to collectibles or objectives.
/// </summary>
public class CollectibleObjectiveConstants : MonoBehaviour
{
	//Powerup effects.

	public float PowerupGiveMultiplierIncrement = 0.03f;
	
	public Vector3 PowerupHurtOthersTempVel = new Vector3(20, 20, 0.0f);
	public double PowerupHurtOtherExtraHurtTime = 10.0f;

    public float PowerupJumbleControlsTime = 15.0f;

    public float PowerupHideWallsTime = 10.0f;

    public float PowerupDisableCollisionTime = 15.0f;

    [SerializeField]
    private GameObject powerupSounds;
    public ControlledNoise PowerupSoundInstance()
    {
        return ((GameObject)Instantiate(powerupSounds)).GetComponent<ControlledNoise>();
    }
    public int PowerupPositiveNoiseIndex = 0;

	//Flag.
	public Vector2 FlagOffsetFromCarrier = new Vector2(0.0f, 0.2f);
	public Vector2 FlagOffsetFromBase = new Vector2(0.0f, 0.0f);

    void Awake()
    {
        WorldConstants.CollObjConsts = this;
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


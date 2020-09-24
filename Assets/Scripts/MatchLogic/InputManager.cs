using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Manages player input, including the ability to temporarily scramble a player's controls.
/// </summary>
public class InputManager : MonoBehaviour
{
    const byte numbInputs = 7;
    [SerializeField]
    private Vector3[] inputs;

    private Dictionary<byte, byte> playerIDToInputID;

    //Jumbling inputs.
    private Dictionary<byte, float> playerJumbleTime;
    private Dictionary<byte, Quaternion> playerJumbleRotation;

    public bool DisableInput = false;

    void Awake()
    {
        inputs = new Vector3[numbInputs];

        for (int i = 0; i < numbInputs; ++i)
            inputs[i] = Vector2.zero;

        playerIDToInputID = new Dictionary<byte, byte>();
        playerJumbleTime = new Dictionary<byte, float>();
        playerJumbleRotation = new Dictionary<byte, Quaternion>();
    }

    void Update()
    {
        //Get the inputs. Don't forget that the inputs start at 1, not 0!
        for (int i = 1; i <= numbInputs; ++i)
        {
            if (DisableInput)
            {
                inputs[i - 1] = Vector3.zero;
            }
            else
            {
                inputs[i - 1] = new Vector3(Input.GetAxis("Horizontal " + i.ToString()),
                                            Input.GetAxis("Vertical " + i.ToString()),
                                            0.0f);
            }
        }

        //Update jumbled controls.
		List<byte> keys = playerJumbleTime.Keys.ToList ();
        foreach (byte id in keys)
        {
            if (playerJumbleTime[id] <= 0.0f)
            {
                continue;
            }

            playerJumbleTime[id] -= Time.deltaTime;

            if (playerJumbleTime[id] <= 0.0f)
            {
                //Get the player.
                StateMachine player = null;
                foreach (StateMachine tempPlayer in WorldConstants.ColTracker.Actors)
                {
                    if (tempPlayer.ActorData.PlayerID == id)
                    {
                        player = tempPlayer;
                        break;
                    }
                }

                WorldConstants.Creator.CreatePowerupFloatingText(WorldConstants.ActorConsts.PowerupFinishedConfusingEnemiesMessages.Trigger, player.transform.position, player.transform, true);

                //Un-jumble his controls.
                playerJumbleTime[id] = 0.0f;
                playerJumbleRotation[id] = Quaternion.AngleAxis(0.0f, Vector3.forward);
            }
        }
    }

    /// <summary>
    /// Jumbles the given player's input.
    /// </summary>
    /// <param name="playerID">The player to jumble.</param>
    /// <param name="jumbleTime">The length of time to jumble him for (stacks onto any previous jumbling).</param>
    public void JumbleInput(byte playerID, float jumbleTime)
    {
        playerJumbleRotation[playerID] = Quaternion.AngleAxis(360 * UnityEngine.Random.value,
                                                              new Vector3(0.0f, 0.0f, 1.0f));
        playerJumbleTime[playerID] += jumbleTime;
    }

    /// <summary>
    /// Finds the first available input ID for a new player to use, or Byte.MaxValue if none exists.
    /// </summary>
    public byte FindFirstAvailableInput()
    {
        for (byte i = 0; i < numbInputs; ++i)
            if (!playerIDToInputID.Values.Contains(i))
                return i;

        return Byte.MaxValue;
    }

    /// <summary>
    /// Binds a player to a specific input.
    /// </summary>
    /// <returns>True if it was successful, false if another player already uses the given input ID.</returns>
    public bool RegisterPlayerInput(byte playerID, byte inputID)
    {
        if (playerIDToInputID.ContainsKey(playerID))
            return false;

        playerIDToInputID.Add(playerID, inputID);
        playerJumbleTime.Add(playerID, 0.0f);
        playerJumbleRotation.Add(playerID, Quaternion.AngleAxis(0.0f, Vector3.forward));
        
        return true;
    }
    /// <summary>
    /// Unbinds the given player from his input ID.
    /// </summary>
    /// <returns>Whether or not the given player ID was actually bound to something before being removed.</returns>
    public bool UnregisterPlayerInput(byte playerID)
    {
        bool b = playerIDToInputID.Remove(playerID);

        playerJumbleTime.Remove(playerID);
        playerJumbleRotation.Remove(playerID);

        return b;
    }

    /// <summary>
    /// Gets the current input for the given player.
    /// </summary>
    public Vector2 GetInput(byte playerIndex)
    {
        if (!playerIDToInputID.ContainsKey(playerIndex))
            throw new ArgumentException("The given player index isn't registered with this InputManager!");
		
		List<byte> inputIDs = playerIDToInputID.Values.ToList();
		
		if (inputs[playerIDToInputID[playerIndex] - 1] == Vector3.zero) return Vector2.zero;
		
        //Jumble the player's controls.
        return playerJumbleRotation[playerIndex] * inputs[playerIDToInputID[playerIndex] - 1];
    }
}

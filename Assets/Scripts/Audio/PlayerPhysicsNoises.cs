using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPhysicsNoises : MonoBehaviour
{
    public GameObject[] Noises = new GameObject[(int)Events.NumberOfEvents];
    public Events[] NoiseEvents = { Events.Run, Events.Jump, Events.HeavyLand, Events.SoftLand, Events.Slide, Events.SlidePushOff, Events.GroundPound, Events.Hurt, Events.GrabItem, Events.StartBurst };

    //TODO: Hook these events. The ones that haven't been done yet are marked with an empty comment.

    /// <summary>
    /// The different things a player can do (or can happen to him) that create physical noise.
    /// </summary>
    public enum Events
    {
        Run = 0,
        Jump,
        HeavyLand,
        SoftLand,
        Slide,
        SlidePushOff,
        GroundPound,
        Hurt,
        GrabItem, //
        StartBurst, //

        NumberOfEvents,
    }

    public Dictionary<Events, GameObject> eventsToNoises;

    void Awake()
    {
        //Build the sound effect dictionary.

        //Error-checking.
        if (Noises.Length != NoiseEvents.Length)
        {
            throw new System.ArgumentException("Noise array and Noise Events array don't line up!");
        }
        if (NoiseEvents.Length != (int)Events.NumberOfEvents)
        {
            throw new System.InvalidOperationException("NoiseEvents array must not be changed!");
        }

        //Go through and create each key/value pair.
        eventsToNoises = new Dictionary<Events, GameObject>();
        Events e;
        for (int i = 0; i < NoiseEvents.Length; ++i)
        {
            e = NoiseEvents[i];

            if (e == Events.NumberOfEvents || eventsToNoises.ContainsKey(e))
            {
                throw new System.InvalidOperationException("NoiseEvents array must not be changed!");
            }

            if (Noises[i] == null)
            {
                continue;
            }

            eventsToNoises.Add(e, Noises[i]);
        }
    }

    /// <summary>
    /// Gets the correct noise object for the given event, or "null" if there is no noise for the event.
    /// </summary>
    public GameObject GetNoise(Events noiseEvent)
    {
        if (eventsToNoises.ContainsKey(noiseEvent) && eventsToNoises[noiseEvent] != null)
        {
            return (GameObject)Instantiate(eventsToNoises[noiseEvent]);
        }

        return null;
    }
    /// <summary>
    /// Plays an appropriate sound effect for the given player action, if one exists.
    /// </summary>
    public void PlayNoise(Events noiseEvent)
    {
        GameObject g = GetNoise(noiseEvent);

        if (g != null)
        {
            g.GetComponent<ControlledNoise>().StartClip();
        }
    }
}
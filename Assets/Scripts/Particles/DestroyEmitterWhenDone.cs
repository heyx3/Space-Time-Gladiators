using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys itself when the particles are done emitting.
/// </summary>
public class DestroyEmitterWhenDone : MonoBehaviour
{
    private ParticleHandler handle;

    void Awake()
    {
        ParticleEmitter em = particleEmitter;
        ParticleSystem sys = particleSystem;

        if (em != null)
        {
            handle = new ParticleHandler(ParticleHandler.ParticleSystems.Legacy, new GameObject[] { gameObject }, gameObject);
        }
        if (sys != null)
        {
            handle = new ParticleHandler(ParticleHandler.ParticleSystems.Shruiken, new GameObject[] { gameObject }, gameObject);
        }
    }

    void Start()
    {
        WorldConstants.Creator.AddTimer(new Timer(WaitSeconds, false, CheckDestroy), true);
    }

    private const float WaitSeconds = 1.0f;
    void CheckDestroy(float elapsedSeconds)
    {
        if (handle.NumbParticles == 0)
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
            WorldConstants.Creator.AddTimer(new Timer(WaitSeconds, false, CheckDestroy), true);
        }
    }
}
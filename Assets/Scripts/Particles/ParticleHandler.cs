using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Wraps usage of particle effects. Abstracts out
/// things such as what system is being used
/// (Shruiken or Legacy).
/// </summary>
public class ParticleHandler
{
    private abstract class Handler
    {
        public Handler(GameObject[] particles, Transform center)
        {
			this.center = center;
            StoreParticles(particles);
        }
        protected abstract void StoreParticles(GameObject[] particles);

        private Transform center;
        public Vector3 Position { get { return center.position; } set { SetPosition(value); } }
        protected abstract void SetPosition(Vector3 newPos);

        public abstract bool Emitting { get; set; }
        public abstract Vector3 WorldVelocity { get; set; }

        public abstract int NumbParticles { get; }

        public abstract void Emit(int? count);
    }

    #region Shruiken particle handler

    private class ShruikenHandler : Handler
    {
        private ParticleSystem[] particles;

        public ShruikenHandler(GameObject[] particles, Transform center)
            : base(particles, center) { }

        protected override void StoreParticles(GameObject[] particles)
        {
            this.particles = new ParticleSystem[particles.Length];
            for (int i = 0; i < particles.Length; ++i)
            {
                this.particles[i] = particles[i].particleSystem;
            }
        }

        public override bool Emitting
        {
            get
            {
                return particles[0].enableEmission;
            }
            set
            {
                for (int i = 0; i < particles.Length; ++i)
                {
                    particles[i].enableEmission = value;
                }
            }
        }
        public override Vector3 WorldVelocity
        {
            get
            {
                throw new InvalidOperationException("Can't access \"world velocity\" on Shruiken particles!");
            }
            set
            {
                throw new InvalidOperationException("Can't set \"world velocity\" on Shruiken particles!");
            }
        }

        public override int NumbParticles { get { return particles.Sum(p => p.particleCount); } }

        public override void Emit(int? count)
        {
            for (int i = 0; i < particles.Length; ++i)
            {
                particles[i].Emit(count.Value);
            }
        }
        protected override void SetPosition(Vector3 newPos)
        {
            Vector3 delta = newPos - Position;
            for (int i = 0; i < particles.Length; ++i)
            {
                particles[i].transform.position += delta;
            }
        }
    }

    #endregion

    #region Legacy particle handler

    private class LegacyHandler : Handler
    {
        private ParticleEmitter[] particles;

        public LegacyHandler(GameObject[] particles, Transform center)
            : base(particles, center) { }

        protected override void StoreParticles(GameObject[] particles)
        {
            this.particles = new ParticleEmitter[particles.Length];
            for (int i = 0; i < particles.Length; ++i)
            {
                this.particles[i] = particles[i].particleEmitter;
            }
        }

        public override bool Emitting
        {
            get
            {
                return particles[0].emit;
            }
            set
            {
                for (int i = 0; i < particles.Length; ++i)
                {
                    particles[i].emit = value;
                }
            }
        }
        public override Vector3 WorldVelocity
        {
            get
            {
                return particles[0].worldVelocity;
            }
            set
            {
                for (int i = 0; i < particles.Length; ++i)
                {
                    particles[i].worldVelocity = value;
                }
            }
        }

        public override int NumbParticles { get { return particles.Sum(p => p.particleCount); } }

        public override void Emit(int? count)
        {
            for (int i = 0; i < particles.Length; ++i)
            {
                if (count.HasValue)
                {
                    particles[i].Emit(count.Value);
                }
                else
                {
                    particles[i].Emit();
                }
            }
        }
        protected override void SetPosition(Vector3 newPos)
        {
            Vector3 delta = newPos - Position;
            for (int i = 0; i < particles.Length; ++i)
            {
                particles[i].transform.position += delta;
            }
        }
    }

    #endregion

    /// <summary>
    /// The different particle systems that could be used.
    /// </summary>
    public enum ParticleSystems
    {
        Legacy,
        Shruiken,
    }

    private Handler handler;

    public ParticleHandler(ParticleSystems system, IEnumerable<GameObject> particles, GameObject center)
    {
		GameObject[] particles2 = particles.ToArray ();
		
        switch (system)
        {
            case ParticleSystems.Legacy:
                handler = new LegacyHandler(particles.ToArray(), center.transform);
                break;

            case ParticleSystems.Shruiken:
                handler = new ShruikenHandler(particles.ToArray(), center.transform);
                break;

            default: throw new NotImplementedException();
        }
    }

    public Vector3 Position { get { return handler.Position; } set { handler.Position = value; } }
    public bool Emitting { get { return handler.Emitting; } set { handler.Emitting = value; } }
    public Vector3 WorldVelocity { get { return handler.WorldVelocity; } set { handler.WorldVelocity = value; } }
    public int NumbParticles { get { return handler.NumbParticles; } }

    /// <summary>
    /// Emits particles.
    /// </summary>
    /// <param name="count">The number of particles to emit, or "null" if the argument shouldn't be passed in.</param>
    public void Emit(int? count = null)
    {
        handler.Emit(count);
    }
}
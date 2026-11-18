using UnityEngine;

namespace Base.UIPackage.Effects
{
    /// <summary>
    /// One sprite of a <see cref="UIParticleField"/>. A plain struct in a flat array, with open fields so the loop
    /// can change it in place, because a field of hundreds has to cost no allocations and no GameObjects.
    /// </summary>
    internal struct UIParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color32 Tint;
        public float Age;
        public float Lifetime;
        public float StartSize;
        public float EndSize;
        public float Rotation;
        public float Spin;
        public float SwayPhase;
    }
}
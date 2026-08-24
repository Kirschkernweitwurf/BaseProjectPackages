namespace Base.CorePackage.Noise
{
    /// <summary>
    /// How a raw noise sample is shaped before the layers are added up. Every option stays inside
    /// the same output range, so switching between them changes the character and not the scale.
    /// </summary>
    public enum ENoiseType : byte
    {
        /// <summary>Plain gradient noise. Soft rolling hills and clouds.</summary>
        Perlin = 0,

        /// <summary>Folded and inverted, turning the middle of the range into a crest. Mountain ridges.</summary>
        Ridged = 1,

        /// <summary>Folded, turning the middle of the range into a crease. Smoke, marble and erosion.</summary>
        Turbulence = 2
    }
}
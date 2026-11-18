using UnityEngine;
using UnityEngine.UI;

namespace Base.UIPackage.Effects
{
    /// <summary>
    /// One live sparkle of <see cref="UISparkles"/>: where it flies, how big it gets, how fast it spins and how far
    /// through its life it is. A twinkle stays put and grows and fades; a burst sparkle flies out and shrinks.
    /// </summary>
    internal struct SparkleParticle
    {
        /// <summary>The pooled image that shows this sparkle.</summary>
        public Image Image { get; set; }

        /// <summary>Where the sparkle starts, in the area's local space.</summary>
        public Vector2 From { get; set; }

        /// <summary>Where the sparkle ends. Same as <see cref="From"/> for a twinkle.</summary>
        public Vector2 To { get; set; }

        /// <summary>Scale at the largest point of the sparkle's life.</summary>
        public float Scale { get; set; }

        /// <summary>Rotation in degrees at the start.</summary>
        public float StartAngle { get; set; }

        /// <summary>Degrees the sparkle turns over its whole life.</summary>
        public float Spin { get; set; }

        /// <summary>Seconds lived so far. Negative while a twinkle still waits to appear.</summary>
        public float Age { get; set; }

        /// <summary>Seconds the sparkle lives.</summary>
        public float Lifetime { get; set; }

        /// <summary>Whether this is a twinkle that respawns, rather than a burst sparkle that ends.</summary>
        public bool IsTwinkle { get; set; }
    }
}
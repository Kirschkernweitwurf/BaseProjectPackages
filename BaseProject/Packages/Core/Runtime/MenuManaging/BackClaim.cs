using System;
using UnityEngine;

namespace Base.CorePackage.MenuManaging
{
    /// <summary>A claim on Back made through <see cref="MenuManager.ClaimBack"/>.</summary>
    internal readonly struct BackClaim
    {
        /// <summary>Who claimed Back. Its place on screen decides when the claim acts.</summary>
        public readonly Component Owner;

        /// <summary>Runs on Back while the claim acts.</summary>
        public readonly Action OnBack;

        public BackClaim(Component owner, Action onBack)
        {
            Owner = owner;
            OnBack = onBack;
        }
    }
}
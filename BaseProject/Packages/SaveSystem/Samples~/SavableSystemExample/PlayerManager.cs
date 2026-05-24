using System.Collections.Generic;
using UnityEngine;

namespace Base.SaveSystemPackage.Example
{
    public abstract class PlayerManager
    {
        public int Level = 1;
        public float Health = 100f;
        public Vector3 Position;
        public List<string> Inventory = new();
    }
}
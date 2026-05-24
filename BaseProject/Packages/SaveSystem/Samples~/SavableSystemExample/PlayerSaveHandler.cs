using System;
using System.Collections.Generic;
using Base.SaveSystemPackage.Savable;
using Base.SystemsCorePackage.Tracking;
using UnityEngine;

namespace Base.SaveSystemPackage.Example
{
    /// <summary>
    /// The decoupled save handler. It knows how to read/write the PlayerManager's
    /// state, registers itself, and owns its own JSON format. The manager stays clean.
    ///
    /// You would new this up once (e.g. in your bootstrap) and keep it alive.
    /// </summary>
    public sealed class PlayerSaveHandler : MonoBehaviour, ISavable, IDisposable
    {
        // The shape we actually serialize. Separate from the manager so the save
        // format can evolve without touching gameplay code.
        [Serializable]
        private struct State
        {
            public int level;
            public float health;
            public Vector3 position;
            public List<string> inventory;
        }

        private readonly PlayerManager _player;

        public string SaveId => "player";          // stable key, never change once shipped
        public EPriority Priority => EPriority.High; // loads/saves before lower-priority systems

        public PlayerSaveHandler(PlayerManager player)
        {
            _player = player;
            SavableRegistry.Register(this);
        }

        public void Dispose() => SavableRegistry.Deregister(this);

        public string Serialize()
        {
            State state = new()
            {
                level = _player.Level,
                health = _player.Health,
                position = _player.Position,
                inventory = _player.Inventory
            };
            return JsonUtility.ToJson(state);
        }

        public void Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
                return; // no saved data for the player yet -> keep defaults

            State state = JsonUtility.FromJson<State>(json);
            _player.Level = state.level;
            _player.Health = state.health;
            _player.Position = state.position;
            _player.Inventory = state.inventory ?? new List<string>();
        }
    }
}
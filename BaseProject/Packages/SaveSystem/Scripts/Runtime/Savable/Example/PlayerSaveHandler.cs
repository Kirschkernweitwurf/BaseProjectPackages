using System;
using System.Collections.Generic;
using Base.SaveSystemPackage.Unity.Composition;
using Base.SystemsCorePackage.Services;
using Base.SystemsCorePackage.Tracking;
using Base.UtilityPackage.Identification;
using UnityEngine;

namespace Base.SaveSystemPackage.Savable.Example
{
    /// <summary>
    /// Example save handler for <see cref="PlayerManager"/>. Owns the player's save format and maps
    /// between the manager and that format, keeping gameplay code free of persistence concerns.
    /// Resolves the savable registry from the <see cref="SaveManager"/> service and registers
    /// itself for its lifetime.
    /// </summary>
    public sealed class PlayerSaveHandler : MonoBehaviour, ISavable
    {
        [Serializable]
        private struct State
        {
            public int level;
            public float health;
            public Vector3 position;
            public List<string> inventory;
        }

        public PersistentKey PersistentKey => Key;

        public EPriority Priority => EPriority.High;

        private static readonly PersistentKey Key = new("player");

        private PlayerManager _player;
        private ISavableRegistry _registry;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out SaveManager saveManager))
                return;

            _player = new PlayerManager();
            _registry = saveManager.Savables;
            _registry.Register(this);
        }

        private void OnDestroy() => _registry?.Deregister(this);

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
                return;

            State state = JsonUtility.FromJson<State>(json);
            _player.Level = state.level;
            _player.Health = state.health;
            _player.Position = state.position;
            _player.Inventory = state.inventory ?? new List<string>();
        }
    }
}
using Base.ControllerSupportPackage.InputPrompts.Glyphs;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.ControllerSupportPackage.Tests
{
    /// <summary>Covers the glyphs of rebound actions: the new control's sprite wins, an unknown one keeps the entry.</summary>
    public sealed class InputGlyphSetTests
    {
        private const string BoundPath = "<Keyboard>/f8";
        private const string ReboundPath = "<Keyboard>/f9";

        private InputGlyphSet _set;
        private InputActionAsset _asset;
        private InputActionReference _reference;
        private Texture2D _texture;
        private Sprite _sprite;

        [SetUp]
        public void SetUp()
        {
            _texture = new Texture2D(width: 1, height: 1);
            _sprite = Sprite.Create(_texture, new Rect(0f, 0f, 1f, 1f), Vector2.zero);

            _asset = ScriptableObject.CreateInstance<InputActionAsset>();
            InputAction action = _asset.AddActionMap("Map").AddAction("Action");
            action.AddBinding(BoundPath);
            _reference = InputActionReference.Create(action);

            _set = ScriptableObject.CreateInstance<InputGlyphSet>();
            SerializedObject serialized = new(_set);
            SerializedProperty glyphs = serialized.FindProperty("controlGlyphs");
            glyphs.arraySize = 1;
            SerializedProperty glyph = glyphs.GetArrayElementAtIndex(0);
            glyph.FindPropertyRelative("<Path>k__BackingField").stringValue = ReboundPath;
            glyph.FindPropertyRelative("<Sprite>k__BackingField").objectReferenceValue = _sprite;
            serialized.FindProperty("<DeviceType>k__BackingField").enumValueIndex = 1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_set);
            Object.DestroyImmediate(_reference);
            Object.DestroyImmediate(_asset);
            Object.DestroyImmediate(_sprite);
            Object.DestroyImmediate(_texture);
        }

        [Test]
        public void TryGetControlSprite_FindsAControlOfTheSet()
        {
            Assert.That(_set.TryGetControlSprite(ReboundPath, out Sprite sprite), Is.True);
            Assert.That(sprite, Is.EqualTo(_sprite));
        }

        [Test]
        public void TryGetControlSprite_MissesOtherControls()
        {
            Assert.That(_set.TryGetControlSprite(BoundPath, out _), Is.False);
        }

        [Test]
        public void TryGetSprite_ShowsTheNewControlOfAReboundAction()
        {
            _reference.action.ApplyBindingOverride(0, ReboundPath);

            Assert.That(_set.TryGetSprite(_reference, out Sprite sprite), Is.True);
            Assert.That(sprite, Is.EqualTo(_sprite));
        }
    }
}
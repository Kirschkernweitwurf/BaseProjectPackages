using Unity.Cinemachine;
using UnityEditor;

namespace Base.ScreenShakePackage.Editor
{
    /// <summary>
    /// Custom editor for <see cref="ScreenShakeProfile"/> that hides or shows certain fields based on the settings.
    /// </summary>
    [CustomEditor(typeof(ScreenShakeProfile))]
    public class ScreenShakeProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty _impactForce;
        private SerializedProperty _impulseDuration;
        private SerializedProperty _impulseType;
        private SerializedProperty _impulseShape;
        private SerializedProperty _customImpulseShape;
        private SerializedProperty _defaultVelocity;
        private SerializedProperty _listenerAmplitude;
        private SerializedProperty _listenerFrequency;
        private SerializedProperty _listenerDuration;

#region Unity Callbacks
        private void OnEnable()
        {
            _impactForce = FindBackingField(nameof(ScreenShakeProfile.ImpactForce));
            _impulseDuration = FindBackingField(nameof(ScreenShakeProfile.ImpulseDuration));
            _impulseType = FindBackingField(nameof(ScreenShakeProfile.ImpulseType));
            _impulseShape = FindBackingField(nameof(ScreenShakeProfile.ImpulseShape));
            _customImpulseShape = FindBackingField(nameof(ScreenShakeProfile.CustomImpulseShape));
            _defaultVelocity = FindBackingField(nameof(ScreenShakeProfile.DefaultVelocity));
            _listenerAmplitude = FindBackingField(nameof(ScreenShakeProfile.ListenerAmplitude));
            _listenerFrequency = FindBackingField(nameof(ScreenShakeProfile.ListenerFrequency));
            _listenerDuration = FindBackingField(nameof(ScreenShakeProfile.ListenerDuration));
        }
#endregion

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Shake Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_impactForce);
            EditorGUILayout.PropertyField(_impulseDuration);
            EditorGUILayout.PropertyField(_impulseType);
            EditorGUILayout.PropertyField(_impulseShape);

            CinemachineImpulseDefinition.ImpulseShapes shapeValue =
                (CinemachineImpulseDefinition.ImpulseShapes)_impulseShape.enumValueIndex;

            if (shapeValue == CinemachineImpulseDefinition.ImpulseShapes.Custom)
                EditorGUILayout.PropertyField(_customImpulseShape);

            EditorGUILayout.PropertyField(_defaultVelocity);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Listener Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_listenerAmplitude);
            EditorGUILayout.PropertyField(_listenerFrequency);
            EditorGUILayout.PropertyField(_listenerDuration);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Finds the auto-property backing field for the given property name.
        /// </summary>
        private SerializedProperty FindBackingField(string propertyName) =>
            serializedObject.FindProperty($"<{propertyName}>k__BackingField");
    }
}
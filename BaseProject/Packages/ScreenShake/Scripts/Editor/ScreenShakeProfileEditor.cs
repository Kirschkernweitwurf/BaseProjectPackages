#if UNITY_EDITOR
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
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Cache all serialized properties
            SerializedProperty impactForce = serializedObject.FindProperty("<ImpactForce>k__BackingField");
            SerializedProperty impulseDuration = serializedObject.FindProperty("<ImpulseDuration>k__BackingField");
            SerializedProperty impulseType = serializedObject.FindProperty("<ImpulseType>k__BackingField");
            SerializedProperty impulseShape = serializedObject.FindProperty("<ImpulseShape>k__BackingField");
            SerializedProperty customImpulseShape =
                serializedObject.FindProperty("<CustomImpulseShape>k__BackingField");

            SerializedProperty defaultVelocity = serializedObject.FindProperty("<DefaultVelocity>k__BackingField");
            SerializedProperty listenerAmplitude = serializedObject.FindProperty("<ListenerAmplitude>k__BackingField");
            SerializedProperty listenerFrequency = serializedObject.FindProperty("<ListenerFrequency>k__BackingField");
            SerializedProperty listenerDuration = serializedObject.FindProperty("<ListenerDuration>k__BackingField");

            // Draw properties
            EditorGUILayout.LabelField("Shake Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(impactForce);
            EditorGUILayout.PropertyField(impulseDuration);
            EditorGUILayout.PropertyField(impulseType);
            EditorGUILayout.PropertyField(impulseShape);

            // Only show custom curve if shape == Custom
            CinemachineImpulseDefinition.ImpulseShapes shapeValue =
                (CinemachineImpulseDefinition.ImpulseShapes)impulseShape.enumValueIndex;

            if (shapeValue == CinemachineImpulseDefinition.ImpulseShapes.Custom)
                EditorGUILayout.PropertyField(customImpulseShape);

            EditorGUILayout.PropertyField(defaultVelocity);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Listener Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(listenerAmplitude);
            EditorGUILayout.PropertyField(listenerFrequency);
            EditorGUILayout.PropertyField(listenerDuration);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
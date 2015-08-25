//
// Custom editor for Grass
//
using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Grass))]
    public class GrassEditor : Editor
    {
        SerializedProperty _density;
        SerializedProperty _extent;
        SerializedProperty _offset;

        SerializedProperty _randomPitchAngle;
        SerializedProperty _noisePitchAngle;
        SerializedProperty _rotationNoiseFrequency;
        SerializedProperty _rotationNoiseSpeed;
        SerializedProperty _rotationNoiseAxis;

        SerializedProperty _baseScale;
        SerializedProperty _minRandomScale;
        SerializedProperty _maxRandomScale;
        SerializedProperty _scaleNoiseAmplitude;
        SerializedProperty _scaleNoiseFrequency;

        SerializedProperty _shapes;
        SerializedProperty _material;
        SerializedProperty _castShadows;
        SerializedProperty _receiveShadows;

        static GUIContent _textRandomPitch    = new GUIContent("Random Pitch");
        static GUIContent _textNoiseToPitch   = new GUIContent("Noise To Pitch");
        static GUIContent _textNoiseFrequency = new GUIContent("Noise Frequency");
        static GUIContent _textNoiseSpeed     = new GUIContent("Noise Speed");
        static GUIContent _textNoiseAxis      = new GUIContent("Noise Axis");
        static GUIContent _textNoiseToScale   = new GUIContent("Noise To Scale");

        static GUIContent _textRandomScale   = new GUIContent("Random Scale");

        void OnEnable()
        {
            _density = serializedObject.FindProperty("_density");
            _extent  = serializedObject.FindProperty("_extent");
            _offset  = serializedObject.FindProperty("_offset");

            _randomPitchAngle       = serializedObject.FindProperty("_randomPitchAngle");
            _noisePitchAngle        = serializedObject.FindProperty("_noisePitchAngle");
            _rotationNoiseFrequency = serializedObject.FindProperty("_rotationNoiseFrequency");
            _rotationNoiseSpeed     = serializedObject.FindProperty("_rotationNoiseSpeed");
            _rotationNoiseAxis      = serializedObject.FindProperty("_rotationNoiseAxis");

            _baseScale           = serializedObject.FindProperty("_baseScale");
            _minRandomScale      = serializedObject.FindProperty("_minRandomScale");
            _maxRandomScale      = serializedObject.FindProperty("_maxRandomScale");
            _scaleNoiseAmplitude = serializedObject.FindProperty("_scaleNoiseAmplitude");
            _scaleNoiseFrequency = serializedObject.FindProperty("_scaleNoiseFrequency");

            _shapes         = serializedObject.FindProperty("_shapes");
            _material       = serializedObject.FindProperty("_material");
            _castShadows    = serializedObject.FindProperty("_castShadows");
            _receiveShadows = serializedObject.FindProperty("_receiveShadows");
        }

        public override void OnInspectorGUI()
        {
            var instance = target as Grass;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_density);
            if (EditorGUI.EndChangeCheck()) instance.NotifyConfigChange();

            EditorGUILayout.PropertyField(_extent);
            EditorGUILayout.PropertyField(_offset);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_randomPitchAngle, _textRandomPitch);
            EditorGUILayout.PropertyField(_noisePitchAngle, _textNoiseToPitch);
            EditorGUILayout.PropertyField(_rotationNoiseFrequency, _textNoiseFrequency);
            EditorGUILayout.PropertyField(_rotationNoiseSpeed, _textNoiseSpeed);
            EditorGUILayout.PropertyField(_rotationNoiseAxis, _textNoiseAxis);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_baseScale);
            MinMaxSlider(_textRandomScale, _minRandomScale, _maxRandomScale, 0.01f, 5.0f);
            EditorGUILayout.PropertyField(_scaleNoiseAmplitude, _textNoiseToScale);
            EditorGUILayout.PropertyField(_scaleNoiseFrequency, _textNoiseFrequency);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_shapes, true);
            if (EditorGUI.EndChangeCheck()) instance.NotifyConfigChange();

            EditorGUILayout.PropertyField(_material);
            EditorGUILayout.PropertyField(_castShadows);
            EditorGUILayout.PropertyField(_receiveShadows);

            serializedObject.ApplyModifiedProperties();
        }

        void MinMaxSlider(GUIContent label, SerializedProperty propMin, SerializedProperty propMax, float minLimit, float maxLimit)
        {
            var min = propMin.floatValue;
            var max = propMax.floatValue;

            EditorGUI.BeginChangeCheck();

            // Min-max slider.
            EditorGUILayout.MinMaxSlider(label, ref min, ref max, minLimit, maxLimit);

            var prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Float value boxes.
            var rect = EditorGUILayout.GetControlRect();
            rect.x += EditorGUIUtility.labelWidth;
            rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2 - 2;

            if (EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.labelWidth = 28;
                min = Mathf.Clamp(EditorGUI.FloatField(rect, "min", min), minLimit, max);
                rect.x += rect.width + 4;
                max = Mathf.Clamp(EditorGUI.FloatField(rect, "max", max), min, maxLimit);
                EditorGUIUtility.labelWidth = 0;
            }
            else
            {
                min = Mathf.Clamp(EditorGUI.FloatField(rect, min), minLimit, max);
                rect.x += rect.width + 4;
                max = Mathf.Clamp(EditorGUI.FloatField(rect, max), min, maxLimit);
            }

            EditorGUI.indentLevel = prevIndent;

            if (EditorGUI.EndChangeCheck()) {
                propMin.floatValue = min;
                propMax.floatValue = max;
            }
        }
    }
}

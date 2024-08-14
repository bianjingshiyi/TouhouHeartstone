using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;
using UI;
using UnityEditor;

namespace TouhouHeartstone
{
    [CustomEditor(typeof(MatchSizeScaler), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the GridLayout Component.
    /// Extend this class to write a custom editor for a component derived from GridLayout.
    /// </summary>
    public class MatchSizeScalerEditor : Editor
    {
        SerializedProperty m_ScaleMode;
        SerializedProperty m_TargetWidth;
        SerializedProperty m_TargetHeight;
        SerializedProperty m_MinScale;
        SerializedProperty m_MaxScale;

        protected virtual void OnEnable()
        {
            m_ScaleMode = serializedObject.FindProperty("m_ScaleMode");
            m_TargetWidth = serializedObject.FindProperty("m_TargetWidth");
            m_TargetHeight = serializedObject.FindProperty("m_TargetHeight");
            m_MinScale = serializedObject.FindProperty("m_MinScale");
            m_MaxScale = serializedObject.FindProperty("m_MaxScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_MinScale, true);
            EditorGUILayout.PropertyField(m_MaxScale, true);
            EditorGUILayout.PropertyField(m_ScaleMode, true);

            var enumValue = (MatchSizeScaler.ScaleMode)m_ScaleMode.enumValueIndex;
            var minOrMax = enumValue == MatchSizeScaler.ScaleMode.Min || enumValue == MatchSizeScaler.ScaleMode.Max;

            EditorGUI.indentLevel++;
            if (enumValue == MatchSizeScaler.ScaleMode.Width || minOrMax)
            {
                EditorGUILayout.PropertyField(m_TargetWidth, true);
            }
            if (enumValue == MatchSizeScaler.ScaleMode.Height || minOrMax)
            {
                EditorGUILayout.PropertyField(m_TargetHeight, true);
            }
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEditor;

namespace IGensoukyo.Utilities
{
    [CustomEditor(typeof(UITextExtended))]
    public class UITextExtendedEditor : GraphicEditor
    {
        SerializedProperty m_Text;
        SerializedProperty m_FontData;
        SerializedProperty isBestFit;
        SerializedProperty MinSize;
        SerializedProperty MaxSize;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
            isBestFit = serializedObject.FindProperty("isBestFit");
            MinSize = serializedObject.FindProperty("MinSize");
            MaxSize = serializedObject.FindProperty("MaxSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_FontData);
            EditorGUILayout.PropertyField(isBestFit);
            if (isBestFit.boolValue)
            {
                EditorGUILayout.PropertyField(MinSize);
                EditorGUILayout.PropertyField(MaxSize);
            }

            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

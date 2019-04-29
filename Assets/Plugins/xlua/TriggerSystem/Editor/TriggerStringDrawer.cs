using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TriggerSystem
{
    [CustomPropertyDrawer(typeof(TriggerStringAttribute))]
    public class TriggerStringDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
        }
    }
}
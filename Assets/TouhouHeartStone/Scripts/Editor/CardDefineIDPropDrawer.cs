using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UI;
using TouhouCardEngine;
using Game;
namespace TouhouHeartstone
{
    [CustomPropertyDrawer(typeof(CardDefineIDAttribute))]
    class CardDefineIDPropDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int index = Array.FindIndex(getCardDefines(), c => c.id == property.intValue) + 1;
            index = EditorGUI.Popup(position, label, index, new GUIContent[] { new GUIContent("None") }
                .Concat(getCardDefines().Select(c => new GUIContent(c.GetType().Name))).ToArray());
            property.intValue = index == 0 ? -1 : getCardDefines()[index - 1].id;
        }
        static CardDefine[] _cardDefines = null;
        static CardDefine[] getCardDefines()
        {
            if (_cardDefines == null)
            {
                _cardDefines = CardHelper.getCardDefines();
            }
            return _cardDefines;
        }
    }
}
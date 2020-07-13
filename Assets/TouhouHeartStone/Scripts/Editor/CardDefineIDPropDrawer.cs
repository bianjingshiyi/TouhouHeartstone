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
            if (label.text.StartsWith("Element", StringComparison.OrdinalIgnoreCase))
                label.text = getCardDefines()[index - 1].GetType().Name;
            if (GUI.Button(position, label))
            {
                getMenu(property).DropDown(position);
            }
        }
        static GenericMenu getMenu(SerializedProperty property)
        {
            int id = property.intValue;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), id == 0, () => property.intValue = 0);
            foreach (var group in getCardDefines().GroupBy(c => c.getCharacterID()))
            {
                CardDefine character = getCardDefines().FirstOrDefault(c => c.id == group.Key);
                foreach (var define in group.OrderBy(c => c.GetType().Name))
                {
                    string path = "";
                    if (character != null)
                        path = character.GetType().Name;
                    else
                        path = "Neutral";
                    int typeId = define.getCategory();
                    if (typeId == CardCategory.SERVANT)
                        path += "/Servant";
                    else if (typeId == CardCategory.SKILL)
                        path += "/Skill";
                    else if (typeId == CardCategory.SPELL)
                        path += "/Spell";
                    else if (typeId == CardCategory.ITEM)
                        path += "/Item";
                    path += "/" + define.GetType().Name;
                    menu.AddItem(new GUIContent(path), define.id == id, () =>
                    {
                        property.intValue = define.id;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }
            return menu;
        }
        static CardDefine[] _cardDefines = null;
        static CardDefine[] getCardDefines()
        {
            if (_cardDefines == null)
            {
                _cardDefines = CardHelper.getCardDefines(null);
            }
            return _cardDefines;
        }
    }
}
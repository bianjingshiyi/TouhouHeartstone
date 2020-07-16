using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UI;
using System.Reflection;
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
            draw(position, property, label);
        }

        public static void draw(Rect position, SerializedProperty property, GUIContent label)
        {
            int index = Array.FindIndex(getCardDefines(), c => c.id == property.intValue) + 1;
            //if (label.text.StartsWith("Element", StringComparison.OrdinalIgnoreCase))
            if (index == 0)
                label.text = "None";
            else
                label.text = getCardDefines()[index - 1].GetType().Name;
            if (GUI.Button(position, label))
            {
                if (UnityEngine.Event.current.button == 0)
                    getMenu(property).DropDown(position);
                else
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("ReloadCards"), false, () => EditorCardDefineHelper.loadCards());
                    menu.DropDown(position);
                }
            }
        }

        static GenericMenu getMenu(SerializedProperty property)
        {
            int id = property.intValue;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), id == 0, () =>
            {
                property.intValue = 0;
                property.serializedObject.ApplyModifiedProperties();
            });
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
        static CardDefine[] getCardDefines()
        {
            return EditorCardDefineHelper.getCardDefines();
        }
    }
    public static class EditorCardDefineHelper
    {
        static CardDefine[] _cardDefines = null;
        public static CardDefine[] getCardDefines()
        {
            if (_cardDefines == null)
            {
                loadCards();
            }
            return _cardDefines;
        }

        public static void loadCards()
        {
            _cardDefines = CardHelper.getCardDefines(new Assembly[] { typeof(THHGame).Assembly }, null);
        }

        public static CardDefine getCardDefine(int id)
        {
            return getCardDefines().FirstOrDefault(c => c.id == id);
        }
    }

}
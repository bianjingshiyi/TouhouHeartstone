using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UI;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
namespace TouhouHeartstone
{
    [CustomPropertyDrawer(typeof(CardSkinData))]
    class CardSkinDataPropDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                float height = EditorGUIUtility.singleLineHeight;//Folder
                height += EditorGUIUtility.singleLineHeight;//id
                height += EditorGUIUtility.singleLineHeight + 128;//image
                height += EditorGUIUtility.singleLineHeight;//name
                height += EditorGUIUtility.singleLineHeight * 4;//desc
                return height;
            }
            else
                return EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rect = position;
            position = new Rect(rect.position, new Vector2(rect.width, EditorGUIUtility.singleLineHeight));
            //Folder
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            position.y += EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                //id
                position = EditorGUI.IndentedRect(position);
                SerializedProperty idProp = property.FindPropertyRelative("_id");
                CardDefineIDPropDrawer.draw(position, idProp, new GUIContent(EditorCardDefineHelper.getCardDefine(idProp.intValue).GetType().Name));
                //int index = Array.FindIndex(getCardDefines(), c => c.id == idProp.intValue) + 1;
                //index = EditorGUI.Popup(position, nameof(CardSkinData.id), index, new string[] { "None" }.Concat(getCardDefines().Select(c => c.GetType().Name)).ToArray());
                //idProp.intValue = index == 0 ? -1 : getCardDefines()[index - 1].id;
                position.y += EditorGUIUtility.singleLineHeight;
                //image
                SerializedProperty imageProp = property.FindPropertyRelative("_" + nameof(CardSkinData.image));
                EditorGUI.PropertyField(position, imageProp);
                position.y += EditorGUIUtility.singleLineHeight;
                drawSprite(new Rect(position.x + position.width - 128, position.y, 128, 128), imageProp.objectReferenceValue as Sprite, new Vector2(128, 128));
                //GUI.Box(new Rect(position.x + position.width - 128, position.y, 128, 128), imageProp.objectReferenceValue == null ? null : (imageProp.objectReferenceValue as Sprite).texture);
                position.y += 128;
                //name
                SerializedProperty nameProp = property.FindPropertyRelative("_" + nameof(CardSkinData.name));
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, nameProp);
                position.y += EditorGUIUtility.singleLineHeight;
                //desc
                SerializedProperty descProp = property.FindPropertyRelative("_" + nameof(CardSkinData.desc));
                position.height = EditorGUIUtility.singleLineHeight * 4;
                EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width / 2, position.height), new GUIContent(nameof(CardSkinData.desc)));
                descProp.stringValue = EditorGUI.TextArea(new Rect(position.x + position.width / 2, position.y, position.width / 2, position.height), descProp.stringValue);
                position.y += EditorGUIUtility.singleLineHeight * 4;
            }
        }
        void drawSprite(Rect position, Sprite sprite, Vector2 size)
        {
            if (sprite == null)
            {
                GUI.Box(position, default(Texture));
                return;
            }
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;
            actualSize.x *= sprite.rect.width / sprite.rect.height;
            //actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
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
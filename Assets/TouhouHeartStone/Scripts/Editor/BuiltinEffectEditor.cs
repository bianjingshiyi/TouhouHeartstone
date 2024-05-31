using System;
using Game.Animations;
using UnityEditor;
using UnityEngine;

namespace Game
{
    [CustomEditor(typeof(BuiltinEffect))]
    public class BuiltinEffectEditor : Editor
    {
        private BuiltinEffect effectComp;
        private void OnEnable()
        {
            effectComp = target as BuiltinEffect;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "自动生成着色目标");
            if (foldout)
            {
                aberration = Mathf.Clamp(EditorGUILayout.FloatField("容差", aberration), 0, 255);
                if (GUILayout.Button("生成"))
                {
                    effectComp.colorTargets = effectComp.getPossibleColorTargets(aberration);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        private static float aberration;
        private static bool foldout;
    }
}


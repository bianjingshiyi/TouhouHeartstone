using UnityEngine;
using UnityEditor;
using Game;
using System.IO;

namespace TouhouHeartstone
{
    [CustomEditor(typeof(ReplayManager))]
    class ReplayManagerEditor: Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("载入Replay"))
            {
                var path = EditorUtility.OpenFilePanel("打开Replay", ReplayManager.getReplayFolder(), ReplayManager.EXTENSION);
                if (!string.IsNullOrEmpty(path))
                {
                    (target as ReplayManager).Play(Path.GetFileName(path));
                }
            }
        }
    }
}
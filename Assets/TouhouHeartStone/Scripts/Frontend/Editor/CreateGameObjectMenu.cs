using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TouhouHeartstone.Frontend.Editor
{
    public class CreateGameObjectMenu : MonoBehaviour
    {
        [MenuItem("GameObject/3D Object/ExtendText") ]
        public static void CreateExtendText()
        {
            GameObject go = new GameObject("New Extend Text");
            go.transform.parent = Selection.activeGameObject.transform;

            var rt = go.AddComponent<RectTransform>();
            rt.localRotation = new Quaternion();
            rt.localScale = Vector3.one;

            go.AddComponent<MeshRenderer>();
            go.AddComponent<MeshFilter>();
            var tm = go.AddComponent<TextMeshExtend>();
        }
    }
}



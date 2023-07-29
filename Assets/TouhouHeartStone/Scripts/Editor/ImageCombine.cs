using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

namespace TouhouHeartstone
{
    class ImageCombine
    {
        [MenuItem("Tools/Combine Images")]
        public static void CombineImages()
        {
            // 获取选中的物体
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length < 1)
                return;

            // 将选中的物体按照Hierarchy视图中的顺序排序
            List<GameObject> sortedObjects = new List<GameObject>(selectedObjects);
            sortedObjects.Sort((a, b) =>
            {
                List<int> orderA = GetHierarchyOrder(a.transform);
                List<int> orderB = GetHierarchyOrder(b.transform);

                // 遍历序列号列表，寻找第一个不同的序列号
                int i = 0;
                while (i < orderA.Count && i < orderB.Count)
                {
                    if (orderA[i] != orderB[i])
                    {
                        // 序列号更小的排在前面
                        return orderA[i].CompareTo(orderB[i]);
                    }
                    i++;
                }

                // 如果一个序列号列表是另一个序列号列表的前缀，那么长度更短的排在前面
                return orderA.Count.CompareTo(orderB.Count);
            });

            // 计算合并后的贴图的尺寸
            int xMin = 0, xMax = 0, yMin = 0, yMax = 0;
            foreach (GameObject obj in sortedObjects)
            {
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                xMin = xMin == 0 ? (int)(rectTransform.position.x - rectTransform.sizeDelta.x / 2) :
                    Mathf.Min(xMin, (int)(rectTransform.position.x - rectTransform.sizeDelta.x / 2));
                xMax = xMax == 0 ? (int)(rectTransform.position.x + rectTransform.sizeDelta.x / 2) :
                    Mathf.Max(xMax, (int)(rectTransform.position.x + rectTransform.sizeDelta.x / 2));
                yMin = yMin == 0 ? (int)(rectTransform.position.y - rectTransform.sizeDelta.y / 2) :
                    Mathf.Min(yMin, (int)(rectTransform.position.y - rectTransform.sizeDelta.y / 2));
                yMax = yMax == 0 ? (int)(rectTransform.position.y + rectTransform.sizeDelta.y / 2) :
                    Mathf.Max(yMax, (int)(rectTransform.position.y + rectTransform.sizeDelta.y / 2));
            }
            int width = xMax - xMin;
            int height = yMax - yMin;

            // 创建合并后的贴图
            Texture2D combinedTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int x = 0; x < combinedTex.width; x++)
            {
                for (int y = 0; y < combinedTex.height; y++)
                {
                    combinedTex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }

            // 遍历每个物体，将其Image组件的Sprite贴到合并后的贴图上
            foreach (GameObject obj in sortedObjects)
            {
                Image img = obj.GetComponent<Image>();
                if (img == null)
                    continue;
                Texture2D imgTex = img.sprite.texture;
                RectTransform rectTransform = img.rectTransform;
                int xStart = (int)rectTransform.position.x - imgTex.width / 2 - xMin;
                int yStart = (int)rectTransform.position.y - imgTex.height / 2 - yMin;
                for (int x = 0; x < imgTex.width; x++)
                {
                    for (int y = 0; y < imgTex.height; y++)
                    {
                        // 计算在合并后的贴图上的位置
                        int xPixel = xStart + x;
                        int yPixel = yStart + y;

                        // 使用Alpha混合来处理重叠的像素
                        Color src = imgTex.GetPixel(x, y);
                        Color dst = combinedTex.GetPixel(xPixel, yPixel);
                        combinedTex.SetPixel(xPixel, yPixel, new Color(
                            src.r * src.a + dst.r * (1 - src.a),
                            src.g * src.a + dst.g * (1 - src.a),
                            src.b * src.a + dst.b * (1 - src.a),
                            src.a + dst.a * (1 - src.a)));
                    }
                }
            }

            // 应用像素数据并保存贴图
            combinedTex.Apply();
            byte[] bytes = combinedTex.EncodeToPNG();
            string path = EditorUtility.SaveFilePanel("Save combined image", Application.dataPath, "Combined image", "png");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, bytes);

                // 在编辑器中刷新，使得新的图片文件可以立即看到
                AssetDatabase.Refresh();
            }
        }
        /// <summary>
        /// 获取一个物体在Hierarchy视图中的位置的序列号列表
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        private static List<int> GetHierarchyOrder(Transform transform)
        {
            List<int> order = new List<int>();
            while (transform != null)
            {
                order.Add(transform.GetSiblingIndex());
                transform = transform.parent;
            }
            // 反转列表，使得列表的首元素是Canvas的序列号，尾元素是物体自身的序列号
            order.Reverse();
            return order;
        }
    }
}
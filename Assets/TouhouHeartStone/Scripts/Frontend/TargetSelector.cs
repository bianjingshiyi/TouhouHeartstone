using UnityEngine;

using System;
using System.Collections.Generic;

namespace TouhouHeartstone.Frontend
{
    /// <summary>
    /// 目标选择器，用于选择一个目标
    /// </summary>
    public class TargetSelector : MonoBehaviour
    {
        Vector3 startPos;

        [SerializeField]
        GameObject HintObject;

        [SerializeField]
        LineRenderer lineRenderer;

        public delegate bool ContentFilter(RaycastHit hit);

        public void SetStartPosition(Vector3 globalPos)
        {
            startPos = globalPos;
        }

        public void HideSelector()
        {
            lineRenderer.gameObject.SetActive(false);
            HintObject.gameObject.SetActive(false);
        }

        RaycastHit lastHit;

        public RaycastHit LastSelectTarget => lastHit;

        public bool IsSelected => lastHit.transform != null;

        public int GetLastSelectID()
        {
            if (lastHit.transform != null)
                return 0;
            else return -1;
        }

        public bool UpdatePos(Vector3 mousePos, ContentFilter filter)
        {
            // 假设检测底板平面的物体
            mousePos.z = Camera.main.transform.position.y;
            var worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.y += 0.1f;

            var result = Physics.RaycastAll(worldPos, Vector3.down);
            foreach (var item in result)
            {
                if (filter(item))
                {
                    lastHit = item;
                    SetHintPos(worldPos, true);
                    return true;
                }
            }
            lastHit = new RaycastHit();
            SetHintPos(worldPos, false);
            return false;
        }

        /// <summary>
        /// 设置hint的位置和显示
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="visible"></param>
        void SetHintPos(Vector3 pos, bool visible)
        {
            HintObject.transform.position = pos;
            HintObject.SetActive(visible);

            lineRenderer.gameObject.SetActive(true);

            var array = generateDropLine(startPos, pos);
            lineRenderer.positionCount = array.Length;
            lineRenderer.SetPositions(array);
        }

        Vector3[] generateDropLine(Vector3 start, Vector3 end)
        {
            const double top = 1;
            const double step = 0.05;

            var start2D = MathUtils.xzy2xy(start);
            var end2D = MathUtils.xzy2xy(end);

            var b = Vector2.Distance(start2D, end2D);
            var k = -4 * top / b / b;

            List<Vector3> result = new List<Vector3>();

            for (double i = 0; i < b; i += step)
            {
                // 2次函数
                var y = k * i * (i - b);
                var pos2D = Vector2.Lerp(start2D, end2D, (float)(i / b));
                var pos = MathUtils.xzy2xyz(pos2D);
                pos.y = (float)y + end.y;

                result.Add(pos);
            }
            result.Add(end);

            return result.ToArray();
        }
    }

    public class DebugUtils
    {
        public static void Log(string message, UnityEngine.Object context = null)
        {
            Debug.Log("[Frontend]" + message, context);
        }

        public static void LogDebug(string message, UnityEngine.Object context = null)
        {
            Debug.Log($"<color=grey>[Frontend]{message}</color>", context);
        }

        public static void LogWarning(string message, UnityEngine.Object context = null)
        {
            Debug.LogWarning("[Frontend]" + message, context);
        }
    }
}

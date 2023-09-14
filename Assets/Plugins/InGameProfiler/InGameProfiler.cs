using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace InGameProfiler
{
    public class InGameProfiler : MonoBehaviour
    {
        [SerializeField]
        Rect startRect = new Rect(400,0,600,400);

        [SerializeField]
        Vector2 buttonSize = new Vector2(64, 32);

        [SerializeField]
        int windowID = 2;

        [SerializeField]
        int maxFrames = 300;

        [SerializeField]
        bool collapseDefault = true;

        [SerializeField]
        bool collapse;

        [SerializeField]
        Rect rect;

        private Vector2 buttonOffset = new Vector2(8, 24);
        private Vector2 buttonMargin = new Vector2(8, 16);

        private float maxTime;
        private List<float> frameTimes = new List<float>();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private void Awake()
        {
            rect = startRect;
            if (collapseDefault)
                toggleCollapse();
        }
        protected void OnGUI()
        {
            var time = Time.unscaledDeltaTime * 1000;
            frameTimes.Add(time);

            if (frameTimes.Count > maxFrames)
            {
                frameTimes.RemoveRange(0, frameTimes.Count - maxFrames);
            }
            var max = frameTimes.Max();
            maxTime = (maxTime - max) * 0.8f + max;
            rect = GUI.Window(windowID, rect, draw, "性能");
        }

        private void draw(int id)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 32));
            if (GUI.Button(new Rect(buttonOffset, buttonSize), collapse ? "显示" : "隐藏"))
            {
                toggleCollapse();
            }

            if (!collapse)
            {
                GUILayout.BeginArea(new Rect(0, buttonSize.y, rect.width, rect.height - 60));
                GUILayout.BeginVertical();
                drawCurve();
                drawDegrees();
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }
        private void drawDegrees()
        {
            var rangeRect = new Rect(0, buttonSize.y, rect.width, rect.height - 60);

            var guiColor = GUI.color;
            GUI.color = Color.black;
            var timeSegment = Mathf.CeilToInt(maxTime / 5f);
            for (int i = 1; i <= 5; i++)
            {
                var time = i * timeSegment;
                var y = rangeRect.height * (1 - time / maxTime);
                var height = 20;
                var degreeStr = $"{time}ms";
                GUI.Label(new Rect(0, y - height, rangeRect.width, height), degreeStr);
                GUI.DrawTexture(new Rect(0, y, rangeRect.width, 1), Texture2D.whiteTexture);
            }
            GUI.color = guiColor;
        }
        private void drawCurve()
        {
            var rangeRect = new Rect(0, buttonSize.y, rect.width, rect.height - 60);
            var width = rect.width / maxFrames;
            var guiColor = GUI.color;
            GUI.color = Color.cyan;
            for (int i = 0; i < maxFrames; i++)
            {
                if (i >= frameTimes.Count)
                    continue;
                var time = frameTimes[i];
                var height = rangeRect.height * time / maxTime;
                var x = rangeRect.width - width * i;
                var y = rangeRect.height - height;
                GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
            }
            GUI.color = guiColor;
        }

        private void toggleCollapse()
        {
            collapse = !collapse;
            rect.size = collapse ? buttonSize + buttonMargin * 2 : startRect.size;
        }
#endif
    }
}



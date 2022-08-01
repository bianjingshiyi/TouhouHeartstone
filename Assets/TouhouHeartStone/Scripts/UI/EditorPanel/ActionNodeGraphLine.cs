using UnityEngine;
using UnityEngine.UI.Extensions;

namespace UI
{

    public class ActionNodeGraphLine : MonoBehaviour
    {
        private Transform[] _controlPoints = new Transform[4];
        private Vector2[] _points = new Vector2[50];
        private UILineRenderer _uiline;
        public int _length = 50;
        private Transform _fatherObject;
        void Update()
        {
            UpdatePoints();
        }
        public void setPosition(GameObject _startPoint, GameObject _endPoint)//为线段设置起点终点
        {
            _fatherObject = transform.parent;
           _uiline = GetComponent<UILineRenderer>();
            Transform[] _startPointChildObject = _startPoint.GetComponentsInChildren<Transform>();

            foreach (Transform t in _startPointChildObject)
            {
                if (t.name == "Slot") { _controlPoints[0] = t; }
                if (t.name == "Slot_d") { _controlPoints[1] = t; }
            }
            Transform[] _endPointChildObject = _endPoint.GetComponentsInChildren<Transform>();

            foreach (Transform t in _endPointChildObject)
            {
                if (t.name == "Slot_d") { _controlPoints[2] = t; }
                if (t.name == "Slot") { _controlPoints[3] = t; }
            }
            _uiline.Points = new Vector2[_length];
            //Debug.LogError(_controlPoints[0].position);
        }


        public void SetColor(Color baseColor)//设定颜色
        {
            _uiline.color = baseColor;
        }
        void UpdatePoints()//更新节点
        {
            _uiline.SetAllDirty();
            for (int i = 1; i <= _length; i++)
            {
                float t = i / (float)_length;
                Vector3 pixel = CacluatePoints(t,
                    _fatherObject.InverseTransformPoint(_controlPoints[0].position),
                    _fatherObject.InverseTransformPoint(_controlPoints[1].position),
                    _fatherObject.InverseTransformPoint(_controlPoints[2].position),
                    _fatherObject.InverseTransformPoint(_controlPoints[3].position)
                    // _controlPoints[1].position, 
                    //  _controlPoints[2].position, 
                    //  _controlPoints[3].position
                    );
                _points[i - 1] = pixel;
            }
            _uiline.Points = _points;
        }

        Vector3 CacluatePoints(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)//计算节点
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}
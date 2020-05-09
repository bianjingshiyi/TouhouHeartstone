using UnityEngine;
namespace UI
{
    partial class Fatigue
    {
        [SerializeField]
        AnimationCurve _moveCurve;
        public AnimationCurve moveCurve
        {
            get { return _moveCurve; }
        }
        [SerializeField]
        AnimationCurve _fadeCurve;
        public AnimationCurve fadeCurve
        {
            get { return _fadeCurve; }
        }
    }
}

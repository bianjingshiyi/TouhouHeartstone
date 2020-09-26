using BJSYGameCore;
using UnityEngine;
using System.Linq;
using UI;
using System;
namespace Game
{
    [Serializable]
    class HandMoveAnim : TableAnimation
    {
        [SerializeField]
        HandListItem _hand;
        [SerializeField]
        Vector3 _startPosition;
        [SerializeField]
        Vector3 _targetPosition;
        [SerializeField]
        Transform _target;
        [SerializeField]
        AnimationCurve _curve;
        [SerializeField]
        Timer _timer;
        public HandMoveAnim(HandListItem hand, Vector3 targetPosition, AnimationCurve curve)
        {
            _hand = hand;
            _startPosition = hand.Card.rectTransform.position;
            _targetPosition = targetPosition;
            _curve = curve;
            _timer = new Timer() { duration = curve.keys.Last().time };
            _timer.start();
        }
        public HandMoveAnim(HandListItem hand, Transform target, AnimationCurve curve)
        {
            _hand = hand;
            _startPosition = hand.Card.rectTransform.position;
            _target = target;
            _curve = curve;
            _timer = new Timer() { duration = curve.keys.Last().time };
            _timer.start();
        }
        public override bool update(TableManager table)
        {
            if (!_timer.isExpired())
            {
                _hand.Card.rectTransform.position = Vector3.Lerp(_startPosition, _target != null ? _target.position : _targetPosition, _curve.Evaluate(_timer.time));
                return false;
            }
            return true;
        }
    }
}

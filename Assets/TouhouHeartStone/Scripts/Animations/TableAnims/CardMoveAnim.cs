using BJSYGameCore;
using UnityEngine;
using System.Linq;
using UI;
using System;
namespace Game
{
    [Serializable]
    class CardMoveAnim : TableAnimation
    {
        [SerializeField]
        Card _card;
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
        public CardMoveAnim(Card card, Vector3 targetPosition, AnimationCurve curve)
        {
            _card = card;
            _startPosition = card.rectTransform.position;
            _targetPosition = targetPosition;
            _curve = curve;
            _timer = new Timer() { duration = curve.keys.Last().time };
            _timer.start();
        }
        public CardMoveAnim(Card card, Transform target, AnimationCurve curve)
        {
            _card = card;
            _startPosition = card.rectTransform.position;
            _target = target;
            _curve = curve;
            _timer = new Timer() { duration = curve.keys.Last().time };
            _timer.start();
        }
        public override bool update(TableManager table, float time)
        {
            if (!_timer.isExpired())
            {
                _card.rectTransform.position = Vector3.Lerp(_startPosition, _target != null ? _target.position : _targetPosition, _curve.Evaluate(_timer.time));
                return false;
            }
            return true;
        }
    }
}

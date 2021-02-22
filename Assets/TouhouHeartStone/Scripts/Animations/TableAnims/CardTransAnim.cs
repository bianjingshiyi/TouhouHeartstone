using BJSYGameCore;
using UnityEngine;
using System.Linq;
using UI;
using System;
namespace Game
{
    [Serializable]
    class CardTransAnim : TableAnimation
    {
        [SerializeField]
        Card _card;
        [SerializeField]
        Vector3 _startPosition;
        [SerializeField]
        Vector3 _startScale;
        [SerializeField]
        Vector3 _targetPosition;
        [SerializeField]
        Transform _target;
        [SerializeField]
        Vector3 _scale;
        [SerializeField]
        AnimationCurve _curve;
        [SerializeField]
        Timer _timer;
        public CardTransAnim(Card card, Vector3 targetPosition, AnimationCurve curve)
        {
            _card = card;
            _startPosition = card.rectTransform.position;
            _targetPosition = targetPosition;
            _curve = curve;
            _timer = new Timer() { duration = curve.keys.Last().time };
            _timer.start();
        }
        public CardTransAnim(Card card, Transform target, Vector3 scale, AnimationCurve curve)
        {
            _card = card;
            _startPosition = card.rectTransform.position;
            _startScale = card.transform.localScale;
            _target = target;
            _scale = scale;
            _curve = curve;
            _timer = new Timer() { duration = curve.keys.Last().time };
            _timer.start();
        }
        public override bool update(TableManager table, float time)
        {
            if (!_timer.isExpired() && _card != null)
            {
                _card.rectTransform.position = Vector3.Lerp(_startPosition, _target != null ? _target.position : _targetPosition, _curve.Evaluate(_timer.time));
                _card.transform.localScale = Vector3.Lerp(_startScale, _scale, _curve.Evaluate(_timer.time));
                return false;
            }
            _card.rectTransform.position = _target != null ? _target.position : _targetPosition;
            _card.transform.localScale = _scale;
            return true;
        }
    }
}

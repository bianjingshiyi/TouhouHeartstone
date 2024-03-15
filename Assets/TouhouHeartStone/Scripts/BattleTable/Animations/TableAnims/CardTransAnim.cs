using System;
using System.Linq;
using BJSYGameCore;
using UnityEngine;
namespace Game.BattleTable
{
    [Serializable]
    class CardTransAnim : TableAnimation
    {
        [SerializeField]
        TableCard _card;
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
        public CardTransAnim(TableCard card, Vector3 targetPosition, AnimationCurve curve)
        {
            _card = card;
            _startPosition = card.transform.position;
            _startScale = card.transform.localScale;
            _targetPosition = targetPosition;
            _scale = _startScale;
            _curve = curve;
            _timer = new Timer() { duration = curve.keys.Last().time };
            _timer.start();
        }
        public CardTransAnim(TableCard card, Vector3 targetPosition, Vector3 scale, AnimationCurve curve)
        {
            _card = card;
            _startPosition = card.transform.position;
            _startScale = card.transform.localScale;
            _targetPosition = targetPosition;
            _scale = scale;
            _curve = curve;
            _timer = new Timer() { duration = curve.keys.Last().time };
            _timer.start();
        }
        public CardTransAnim(TableCard card, Transform target, Vector3 scale, AnimationCurve curve)
        {
            _card = card;
            _startPosition = card.transform.position;
            _startScale = card.transform.localScale;
            _target = target;
            _scale = scale;
            _curve = curve;
            _timer = new Timer() { duration = curve.keys.Last().time };
            _timer.start();
        }
        public override bool update(BattleTableManager table, float time)
        {
            if (_card == null)
                return true;
            var destination = _target != null ? _target.position : _targetPosition;
            var expired = _timer.isExpired();
            float t = !expired ? t = _curve.Evaluate(_timer.time) : 1;
            _card.transform.position = Vector3.Lerp(_startPosition, destination, t);
            _card.transform.localScale = Vector3.Lerp(_startScale, _scale, t);
            return expired;
        }
    }
}

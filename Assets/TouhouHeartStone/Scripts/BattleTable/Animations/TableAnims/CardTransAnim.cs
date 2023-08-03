using BJSYGameCore;
using UnityEngine;
using System.Linq;
using System;
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
            if (!_timer.isExpired() && _card != null)
            {
                _card.transform.position = Vector3.Lerp(_startPosition, _target != null ? _target.position : _targetPosition, _curve.Evaluate(_timer.time));
                _card.transform.localScale = Vector3.Lerp(_startScale, _scale, _curve.Evaluate(_timer.time));
                return false;
            }
            _card.transform.position = _target != null ? _target.position : _targetPosition;
            _card.transform.localScale = _scale;
            return true;
        }
    }
}

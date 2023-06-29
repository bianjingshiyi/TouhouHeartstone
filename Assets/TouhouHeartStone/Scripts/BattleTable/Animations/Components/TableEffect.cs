using Game.BattleTable;
using UnityEngine;

namespace Game.Animations
{
    public class TableEffect : MonoBehaviour
    {
        public virtual void init(BattleTableManager table, AnimationDefine define, Transform sourceTransform, Transform destTransform)
        {
            _table = table;
            _srcTransform = sourceTransform;
            transform.position = _srcTransform.position;
            _destTransform = destTransform;
            _duration = define.duration;
            _blockTime = define.blockTime;
        }
        public virtual void update(float deltaTime)
        {
            if (_animator)
            {
                _animator.enabled = false;
                _animator.Update(deltaTime);
            }
            _time += deltaTime;
            if (_duration > 0 && _time > _duration)
            {
                finish();
            }
        }
        public void finish()
        {
            _finished = true;
        }
        public bool isFinished()
        {
            return _finished;
        }
        public float blockTime => _blockTime;
        public Transform sourceTransform => _srcTransform;
        public Transform destTransform => _destTransform;
        public BattleTableManager table => _table;
        public float time => _time;
        public float duration => _duration;
        [SerializeField]
        protected Animator _animator;
        [SerializeField]
        private Transform _srcTransform;
        [SerializeField]
        private Transform _destTransform;
        protected bool _finished;
        protected float _time;
        protected float _duration;
        protected float _blockTime;
        protected BattleTableManager _table;
    }
}

using Game.BattleTable;
using UnityEngine;

namespace Game.Animations
{
    public class TableEffect : MonoBehaviour
    {
        public virtual void init(BattleTableManager table, AnimationDefine define, VisualEffectInfo info)
        {
            _table = table;
            _srcTransform = info.srcTransform;
            if (_srcTransform)
                transform.position = _srcTransform.position;
            _destTransform = info.destTransform;
            _duration = define.duration;
            _blockTime = define.blockTime;
        }
        private void OnEnable()
        {
            if (isTestMode())
            {
                _time = 0;
                _duration = GetComponent<BuiltinEffect>()?.duration ?? 0;
            }
        }
        private void Update()
        {
            if (isTestMode())
            {
                update(Time.deltaTime);
            }
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
            if (isTestMode())
            {
                gameObject.SetActive(false);
            }
        }
        public bool isFinished()
        {
            return _finished;
        }
        protected bool isTestMode()
        {
#if UNITY_EDITOR
            return _testMode;
#else
            return false;
#endif
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
#if UNITY_EDITOR
        [SerializeField]
        protected bool _testMode;
#endif
        protected bool _finished;
        protected float _time;
        protected float _duration;
        protected float _blockTime;
        protected BattleTableManager _table;
    }
    public class VisualEffectInfo
    {
        public Transform srcTransform;
        public Transform destTransform;
    }
}

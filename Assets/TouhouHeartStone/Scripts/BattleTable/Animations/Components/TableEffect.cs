using Game.BattleTable;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Game.Animations
{
    public abstract class TableEffect : MonoBehaviour
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
        private void Start()
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
        public void update(float deltaTime)
        {
            if (!_isStarted)
            {
                if (sourceTransform == null)
                {
                    rootObj?.SetActive(false);
                    return;
                }
                else if (!rootObj.activeSelf)
                {
                    rootObj?.SetActive(true);
                }
                _isStarted = true;
            }
            if (_animator)
            {
                _animator.enabled = false;
                _animator.Update(deltaTime);
            }
            updateAnim(deltaTime);
            _time += deltaTime;
            if (_duration > 0 && _time > _duration)
            {
                finish();
            }
        }
        protected abstract void updateAnim(float deltaTime);
        public void finish()
        {
            _finished = true;
            if (isTestMode())
            {
                gameObject.SetActive(false);
            }
            onFinished?.Invoke(this);
        }
        public bool isFinished()
        {
            return _finished;
        }
        protected bool isTestMode()
        {
#if UNITY_EDITOR
            return testMode;
#else
            return false;
#endif
        }
        [Conditional("UNITY_EDITOR")]
        public void setTestMode(bool value)
        {

#if UNITY_EDITOR
            testMode = value;
#endif
        }
        public float blockTime => _blockTime;
        public Transform sourceTransform 
        {
            get => _srcTransform;
            set => _srcTransform = value;
        }
        public Transform destTransform
        {
            get => _destTransform;
            set => _destTransform = value;
        }
        public GameObject rootObj
        {
            get 
            { 
                if (!_rootObj)
                {
                    _rootObj = transform.Find("Root").gameObject;
                }
                return _rootObj;
            }
        }
        public bool isStarted => _isStarted;
        public BattleTableManager table => _table;
        public float time => _time;
        public float duration => _duration;
        public event Action<TableEffect> onFinished;
        [SerializeField]
        protected Animator _animator;
        [SerializeField]
        private Transform _srcTransform;
        [SerializeField]
        private Transform _destTransform;
#if UNITY_EDITOR
        [SerializeField]
        private bool testMode;
#endif
        protected bool _finished;
        protected float _time;
        protected float _duration;
        protected float _blockTime;
        protected GameObject _rootObj;
        protected BattleTableManager _table;
        private bool _isStarted;
    }
    public class VisualEffectInfo
    {
        public Transform srcTransform;
        public Transform destTransform;
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using Game.BattleTable;
using UnityEngine;

namespace Game.Animations
{
    public abstract class TableEffect : MonoBehaviour
    {
        #region 公有方法
        public virtual void init(BattleTableManager table, AnimationDefine define, VisualEffectInfo info)
        {
            _table = table;
            _srcTransform = info.srcTransform;
            if (_srcTransform)
                transform.position = _srcTransform.position;
            _destTransform = info.destTransform;
            _duration = define.duration;
            _blockTime = define.blockTime;
            _isPersistent = define.isPersistent;
        }
        public void update(float deltaTime)
        {
            // 禁用动画器。
            if (_animator)
            {
                _animator.enabled = false;
            }
            if (_state == EffectState.None)
            {
                if (!isTestMode())
                {
                    // 必须要求存在源位置才能播放。
                    if (sourceTransform == null)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                    // 源位置存在，启动。
                    if (!gameObject.activeSelf)
                    {
                        gameObject.SetActive(true);
                    }
                }
                _state = EffectState.Started;
                transform.position = sourceTransform.position;
                if (_playSoundOnStart && sePlayer)
                {
                    sePlayer.Play();
                }
            }
            // 更新动画器。
            if (_animator)
            {
                _animator.Update(deltaTime);
            }
            // 更新动画。
            updateAnim(deltaTime);
            if (_isPersistent)
            {
                updatePersistentAnim();
            }
            _time += deltaTime;

            // 动画结束判断。
            checkFinished();
        }
        public void end()
        {
            if (_state == EffectState.Stopped || _state == EffectState.Finished)
                return;
            _state = EffectState.Stopped;
            if (_animator)
                _animator.SetTrigger("End");
            stopParticles();
        }
        public void finish()
        {
            if (_state == EffectState.Finished)
                return;
            _state = EffectState.Finished;
            if (isTestMode())
            {
                gameObject.SetActive(false);
            }
            onFinished?.Invoke(this);
        }
        public bool isFinished()
        {
            return _state == EffectState.Finished;
        }
        public void setVisible(bool visible)
        {
            _visible = visible;
        }
        [Conditional("UNITY_EDITOR")]
        public void setTestMode(bool value)
        {
#if UNITY_EDITOR
            testMode = value;
#endif
        }
        public void modifyColorTarget(ColorTarget target, Color color)
        {
            var child = transform.Find(target.path);
            switch (target.type)
            {
                case ColorTargetType.MeshRenderer:
                    var mesh = child.GetComponent<MeshRendererController>();
                    if (!mesh)
                    {
                        mesh = child.gameObject.AddComponent<MeshRendererController>();
                    }
                    color.a = mesh.color.a;
                    mesh.color = color;
                    break;
                case ColorTargetType.MeshAnimator:
                    var meshAnimator = child.GetComponent<MeshRendererAnimator>();
                    if (!meshAnimator)
                    {
                        mesh = child.gameObject.AddComponent<MeshRendererController>();
                    }
                    meshAnimator.gradient = modifyGradient(meshAnimator.gradient, target.index, color);
                    break;
                case ColorTargetType.Light:
                    var light = child.GetComponent<Light>();
                    if (light)
                    {
                        color.a = light.color.a;
                        light.color = color;
                    }
                    break;
                case ColorTargetType.ParticleSystem:
                    var particle = child.GetComponent<ParticleSystem>();
                    if (particle)
                    {
                        switch (target.particleType)
                        {
                            case ParticleColorTarget.StartColor:
                                var main = particle.main;
                                main.startColor = modifyMinMaxGradient(main.startColor, target.index, color);
                                break;
                            case ParticleColorTarget.ColorOverLifetime:
                                var colorOverLifetime = particle.colorOverLifetime;
                                colorOverLifetime.color = modifyMinMaxGradient(colorOverLifetime.color, target.index, color);
                                break;
                            case ParticleColorTarget.ColorBySpeed:
                                var colorBySpeed = particle.colorBySpeed;
                                colorBySpeed.color = modifyMinMaxGradient(colorOverLifetime.color, target.index, color);
                                break;
                        }
                    }
                    break;
            }
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void OnEnable()
        {
            // 禁用动画器。
            if (_animator)
            {
                _animator.enabled = false;
            }
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
        #endregion
        protected abstract void updateAnim(float deltaTime);
        protected bool isTestMode()
        {
#if UNITY_EDITOR
            return testMode;
#else
            return false;
#endif
        }
        private void updatePersistentAnim()
        {
            if (!sourceTransform || !_visible)
            {
                if (_state == EffectState.Started)
                {
                    _state = EffectState.Dormant;
                    stopParticles();
                }
            }
            else if (_state == EffectState.Dormant)
            {
                _state = EffectState.Started;
                resumeParticles();
            }
        }
        private void checkFinished()
        {
            if (_isPersistent) // 持续性特效
            {
                if (_state == EffectState.Stopped)
                {
                    bool allStopped = true;
                    foreach (var particleSystem in particleSystems)
                    {
                        if (!particleSystem.isStopped)
                        {
                            allStopped = false;
                            break;
                        }
                    }
                    if (allStopped)
                    {
                        finish();
                    }
                }
            }
            else // 非持续性特效。
            {
                if (_state == EffectState.Started)
                {
                    if (_duration > 0 && _time > _duration)
                    {
                        finish();
                    }
                }
            }
        }
        private void stopParticles()
        {
            foreach (var particleSystem in particleSystems)
            {
                if (!particleSystem.isEmitting)
                    continue;
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        private void resumeParticles()
        {
            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem.isEmitting)
                    continue;
                particleSystem.Play(true);
            }
        }
        private static ParticleSystem.MinMaxGradient modifyMinMaxGradient(ParticleSystem.MinMaxGradient input, int index, Color color)
        {
            var result = new ParticleSystem.MinMaxGradient()
            {
                mode = input.mode
            };
            switch (input.mode)
            {
                case ParticleSystemGradientMode.Color:
                    color.a = input.color.a;
                    result.color = color;
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    if (index == 0)
                    {
                        color.a = input.colorMin.a;
                        result.colorMin = color;
                    }
                    else
                    {
                        color.a = input.colorMax.a;
                        result.colorMax = color;
                    }
                    break;
                case ParticleSystemGradientMode.Gradient:
                case ParticleSystemGradientMode.RandomColor:
                    result.gradient = modifyGradient(input.gradient, index, color);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    var minLength = input.gradientMin.colorKeys.Length;
                    if (index < minLength)
                        result.gradientMin = modifyGradient(input.gradientMin, index, color);
                    else
                        result.gradientMax = modifyGradient(input.gradientMax, index - minLength, color);
                    break;
            }
            return result;
        }
        private static Gradient modifyGradient(Gradient input, int index, Color color)
        {
            var colorKeys = input.colorKeys.ToArray();

            var colorKey = colorKeys[index];
            colorKey.color = color;
            colorKeys[index] = colorKey;

            var output = new Gradient()
            {
                mode = input.mode,
                colorSpace = input.colorSpace,
                alphaKeys = input.alphaKeys.ToArray(),
                colorKeys = colorKeys,
            };
            return output;
        }
        #endregion

        #region 属性字段
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
        public BattleTableManager table => _table;
        public float time => _time;
        public float duration => _duration;
        public float blockTime => _blockTime;
        public EffectState state => _state;
        public event Action<TableEffect> onFinished;
        [SerializeField]
        protected Animator _animator;
        [SerializeField]
        private Transform _srcTransform;
        [SerializeField]
        private Transform _destTransform;
        [SerializeField]
        private bool _playSoundOnStart;
#if UNITY_EDITOR
        [SerializeField]
        private bool testMode;
#endif
        private ParticleSystem[] _particleSystems;
        private ParticleSystem[] particleSystems
        {
            get
            {
                if (_particleSystems == null)
                    _particleSystems = GetComponentsInChildren<ParticleSystem>();
                return _particleSystems;
            }
        }
        private TableSEPlayer _sePlayer;
        private TableSEPlayer sePlayer
        {
            get
            {
                if (_sePlayer == null)
                    _sePlayer = GetComponent<TableSEPlayer>();
                return _sePlayer;
            }
        }
        protected float _time;
        protected float _duration;
        protected float _blockTime;
        protected bool _isPersistent;
        protected GameObject _rootObj;
        protected BattleTableManager _table;
        protected EffectState _state;
        protected bool _visible = true;
        #endregion
    }
    public enum EffectState
    {
        /// <summary>
        /// 没有状态。
        /// </summary>
        None,
        /// <summary>
        /// 播放状态。播放特效。
        /// </summary>
        Started,
        /// <summary>
        /// 停止播放状态。不再产生新的粒子，并在所有粒子都消失后被清理。
        /// </summary>
        Stopped,
        /// <summary>
        /// 完成状态。所有特效都已经消失，等待被摧毁。
        /// </summary>
        Finished,
        /// <summary>
        /// 休眠状态。不存在有效源位置的持续性特效会进入休眠，停止所有粒子，直到源位置再次有效。
        /// </summary>
        Dormant
    }
    public class VisualEffectInfo
    {
        public Transform srcTransform;
        public Transform destTransform;
    }
}

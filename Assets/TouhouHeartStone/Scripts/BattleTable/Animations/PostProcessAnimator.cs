using BJSYGameCore;
using Game.PostProcess;
using RestSharp;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Game.BattleTable
{
    /// <summary>
    /// 使用动画曲线来控制后处理效果
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [ExecuteInEditMode]
    public class PostProcessAnimator : MonoBehaviour
    {
        protected void OnEnable()
        {
            _volume = this.findInstance<PostProcessVolume>();
            if (_volume == null || _volume.profile == null)
                return;

            _settings = null;
            foreach (PostProcessEffectSettings setting in _volume.profile.settings)
            {
                if (setting.GetType().Name.StartsWith(effectName) && setting is THHPostProcessEffectSettings thhSettings)
                {
                    _settings = thhSettings;
                    _isEffectActiveBeforeSetting = setting.active;
                    break;
                }
            }
        }
        protected void Update()
        {
            if (_settings == null)
                return;

            _settings.active = active;
            if (type == PostProcessParameterType.Float)
                _settings.SetParameter(paramName, floatValue);
            else if (type == PostProcessParameterType.Vector2)
                _settings.SetParameter(paramName, vector2Value);
            else if (type == PostProcessParameterType.Vector3)
                _settings.SetParameter(paramName, vector3Value);
            else if (type == PostProcessParameterType.Vector4)
                _settings.SetParameter(paramName, vector4Value);
            else if (type == PostProcessParameterType.Color)
                _settings.SetParameter(paramName, colorValue);
        }
        protected void OnDisable()
        {
            if (!resetOnDisable)
                return;
            if (_settings == null)
                return;

            if (_isParamActiveBeforeSetting)
                _settings.SetParameter(paramName, _paramValueBeforeSetting);
            else
                _settings.SetParameter(paramName, null);
            if (!_isEffectActiveBeforeSetting)
                _settings.active = false;
        }
        public string effectName;
        public string paramName;
        public bool active;
        public PostProcessParameterType type;
        public float floatValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public Color colorValue;
        public bool resetOnDisable = true;
        private PostProcessVolume _volume;
        private THHPostProcessEffectSettings _settings;
        private bool _isEffectActiveBeforeSetting;
        private bool _isParamActiveBeforeSetting;
        private object _paramValueBeforeSetting;
    }
    public enum PostProcessParameterType
    {
        Float,
        Vector2,
        Vector3,
        Vector4,
        Color
    }
}

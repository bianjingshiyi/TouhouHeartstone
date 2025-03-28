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
        }
        protected void Update()
        {
            foreach (PostProcessEffectSettings setting in _volume.profile.settings)
            {
                if (setting.GetType().Name.StartsWith(effectName) && setting is THHPostProcessEffectSettings thhSettings)
                {
                    setting.active = true;
                    if (type == PostProcessParameterType.Float)
                        thhSettings.SetParameter(paramName, floatValue);
                    else if (type == PostProcessParameterType.Vector2)
                        thhSettings.SetParameter(paramName, vector2Value);
                    else if (type == PostProcessParameterType.Vector3)
                        thhSettings.SetParameter(paramName, vector3Value);
                    else if (type == PostProcessParameterType.Vector4)
                        thhSettings.SetParameter(paramName, vector4Value);
                    else if (type == PostProcessParameterType.Color)
                        thhSettings.SetParameter(paramName, colorValue);
                    break;
                }
            }
        }
        protected void OnDisable()
        {
            if (!resetOnDisable)
                return;
            foreach (PostProcessEffectSettings setting in _volume.profile.settings)
            {
                if (setting.GetType().Name.StartsWith(effectName) && setting is THHPostProcessEffectSettings thhSettings)
                {
                    thhSettings.ResetParameter(paramName);
                    break;
                }
            }
        }
        public string effectName;
        public string paramName;
        public PostProcessParameterType type;
        public float floatValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public Color colorValue;
        public bool resetOnDisable = true;
        private PostProcessVolume _volume;
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

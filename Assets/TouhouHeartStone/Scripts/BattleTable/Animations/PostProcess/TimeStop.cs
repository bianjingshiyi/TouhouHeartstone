using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Game.PostProcess
{
    [Serializable]
    [PostProcess(typeof(TimeStopRenderer), PostProcessEvent.AfterStack, "THH/TimeStop")]
    public sealed class TimeStopSettings : THHPostProcessEffectSettings
    {
        public override void SetParameter(string name, object value)
        {
            if (name == nameof(center))
            {
                if (value is Vector2 vector2)
                {
                    center.overrideState = true;
                    center.value = vector2;
                }
                else
                    center.overrideState = false;
            }
            else if (name == nameof(radius))
            {
                if (value is float f)
                {
                    radius.overrideState = true;
                    radius.value = f;
                }
                else
                    radius.overrideState = false;
            }
            else if (name == nameof(fade))
            {
                if (value is float f)
                {
                    fade.overrideState = true;
                    fade.value = f;
                }
                else
                    fade.overrideState = false;
            }
            else if (name == nameof(ratio))
            {
                if (value is float f)
                {
                    ratio.overrideState = true;
                    ratio.value = f;
                }
                else
                    ratio.overrideState = false;
            }
        }
        public override object GetParameter(string name)
        {
            if (name == nameof(center))
                return center.value;
            else if (name == nameof(radius))
                return radius.value;
            else if (name == nameof(fade))
                return fade.value;
            else if (name == nameof(ratio))
                return ratio.value;
            else
                return null;
        }
        [Tooltip("时间停止中心")]
        public Vector2Parameter center = new() { value = new Vector2(0.5f, 0.5f) };
        [Range(0, 0.75f)]
        [Tooltip("时间停止半径")]
        public FloatParameter radius = new() { value = 0.5f };
        [Range(0, 0.75f)]
        [Tooltip("渐变范围")]
        public FloatParameter fade = new() { value = 0.05f };
        [Range(0, 1)]
        [Tooltip("比例")]
        public FloatParameter ratio = new() { value = 1 };
    }
    public sealed class TimeStopRenderer : PostProcessEffectRenderer<TimeStopSettings>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(Shader.Find("THH/PostProcess/TimeStop"));
            sheet.properties.SetVector("_Center", settings.center);
            sheet.properties.SetFloat("_Radius", settings.radius);
            sheet.properties.SetFloat("_Fade", settings.fade);
            sheet.properties.SetFloat("_Ratio", settings.ratio);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
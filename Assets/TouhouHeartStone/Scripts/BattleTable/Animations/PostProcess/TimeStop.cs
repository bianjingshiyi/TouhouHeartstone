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
                center.overrideState = true;
                center.value = (Vector2)value;
            }
            else if (name == nameof(radius))
            {
                radius.overrideState = true;
                radius.value = (float)value;
            }
        }
        public override void ResetParameter(string name)
        {
            if (name == nameof(center))
                center.overrideState = false;
            else if (name == nameof(radius))
                radius.overrideState = false;
        }
        [Tooltip("时间停止中心")]
        public Vector2Parameter center = new Vector2Parameter { value = new Vector2(0.5f, 0.5f) };
        [Range(0, 0.75f)]
        [Tooltip("时间停止半径")]
        public FloatParameter radius = new FloatParameter { value = 0.5f };
    }
    public sealed class TimeStopRenderer : PostProcessEffectRenderer<TimeStopSettings>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(Shader.Find("THH/PostProcess/TimeStop"));
            sheet.properties.SetVector("_Center", settings.center);
            sheet.properties.SetFloat("_Radius", settings.radius);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
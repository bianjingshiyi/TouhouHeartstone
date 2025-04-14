using System;
using UnityEngine.Rendering.PostProcessing;

namespace Game.PostProcess
{
    public class THHPostProcessEffectSettings : PostProcessEffectSettings
    {
        public virtual void SetParameter(string name, object value)
        {
            throw new NotImplementedException();
        }
        public virtual object GetParameter(string name)
        {
            throw new NotImplementedException();
        }
    }
}
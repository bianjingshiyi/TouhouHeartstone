using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using BJSYGameCore.Animations;
namespace Game
{
    public class ServantMatCtrl : GraphMatPropCtrl
    {
        [SerializeField]
        float _Gray;
        [SerializeField]
        Color _Color;
        protected override void Update()
        {
            material.SetFloat(nameof(_Gray), _Gray);
            material.SetColor(nameof(_Color), _Color);
        }
        protected override void Reset()
        {
            base.Reset();
            _Gray = material.GetFloat(nameof(_Gray));
            _Color = material.GetColor(nameof(_Color));
        }
    }
}

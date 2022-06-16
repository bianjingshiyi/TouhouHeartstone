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
            materials[0].SetFloat(nameof(_Gray), _Gray);
            materials[0].SetColor(nameof(_Color), _Color);
        }
        protected override void Reset()
        {
            base.Reset();
            _Gray = materials[0].GetFloat(nameof(_Gray));
            _Color = materials[0].GetColor(nameof(_Color));
        }
    }
}

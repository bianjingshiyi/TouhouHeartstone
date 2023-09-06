using System.Collections;
using System.Collections.Generic;
using Game.BattleTable;
using UnityEngine;

namespace UI
{
    partial class PropNumber
    {
        protected override void Awake()
        {
            base.Awake();
            init();
        }
        public void setTextColor(PropertyNumberColor color)
        {
            switch (color)
            {
                case PropertyNumberColor.White:
                    WhiteText.invokeBeforeAnim();
                    animator.Play(WhiteText.animName);
                    WhiteText.invokeAfterAnim();
                    break;
                case PropertyNumberColor.Red:
                    RedText.invokeBeforeAnim();
                    animator.Play(RedText.animName);
                    RedText.invokeAfterAnim();
                    break;
                case PropertyNumberColor.Green:
                    GreenText.invokeBeforeAnim();
                    animator.Play(GreenText.animName);
                    GreenText.invokeAfterAnim();
                    break;
            }
        }
        public SimpleAnim RedText;
        public SimpleAnim WhiteText;
        public SimpleAnim GreenText;
    }
}


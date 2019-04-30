﻿using IGensoukyo.Utilities;
using System;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 卡到手牌
    /// </summary>
    public class CardDrawToHand : CardAnimation
    {
        public override string AnimationName => "CardToHand";

        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            var arg = Utilities.CheckType<CardPositionEventArgs>(args);

            var gv = Card.GetComponentInParent<GlobalView>();
            var t = gv.CardPositionCalculator.GetCardHand(arg.GroupID, arg.GroupCount);

            Card.transform.SetSiblingIndex(arg.GroupID);

            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[2] {
                    Card.transform.localPosition,
                    t.Position
                }, new Vector3[2]
                {
                    Card.transform.localRotation.eulerAngles,
                    t.Rotation
                }, callback);
        }

    }
}

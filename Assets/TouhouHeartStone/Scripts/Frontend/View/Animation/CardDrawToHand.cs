using IGensoukyo.Utilities;
using System;
using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    class CardAttackAnimation : CardAnimation
    {
        public override string AnimationName
        {
            get { return "servantAttack"; }
        }
        public override void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            AutoPlayerCardEventArgs autoArgs = args as AutoPlayerCardEventArgs;


        }
    }
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
            if (gv == null)
            {
                callback?.Invoke(null, null);
                return;
            }

            var t = gv.CardPositionCalculator.GetCardHand(arg.GroupID, arg.GroupCount);

            // 设置父物体的层级
            Card.GetComponentInParent<CardViewModel>().transform.SetSiblingIndex(arg.GroupID);

            Card.GetOrAddComponent<PositionAnimation>().Play(new Vector3[2]
            {
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

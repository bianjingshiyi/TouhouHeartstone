using TouhouHeartstone;
using UI;
using UnityEngine;
namespace Game
{
    class SecretTriggerAnim : EventAnimation<THHCard.SecretTriggerEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, THHCard.SecretTriggerEventArg eventArg)
        {
            if (table.tryGetSecret(eventArg.card, out var secret))
            {
                if (_anim == null)
                {
                    secret.Card.GetComponent<PositionLerp>().setTarget(table.ui.CenterDisplayImage.rectTransform);
                }
                if (!SimpleAnimHelper.update(table, ref _anim, secret.onTrigger, secret.animator))
                {
                    table.setCard(secret.Card, eventArg.card, true);
                    return false;
                }
            }
            return true;
        }
    }
}

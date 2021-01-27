using TouhouHeartstone;
using UI;
using UnityEngine;
namespace Game
{
    class SecretTriggerAnim : EventAnimation<THHCard.SecretTriggerEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, THHCard.SecretTriggerEventArg eventArg, float time)
        {
            if (table.tryGetSecret(eventArg.card, out var secret))
            {
                if (_anim == null)
                {
                    secret.getChild("DummyTarget").GetComponent<RectTransform>().position = table.ui.CenterDisplayImage.rectTransform.position;
                }
                if (!SimpleAnimHelper.update(table, ref _anim, secret.onTrigger, secret.animator, time))
                {
                    table.setCard(secret.Card, eventArg.card, true);
                    return false;
                }
            }
            return true;
        }
    }
}

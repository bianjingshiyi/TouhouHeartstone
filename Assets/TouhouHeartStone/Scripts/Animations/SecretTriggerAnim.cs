using TouhouHeartstone;
using UI;
namespace Game
{
    class SecretTriggerAnim : EventAnimation<THHCard.SecretTriggerEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, THHCard.SecretTriggerEventArg eventArg)
        {
            if (table.tryGetSecret(eventArg.card, out var secret))
            {
                table.setCard(secret.Card, eventArg.card, true);
                secret.Card.GetComponent<PositionLerp>().setTarget(table.ui.CenterDisplayImage.rectTransform);
                if (!SimpleAnimHelper.update(table, ref _anim, secret.onTrigger, secret.animator))
                    return false;
            }
            return true;
        }
    }
}

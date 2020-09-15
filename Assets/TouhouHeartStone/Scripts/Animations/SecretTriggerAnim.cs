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
                if (!SimpleAnimHelper.update(table, ref _anim, secret.onTrigger, secret.animator))
                    return false;
            }
            return true;
        }
    }
}

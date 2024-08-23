using System.Collections;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = SecretTriggerEventDefine;
    public class SecretTriggerAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, EVENTVAR.CARD);
            if (table.tryGetSecret(card, out var secret))
            {
                var position = secret.transform.position;
                secret.dummyTarget.position = table.table.CenterDisplayTransform.position;
                table.setCard(secret.card, card);
                secret.card.setFaceup(true);

                AnimAnim triggerAnim = new AnimAnim(secret.animator, secret.onSecretTrigger.animName);
                AnimAnim exitAnim = new AnimAnim(secret.animator, secret.onSecretExit.animName);
                secret.onSecretTrigger.invokeBeforeAnim();
                secret.onSecretExit.invokeBeforeAnim();
                while (AnimHelper.isAnyAnimUpdating(table, _deltaTime, triggerAnim, exitAnim))
                {
                    yield return null;
                }
                secret.onSecretTrigger.invokeAfterAnim();
                secret.onSecretExit.invokeAfterAnim();
                table.removeSecret(card.id);
                table.createDeadCardAnchor(card.id, position);
            }
        }
    }
}

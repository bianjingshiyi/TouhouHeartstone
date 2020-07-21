using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using UI;
using BJSYGameCore.UI;
namespace Game
{
    class UseAnimation : EventAnimation<THHPlayer.UseEventArg>
    {
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = 1f };
        HandToFieldAnim _handToField;
        Timer _targetingTimer = new Timer() { duration = .8f };
        AnimAnim _useAnim;
        AnimAnim _targetedAnim;
        public override bool update(TableManager table, THHPlayer.UseEventArg eventArg)
        {
            if (eventArg.card.define is ServantCardDefine)
            {
                if (eventArg.player == table.player)
                {
                    if (!table.ui.ServantPlaceHolder.Servant.isDisplaying)
                    {
                        HandListItem item = table.getHand(eventArg.card);
                        if (_handToField == null)
                            _handToField = new HandToFieldAnim(table, item, table.ui.SelfFieldList, eventArg.position);
                        if (!_handToField.update(table))
                            return false;
                    }
                    if (tryTargetedAnim(table, eventArg))
                        return false;
                    table.ui.SelfHandList.removeItem(table.getHand(eventArg.card));
                    table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                    table.ui.ServantPlaceHolder.hide();
                }
                else
                {
                    var hand = table.getHand(eventArg.card);
                    if (hand == null)
                        throw new ActorNotFoundException(eventArg.card);
                    if (!table.ui.ServantPlaceHolder.Servant.isDisplaying)
                    {
                        //敌方使用随从
                        if (_handToField == null)
                            _handToField = new HandToFieldAnim(table, hand, table.ui.EnemyFieldList, eventArg.position);
                        if (!_handToField.update(table))
                            return false;
                    }
                    if (tryTargetedAnim(table, eventArg))
                        return false;
                    table.ui.EnemyHandList.removeItem(hand);
                    table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                    table.ui.ServantPlaceHolder.hide();
                }
            }
            else if (eventArg.card.define is SpellCardDefine)
            {
                if (eventArg.player == table.player)
                {
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onSelfUse, hand.animator))
                            return false;
                        table.ui.SelfHandList.removeItem(hand);
                    }
                    if (tryTargetedAnim(table, eventArg))
                        return false;
                }
                else
                {
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        table.setCard(hand.Card, eventArg.card, true);
                        hand.GetComponentInChildren<PositionLerp>().setTarget(table.ui.getChild("SpellDisplay"));
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onEnemyUse, hand.animator))
                            return false;
                        table.ui.EnemyHandList.removeItem(table.getHand(eventArg.card));
                    }
                    if (tryTargetedAnim(table, eventArg))
                        return false;
                }
            }
            else if (eventArg.card.isSkill())
            {
                if (eventArg.player == table.player)
                    table.setSkill(table.ui.SelfSkill, eventArg.card);
                else
                    table.setSkill(table.ui.EnemySkill, eventArg.card);
                if (tryTargetedAnim(table, eventArg))
                    return false;
            }
            else if (eventArg.card.isItem())
            {
                Item item = eventArg.player == table.player ? table.ui.SelfItem : table.ui.EnemyItem;
                if (!item.isDisplaying)
                    table.setItem(item, eventArg.card);
                if (!SimpleAnimHelper.update(table, ref _useAnim, table.ui.EnemyItem.onEquip, table.ui.EnemyItem.animator))
                    return false;
                if (tryTargetedAnim(table, eventArg))
                    return false;
            }
            return true;
        }
        bool tryTargetedAnim(TableManager table, THHPlayer.UseEventArg eventArg)
        {
            if (eventArg.targets != null && eventArg.targets.Length > 0 && eventArg.targets[0] is var targetCard)
            {
                SimpleAnim simpleAnim = null;
                Animator animator = null;
                if (_targetedAnim == null)
                {
                    if (table.tryGetMaster(targetCard, out var targetMaster))
                    {
                        simpleAnim = targetMaster.onTargeted;
                        animator = targetMaster.animator;
                    }
                    else if (table.tryGetServant(targetCard, out var targetServant))
                    {
                        simpleAnim = targetServant.onTargeted;
                        animator = targetServant.animator;
                    }
                    else
                        throw new ActorNotFoundException(targetCard);
                }
                if (!SimpleAnimHelper.update(table, ref _targetedAnim, simpleAnim, animator, next => false))
                    return true;
            }
            return false;
        }
    }
}

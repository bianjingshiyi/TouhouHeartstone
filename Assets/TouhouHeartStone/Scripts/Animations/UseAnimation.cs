using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using UI;
using BJSYGameCore.UI;
using UObject = UnityEngine.Object;
namespace Game
{
    class UseAnimation : EventAnimation<THHPlayer.UseEventArg>
    {
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = 1f };
        HandToFieldAnim _handToField;
        Timer _targetingTimer = new Timer() { duration = .8f };
        AnimAnim _useAnim;
        AnimAnim _equipAnim;
        TableAnimation _targetedAnim;
        public override bool update(TableManager table, THHPlayer.UseEventArg eventArg)
        {
            UIObject sourceActor;
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
                    table.ui.SelfHandList.removeItem(table.getHand(eventArg.card));
                    if (tryTargetedAnim(table, table.ui.ServantPlaceHolder, eventArg))
                        return false;
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
                    table.ui.EnemyHandList.removeItem(hand);
                    if (tryTargetedAnim(table, table.ui.ServantPlaceHolder, eventArg))
                        return false;
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
                    table.getManager<SEManager>().playSE(table.getSkin(eventArg.card).sound_onUse);
                    if (tryTargetedAnim(table, table.ui.SelfMaster, eventArg))
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
                    table.getManager<SEManager>().playSE(table.getSkin(eventArg.card).sound_onUse);
                    if (tryTargetedAnim(table, table.ui.EnemyMaster, eventArg))
                        return false;
                }
            }
            else if (eventArg.card.isSkill())
            {
                if (eventArg.player == table.player)
                {
                    table.setSkill(table.ui.SelfSkill, eventArg.card);
                    sourceActor = table.ui.SelfSkill;
                }
                else
                {
                    table.setSkill(table.ui.EnemySkill, eventArg.card);
                    sourceActor = table.ui.EnemySkill;
                }
                table.getManager<SEManager>().playSE(table.getSkin(eventArg.card).sound_onUse);
                if (tryTargetedAnim(table, sourceActor, eventArg))
                    return false;
            }
            else if (eventArg.card.isItem())
            {
                Item item;
                if (eventArg.player == table.player)
                {
                    item = table.ui.SelfItem;
                    if (!item.isDisplaying)
                    {
                        table.setItem(item, eventArg.card);
                        table.setMaster(table.ui.SelfMaster, eventArg.card.getOwner().master);
                    }
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onSelfUse, hand.animator))
                            return false;
                        table.ui.SelfHandList.removeItem(hand);
                    }
                    if (!SimpleAnimHelper.update(table, ref _equipAnim, table.ui.EnemyItem.onEquip, table.ui.EnemyItem.animator))
                        return false;
                    if (tryTargetedAnim(table, table.ui.SelfItem, eventArg))
                        return false;
                }
                else
                {
                    item = table.ui.EnemyItem;
                    if (!item.isDisplaying)
                        table.setItem(item, eventArg.card);
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        table.setCard(hand.Card, eventArg.card, true);
                        hand.GetComponentInChildren<PositionLerp>().setTarget(table.ui.getChild("SpellDisplay"));
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onEnemyUse, hand.animator))
                            return false;
                        table.ui.EnemyHandList.removeItem(table.getHand(eventArg.card));
                    }
                    if (!SimpleAnimHelper.update(table, ref _equipAnim, table.ui.EnemyItem.onEquip, table.ui.EnemyItem.animator))
                        return false;
                    if (tryTargetedAnim(table, table.ui.EnemyItem, eventArg))
                        return false;
                }
            }
            return true;
        }
        bool tryTargetedAnim(TableManager table, UIObject sourceActor, THHPlayer.UseEventArg eventArg)
        {
            if (eventArg.targets != null && eventArg.targets.Length > 0 && eventArg.targets[0] is var targetCard)
            {
                if (table.getSkin(eventArg.card) is var skin && skin.projectile != null)
                {
                    UIObject targetActor;
                    if (table.tryGetMaster(targetCard, out var targetMaster))
                        targetActor = targetMaster;
                    else if (table.tryGetServant(targetCard, out var targetServant))
                        targetActor = targetServant;
                    else
                        throw new ActorNotFoundException(targetCard);
                    if (_targetedAnim == null)
                    {
                        _targetedAnim = new ProjectileAnimation(
                            UObject.Instantiate(
                                skin.projectile,
                                sourceActor.transform.position,
                                Quaternion.identity,
                                table.ui.FieldsImage.transform),
                            targetActor);
                    }
                    if (!_targetedAnim.update(table))
                        return true;
                }
                else
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
            }
            return false;
        }
    }
}

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
        TableAnimation _moveAnim;
        Timer _targetingTimer = new Timer() { duration = .8f };
        AnimAnim _useAnim;
        AnimAnim _equipAnim;
        TableAnimation _targetedAnim;
        public override bool update(TableManager table, THHPlayer.UseEventArg eventArg, float time)
        {
            UIObject sourceActor;
            if (eventArg.card.define is ServantCardDefine)
            {
                if (eventArg.player == table.player)
                {
                    if (!eventArg.isCanceled)
                    {
                        if (!table.ui.ServantPlaceHolder.isDisplaying)
                        {
                            HandListItem item = table.getHand(eventArg.card);
                            if (_moveAnim == null)
                                _moveAnim = new HandToFieldAnim(table, item, table.ui.SelfFieldList, eventArg.position);
                            if (!_moveAnim.update(table, time))
                                return false;
                        }
                        //table.ui.SelfHandList.removeItem(table.getHand(eventArg.card));
                        //将使用随从的移除手牌转移至TryEnterFiledAnim
                        if (tryTargetedAnim(table, table.ui.ServantPlaceHolder, eventArg, time))
                            return false;
                        table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                        table.ui.ServantPlaceHolder.hide();
                    }
                    else
                    {
                        HandListItem item = table.getHand(eventArg.card);
                        if (item == null)
                            throw new ActorNotFoundException(eventArg.card);
                        if (table.ui.ServantPlaceHolder.isDisplaying)
                        {
                            table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                            table.ui.ServantPlaceHolder.hide();
                        }
                        if (_moveAnim == null)
                        {
                            item.Card.display();
                            table.setCard(item.Card, eventArg.card, true);
                            _moveAnim = new CardMoveAnim(item.Card, item.rectTransform, table.handToFieldCurve);
                        }
                        if (!_moveAnim.update(table, time))
                            return false;
                    }
                }
                else
                {
                    var hand = table.getHand(eventArg.card);
                    if (hand == null && _moveAnim == null)
                        throw new ActorNotFoundException(eventArg.card);
                    if (_moveAnim == null)
                        _moveAnim = new HandToFieldAnim(table, hand, table.ui.EnemyFieldList, eventArg.position);
                    if (hand != null && !_moveAnim.update(table, time))
                        return false;
                    if (!eventArg.isCanceled)
                    {
                        table.ui.EnemyHandList.removeItem(hand);
                        if (tryTargetedAnim(table, table.ui.ServantPlaceHolder, eventArg, time))
                            return false;
                    }
                    else
                    {
                        if (_targetedAnim == null)
                        {
                            _targetedAnim = new CardMoveAnim(hand.Card, hand.rectTransform, table.handToFieldCurve);
                            table.setCard(hand.Card, eventArg.card, true);
                        }
                        if (!_targetedAnim.update(table, time))
                            return false;
                    }
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
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onSelfUse, hand.animator, time))
                            return false;
                        table.ui.SelfHandList.removeItem(hand);
                    }
                    table.getManager<SEManager>().playSE(table.getSkin(eventArg.card).sound_onUse);
                    if (tryTargetedAnim(table, table.ui.SelfMaster, eventArg, time))
                        return false;
                }
                else
                {
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        if (eventArg.card.hasKeyword(table.game, KEYWORD.SECRET))
                            table.setCard(hand.Card, eventArg.card, false);
                        else
                            table.setCard(hand.Card, eventArg.card, true);
                        hand.GetComponentInChildren<PositionLerp>().setTarget(table.ui.getChild("SpellDisplay"));
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onEnemyUse, hand.animator, time))
                            return false;
                        table.ui.EnemyHandList.removeItem(table.getHand(eventArg.card));
                    }
                    table.getManager<SEManager>().playSE(table.getSkin(eventArg.card).sound_onUse);
                    if (tryTargetedAnim(table, table.ui.EnemyMaster, eventArg, time))
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
                if (tryTargetedAnim(table, sourceActor, eventArg, time))
                    return false;
            }
            else if (eventArg.card.isItem())
            {
                if (eventArg.player == table.player)
                {
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onSelfUse, hand.animator, time))
                            return false;
                        table.ui.SelfHandList.removeItem(hand);
                    }
                    table.setMaster(table.ui.SelfMaster, table.player.master);
                    if (tryTargetedAnim(table, table.ui.SelfItem, eventArg, time))
                        return false;
                }
                else
                {
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        table.setCard(hand.Card, eventArg.card, true);
                        hand.GetComponentInChildren<PositionLerp>().setTarget(table.ui.getChild("SpellDisplay"));
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onEnemyUse, hand.animator, time))
                            return false;
                        table.ui.EnemyHandList.removeItem(table.getHand(eventArg.card));
                    }
                    table.setMaster(table.ui.EnemyMaster, table.game.getOpponent(table.player).master);
                    if (tryTargetedAnim(table, table.ui.EnemyItem, eventArg, time))
                        return false;
                }
            }
            return true;
        }
        bool tryTargetedAnim(TableManager table, UIObject sourceActor, THHPlayer.UseEventArg eventArg, float time)
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
                            table.createProjectile(skin.projectile, sourceActor.rectTransform, targetActor.rectTransform),
                            targetActor);
                    }
                    if (!_targetedAnim.update(table, time))
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
                    if (!SimpleAnimHelper.update(table, ref _targetedAnim, simpleAnim, animator, time, next => false))
                        return true;
                }
            }
            return false;
        }
    }
}

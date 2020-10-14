using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using UI;
using BJSYGameCore.UI;
using UObject = UnityEngine.Object;
using TouhouHeartstone.Builtin;

namespace Game
{
    class InvokeAnim : EventAnimation<Sanae.RevealDuelArg>
    {
        Card _cardSelf;
        Card _cardOppo;
        CardMoveAnim _moveToSelf;
        CardMoveAnim _moveToOppo;
        AnimAnim _flipFaceSelf;
        AnimAnim _flipFaceOppo;
        AnimAnim _winAnim;
        AnimAnim _flipBackSelf;
        AnimAnim _flipBackOppo;
        CardMoveAnim _moveBackSelf;
        CardMoveAnim _moveBackOppo;
        public override bool update(TableManager table, Sanae.RevealDuelArg eventArg)
        {
            //在对应的卡组上创建卡牌实例
            _cardSelf = UObject.Instantiate(table.cardPrefab, table.ui.SelfDeck.transform.position, Quaternion.identity, table.ui.SelfDeck.transform);
            table.setCard(_cardSelf, eventArg.card1.getOwner() == table.player ? eventArg.card1 : eventArg.card2, true);
            _cardOppo = UObject.Instantiate(table.cardPrefab, table.ui.EnemyDeck.transform.position, Quaternion.identity, table.ui.EnemyDeck.transform);
            table.setCard(_cardOppo, eventArg.card1.getOwner() != table.player ? eventArg.card1 : eventArg.card2, true);
            //播放位移曲线动画到对应的位置
            if (_moveToSelf == null)
                _moveToSelf = new CardMoveAnim(_cardSelf, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
            if (_moveToOppo == null)
                _moveToOppo = new CardMoveAnim(_cardOppo, table.ui.RightDisplayImage.transform, table.cardShowCurve);
            if (!_moveToSelf.update(table) || !_moveToOppo.update(table))
                return false;
            //翻转揭示
            if (_flipFaceSelf == null)
                _flipFaceSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToFaceAnimName);
            if (_flipFaceOppo == null)
                _flipFaceOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToBackAnimName);
            if (!_flipFaceSelf.update(table) || !_flipFaceOppo.update(table))
                return false;
            //如果胜利应该有一个胜利动画
            if (eventArg.result == true)
                _winAnim = new AnimAnim(_cardSelf.animator, _cardSelf._notifyAnimName);
            if (_winAnim != null && !_winAnim.update(table))
                return false;
            //重新翻回
            if (_flipBackSelf == null)
                _flipBackSelf = new AnimAnim(_cardSelf.animator, _cardSelf._flipToBackAnimName);
            if (_flipBackOppo == null)
                _flipBackOppo = new AnimAnim(_cardOppo.animator, _cardOppo._flipToBackAnimName);
            if (!_flipBackSelf.update(table) || !_flipBackOppo.update(table))
                return false;
            //放回牌库
            if (_moveBackSelf == null)
                _moveBackSelf = new CardMoveAnim(_cardSelf, table.ui.LeftDisplayImage.transform, table.cardShowCurve);
            if (_moveBackOppo == null)
                _moveBackOppo = new CardMoveAnim(_cardOppo, table.ui.RightDisplayImage.transform, table.cardShowCurve);
            if (!_moveBackSelf.update(table) || !_moveBackOppo.update(table))
                return false;
            //销毁实例
            UObject.Destroy(_cardSelf);
            UObject.Destroy(_cardOppo);
            return true;
        }
    }
    class UseAnimation : EventAnimation<THHPlayer.UseEventArg>
    {
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = 1f };
        TableAnimation _moveAnim;
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
                    if (!eventArg.isCanceled)
                    {
                        if (!table.ui.ServantPlaceHolder.isDisplaying)
                        {
                            HandListItem item = table.getHand(eventArg.card);
                            if (_moveAnim == null)
                                _moveAnim = new HandToFieldAnim(table, item, table.ui.SelfFieldList, eventArg.position);
                            if (!_moveAnim.update(table))
                                return false;
                        }
                        //table.ui.SelfHandList.removeItem(table.getHand(eventArg.card));
                        //将使用随从的移除手牌转移至TryEnterFiledAnim
                        if (tryTargetedAnim(table, table.ui.ServantPlaceHolder, eventArg))
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
                        if (!_moveAnim.update(table))
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
                    if (hand != null && !_moveAnim.update(table))
                        return false;
                    if (!eventArg.isCanceled)
                    {
                        table.ui.EnemyHandList.removeItem(hand);
                        if (tryTargetedAnim(table, table.ui.ServantPlaceHolder, eventArg))
                            return false;
                    }
                    else
                    {
                        if (_targetedAnim == null)
                        {
                            _targetedAnim = new CardMoveAnim(hand.Card, hand.rectTransform, table.handToFieldCurve);
                            table.setCard(hand.Card, eventArg.card, true);
                        }
                        if (!_targetedAnim.update(table))
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
                        if (eventArg.card.hasKeyword(table.game, KEYWORD.SECRET))
                            table.setCard(hand.Card, eventArg.card, false);
                        else
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
                if (eventArg.player == table.player)
                {
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onSelfUse, hand.animator))
                            return false;
                        table.ui.SelfHandList.removeItem(hand);
                    }
                    table.setMaster(table.ui.SelfMaster, table.player.master);
                    if (tryTargetedAnim(table, table.ui.SelfItem, eventArg))
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
                    table.setMaster(table.ui.EnemyMaster, table.game.getOpponent(table.player).master);
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
                            table.createProjectile(skin.projectile, sourceActor.rectTransform, targetActor.rectTransform),
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

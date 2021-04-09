using TouhouHeartstone;
using UI;
using TouhouCardEngine;
using UnityEngine;
using BJSYGameCore;
using Timer = BJSYGameCore.Timer;
using Card = TouhouCardEngine.Card;
using UDeck = UI.Deck;
namespace Game
{
    class MoveCardAnim : EventAnimation<Pile.MoveCardEventArg>
    {
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = .4f };
        UI.Card _card;
        TableAnimation _anim;
        TableAnimation _anim2;
        public override bool update(TableManager table, Pile.MoveCardEventArg eventArg, float time)
        {
            if (eventArg.from == null)
            {
                if (eventArg.to.name == PILE.HAND)
                {
                    //发现，印卡
                    if (!table.tryGetHand(eventArg.card, out var hand))
                        hand = table.createHand(eventArg.card);
                    if (!SimpleAnimHelper.update(table, ref _anim, hand.onCreate, hand.CardAnimator, time))
                        return false;
                }
                else if (eventArg.to.name == PILE.HEAD)
                {
                    if (!onSecretEnter(table, eventArg.card, time))
                        return false;
                }
                return true;
            }
            switch (eventArg.from.name)
            {
                case PILE.INIT:
                    if (eventArg.to.name == PILE.HAND)
                    {
                        //初始手牌
                        table.createHand(eventArg.card);
                    }
                    return true;
                case PILE.DECK:
                    {
                        RectTransform deckTransform = eventArg.from == table.player.getDeck() ? table.ui.SelfDeck.rectTransform : table.ui.EnemyDeck.rectTransform;
                        if (eventArg.to.name == PILE.HAND)
                        {
                            //抽牌
                            if (!table.tryGetHand(eventArg.card, out var hand))
                            {
                                hand = table.createHand(eventArg.card);
                                hand.rectTransform.position = deckTransform.position;
                                hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                            }
                            if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator, time))
                                return false;
                        }
                        else if (eventArg.to.name == PILE.WARP)
                        {
                            if (!table.tryGetHand(eventArg.card, out var hand))
                            {
                                hand = table.createHand(eventArg.card);
                                hand.GetComponentInChildren<PositionLerp>().setTarget(deckTransform);
                            }
                            if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator, time))
                                return false;
                            if (!SimpleAnimHelper.update(table, ref _anim2, hand.onDiscard, hand.CardAnimator, time))
                                return false;
                            table.ui.SelfHandList.removeItem(hand);
                            table.ui.EnemyHandList.removeItem(hand);
                        }
                        else if (eventArg.to.name == PILE.HEAD)
                        {
                            if (!onSecretEnter(table, eventArg.card, time))
                                return false;
                        }
                        else if (eventArg.to.name == PILE.GRAVE)
                        {
                            var targetTransform = eventArg.from == table.player.getDeck() ? table.ui.SelfGraveDeck.rectTransform : table.ui.EnemyGraveDeck.rectTransform;
                            if (!table.tryGetHand(eventArg.card, out var hand))
                            {
                                hand = table.createHand(eventArg.card);
                                hand.rectTransform.position = deckTransform.position;
                                hand.GetComponentInChildren<PositionLerp>().setTarget(targetTransform);
                            }
                            if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator, time))
                                return false;
                            table.ui.SelfHandList.removeItem(hand);
                            table.ui.EnemyHandList.removeItem(hand);
                        }
                        return true;
                    }
                case PILE.HAND:
                    {
                        if (table.tryGetHand(eventArg.card, out var hand))
                        {
                            if (eventArg.to.name == PILE.ITEM)
                            {
                                var item = eventArg.to.owner == table.player ? table.ui.SelfItem : table.ui.EnemyItem;
                                table.setItem(item, eventArg.card);
                                item.display();
                                if (!SimpleAnimHelper.update(table, ref _anim, item.onEquip, item.animator, time))
                                    return false;
                            }
                            else if (eventArg.to.name == PILE.GRAVE)
                            {
                                //弃牌
                                hand.GetComponentInChildren<PositionLerp>().resetPosition();
                                hand.GetComponentInChildren<PositionLerp>().setTarget(hand.getChild("GameObject").GetComponent<RectTransform>());
                                if (!SimpleAnimHelper.update(table, ref _anim, hand.onDiscard, hand.animator, time, next =>
                                {
                                    if (next is MoveCardAnim moveCard && moveCard.tEventArg.card != eventArg.card)
                                        return true;
                                    return false;
                                }))
                                    return false;
                                table.ui.SelfHandList.removeItem(hand);
                                table.ui.EnemyHandList.removeItem(hand);
                            }
                            else if (eventArg.to.name == PILE.HEAD)
                            {
                                table.ui.SelfHandList.removeItem(hand);
                                table.ui.EnemyHandList.removeItem(hand);
                                if (!onSecretEnter(table, eventArg.card, time))
                                    return false;
                            }
                            else if (eventArg.to.name == PILE.DECK)
                            {
                                var DeckRectTransform = table.player == eventArg.card.owner ? table.ui.SelfDeck.rectTransform : table.ui.EnemyDeck.rectTransform;
                                hand.GetComponentInChildren<PositionLerp>().resetPosition();
                                hand.GetComponentInChildren<PositionLerp>().setTarget(DeckRectTransform);
                                if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator, time, next =>
                                {
                                    if (next is MoveCardAnim moveCard && moveCard.tEventArg.card != eventArg.card)
                                        return true;
                                    return false;
                                }))
                                    return false;
                                table.ui.SelfHandList.removeItem(hand);
                                table.ui.EnemyHandList.removeItem(hand);
                            }
                        }
                        return true;
                    }
                case PILE.GRAVE:
                    {
                        RectTransform graveTransform = eventArg.from == table.player.getGrave() ? table.ui.SelfGraveDeck.rectTransform : table.ui.EnemyGraveDeck.rectTransform;
                        if(eventArg.to.name == PILE.DECK)
                        {
                            var targetTransform = eventArg.from == table.player.getGrave() ? table.ui.SelfDeck.rectTransform : table.ui.EnemyDeck.rectTransform;
                            if (!table.tryGetHand(eventArg.card, out var hand))
                            {
                                hand = table.createHand(eventArg.card);
                                hand.rectTransform.position = graveTransform.position;
                                hand.GetComponentInChildren<PositionLerp>().setTarget(targetTransform);
                            }
                            if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator, time))
                                return false;
                            table.ui.SelfHandList.removeItem(hand);
                            table.ui.EnemyHandList.removeItem(hand);
                        }
                        if(eventArg.to.name == PILE.GRAVE)
                        {
                            var targetTransform = eventArg.from == table.player.getGrave() ? table.ui.EnemyGraveDeck.rectTransform:table.ui.SelfGraveDeck.rectTransform ;
                            if (!table.tryGetHand(eventArg.card, out var hand))
                            {
                                hand = table.createHand(eventArg.card);
                                hand.rectTransform.position = graveTransform.position;
                                hand.GetComponentInChildren<PositionLerp>().setTarget(targetTransform);
                            }
                            if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator, time))
                                return false;
                            table.ui.SelfHandList.removeItem(hand);
                            table.ui.EnemyHandList.removeItem(hand);
                        }
                        if (eventArg.to.name == PILE.HAND)
                        {
                            //从墓地抽牌
                            if (!table.tryGetHand(eventArg.card, out var hand))
                            {
                                hand = table.createHand(eventArg.card);
                                hand.rectTransform.position = graveTransform.position;
                                hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                            }
                            if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator, time))
                                return false;
                        }
                        else if (eventArg.to.name == PILE.HEAD)
                        {
                            if (!onSecretEnter(table, eventArg.card, time))
                                return false;
                        }
                        return true;
                    }
                case PILE.FIELD:
                    {
                        RectTransform fieldTransform = eventArg.from == table.player.getField() ? table.ui.SelfFieldList.rectTransform : table.ui.SelfFieldList.rectTransform;
                        switch (eventArg.to.name)
                        {
                            case PILE.HAND:
                                {
                                    //回手
                                    if (table.tryGetServant(eventArg.card, out var servant))//尝试获取将要回手的随从
                                    {
                                        table.ui.SelfFieldList.removeItem(servant);
                                        table.ui.EnemyFieldList.removeItem(servant);
                                    }
                                    //关闭沉默开关
                                    if (eventArg.card.isSilenced(table.game))
                                    {
                                        eventArg.card.setProp(table.game, PROPERTY.SILENCE, false);
                                    }
                                    if (!table.tryGetHand(eventArg.card, out var hand))
                                    {
                                        hand = table.createHand(eventArg.card);
                                        hand.rectTransform.position = fieldTransform.position;
                                        hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                                    }
                                    if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator, time))
                                        return false;
                                    return true;
                                }
                            case PILE.FIELD:
                                {
                                    //获得控制权
                                    if (_anim == null)
                                    {
                                        if (table.tryGetServant(eventArg.card, out var servant))
                                            _anim = new FieldMoveAnim(table, servant, eventArg.to == table.player.getField() ? table.ui.SelfFieldList : table.ui.EnemyFieldList, eventArg.position);
                                        else
                                            throw new ActorNotFoundException(eventArg.card);
                                    }
                                    if (!_anim.update(table, time))
                                        return false;
                                    return true;
                                }
                            case PILE.DECK:
                                {
                                    //洗回牌库
                                    if (table.tryGetServant(eventArg.card, out var servant))
                                    {
                                        table.ui.SelfFieldList.removeItem(servant);
                                        table.ui.EnemyFieldList.removeItem(servant);
                                    }
                                    //关闭沉默开关
                                    if (eventArg.card.isSilenced(table.game))
                                    {
                                        eventArg.card.setProp(table.game, PROPERTY.SILENCE, false);
                                    }
                                    //创建卡牌
                                    if (_card == null)
                                        _card = table.createCard(eventArg.card, servant.transform.position, servant.rectTransform.rect.height);
                                    //播放动画
                                    if (_anim == null)
                                    {
                                        UDeck targetDeck = eventArg.to == table.player.getDeck() ? table.ui.SelfDeck : table.ui.EnemyDeck;
                                        _anim = new CardTransAnim(_card,
                                            targetDeck.transform,
                                            targetDeck.bottom.rect.height * targetDeck.bottom.lossyScale / _card.rectTransform.rect.height,
                                            table.fieldToDeckCurve);
                                    }

                                    if (!_anim.update(table, time))//没放完动画就让它接着放
                                        return false;
                                    //摧毁卡牌
                                    Object.Destroy(_card.gameObject);
                                    return true;//放完了
                                }
                            default:
                                return true;
                        }
                    }
                case PILE.HEAD:
                    if (!onSecretExit(table, eventArg.card, time))
                        return false;
                    return true;
                case PILE.WARP:
                    if (eventArg.to.name == PILE.HEAD)
                    {
                        if (!onSecretEnter(table, eventArg.card, time))
                            return false;
                    }
                    return true;
                default:
                    return true;
            }
        }
        /// <summary>
        /// 奥秘入场
        /// </summary>
        /// <param name="table"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        bool onSecretEnter(TableManager table, Card card, float time)
        {
            if (!table.tryGetSecret(card, out var secret))
            {
                secret = table.createSecret(card);
            }
            if (!SimpleAnimHelper.update(table, ref _anim, secret.onEnter, secret.animator, time))
                return false;
            return true;
        }
        bool onSecretExit(TableManager table, Card card, float time)
        {
            if (table.tryGetSecret(card, out var secret))
            {
                if (SimpleAnimHelper.update(table, ref _anim, secret.onExit, secret.animator, time))
                    return false;
                table.removeSecret(card);
            }
            return true;
        }
    }
}

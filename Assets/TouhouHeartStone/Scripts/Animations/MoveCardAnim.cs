﻿using TouhouHeartstone;
using UI;
using TouhouCardEngine;
using UnityEngine;
using BJSYGameCore;
using Timer = BJSYGameCore.Timer;
namespace Game
{
    class MoveCardAnim : EventAnimation<Pile.MoveCardEventArg>
    {
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = .4f };
        HandListItem _item;
        AnimAnim _anim;
        AnimAnim _anim2;
        public override bool update(TableManager table, Pile.MoveCardEventArg eventArg)
        {
            if (eventArg.from == null)
            {
                if (eventArg.to.name == PileName.HAND)
                {
                    //发现，印卡
                    if (!table.tryGetHand(eventArg.card, out var hand))
                        hand = table.createHand(eventArg.card);
                    if (!SimpleAnimHelper.update(table, ref _anim, hand.onCreate, hand.animator))
                        return false;
                }
            }
            else if (eventArg.from.name == PileName.INIT)
            {
                if (eventArg.to.name == PileName.HAND)
                {
                    //初始手牌
                    table.createHand(eventArg.card);
                }
            }
            else if (eventArg.from.name == PileName.DECK)
            {
                RectTransform deckTransform = eventArg.from == table.player.deck ? table.ui.SelfDeck.rectTransform : table.ui.EnemyDeck.rectTransform;
                if (eventArg.to.name == PileName.HAND)
                {
                    //抽牌
                    if (!table.tryGetHand(eventArg.card, out var hand))
                    {
                        hand = table.createHand(eventArg.card);
                        hand.rectTransform.position = deckTransform.position;
                        hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                    }
                    if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator))
                        return false;
                    //if (!_timer.isStarted)
                    //{
                    //    _item = table.createHand(eventArg.card);
                    //    if (eventArg.from.owner == table.player)
                    //    {
                    //        _item.Card.rectTransform.position = table.ui.SelfDeck.rectTransform.position;
                    //        _startPosition = table.ui.SelfDeck.rectTransform.position;
                    //    }
                    //    else
                    //    {
                    //        _item.Card.rectTransform.position = table.ui.EnemyDeck.rectTransform.position;
                    //        _startPosition = table.ui.EnemyDeck.rectTransform.position;
                    //    }
                    //    _timer.start();
                    //}
                    //_item.Card.rectTransform.position = Vector3.Lerp(_startPosition, _item.rectTransform.position, _item.Card.drawCurve.Evaluate(_timer.progress));
                    //if (!_timer.isExpired())
                    //    return false;
                }
                else if (eventArg.to.name == PileName.WARP)
                {
                    if (!table.tryGetHand(eventArg.card, out var hand))
                    {
                        hand = table.createHand(eventArg.card);
                        hand.GetComponentInChildren<PositionLerp>().setTarget(deckTransform);
                    }
                    if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator))
                        return false;
                    if (!SimpleAnimHelper.update(table, ref _anim2, hand.onDiscard, hand.animator))
                        return false;
                    table.ui.SelfHandList.removeItem(hand);
                    table.ui.EnemyHandList.removeItem(hand);
                }
            }
            else if (eventArg.from.name == PileName.HAND)
            {
                if (table.tryGetHand(eventArg.card, out var hand))
                {
                    if (eventArg.to.name == PileName.GRAVE)
                    {
                        //弃牌
                        if (!SimpleAnimHelper.update(table, ref _anim, hand.onDiscard, hand.animator, next =>
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
            }
            else if (eventArg.from.name == PileName.GRAVE)
            {
                RectTransform graveTransform = eventArg.from == table.player.grave ? table.ui.SelfGraveDeck.rectTransform : table.ui.EnemyGraveDeck.rectTransform;
                if (eventArg.to.name == PileName.HAND)
                {
                    //从墓地抽牌
                    if (!table.tryGetHand(eventArg.card, out var hand))
                    {
                        hand = table.createHand(eventArg.card);
                        hand.rectTransform.position = graveTransform.position;
                        hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                    }
                    if (!SimpleAnimHelper.update(table, ref _anim, hand.onDraw, hand.animator))
                        return false;
                }
            }
            return true;
        }
    }
}

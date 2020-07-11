using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using System.Linq;
using UI;
using System;
namespace Game
{
    class CodeAnim : TableAnimation
    {
        Action _action;
        public CodeAnim(Action action)
        {
            _action = action;
        }
        public override bool update(TableManager table)
        {
            _action();
            return true;
        }
    }
    class HandToFieldAnim : TableAnimation
    {
        HandListItem _item;
        Vector3 _startPosition;
        Timer _timer;
        public HandToFieldAnim(TableManager table, HandListItem item, FieldList field, int index)
        {
            _item = item;
            _startPosition = item.Card.rectTransform.position;
            _timer = new Timer() { duration = table.handToFieldCurve.keys.Last().time };

            field.addChild(table.ui.ServantPlaceHolder.rectTransform);
            field.defaultItem.rectTransform.SetAsFirstSibling();
            table.ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
            table.ui.ServantPlaceHolder.display();
            _startPosition = _item.Card.rectTransform.position;
            _timer.start();
        }
        public override bool update(TableManager table)
        {
            if (!_timer.isExpired())
            {
                _item.Card.rectTransform.position = Vector3.Lerp(_startPosition, table.ui.ServantPlaceHolder.rectTransform.position, table.handToFieldCurve.Evaluate(_timer.time));
                return false;
            }
            return true;
        }
    }
    class UseAnimation : EventAnimation<THHPlayer.UseEventArg>
    {
        Vector3 _startPosition;
        Timer _timer;
        HandToFieldAnim _handToField;
        Timer _targetingTimer = new Timer() { duration = .8f };
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
                    table.ui.SelfHandList.removeItem(table.getHand(eventArg.card));
                    table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                    table.ui.ServantPlaceHolder.hide();
                    if (selectTarget(table, eventArg))
                        return false;
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
                    table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                    table.ui.ServantPlaceHolder.hide();
                    if (selectTarget(table, eventArg))
                        return false;
                }
            }
            else if (eventArg.card.define is SpellCardDefine)
            {
                if (eventArg.player == table.player)
                {
                    table.ui.SelfHandList.removeItem(table.getHand(eventArg.card));
                    if (selectTarget(table, eventArg))
                        return false;
                }
                else
                {
                    var hand = table.getHand(eventArg.card);
                    if (hand == null)
                        throw new ActorNotFoundException(eventArg.card);
                    if (!_timer.isStarted)
                    {
                        _startPosition = hand.Card.rectTransform.position;
                        _timer.start();
                    }
                    if (_timer.progress <= .4f)
                        hand.Card.rectTransform.position = Vector3.Lerp(_startPosition, table.ui.getChild("SpellDisplay").position, hand.Card.useCurve.Evaluate(_timer.progress / .4f));
                    else
                        hand.Card.rectTransform.position = table.ui.getChild("SpellDisplay").position;
                    if (!_timer.isExpired())
                        return false;
                    table.ui.EnemyHandList.removeItem(hand);
                    if (selectTarget(table, eventArg))
                        return false;
                }
            }
            return true;
        }
        bool selectTarget(TableManager table, THHPlayer.UseEventArg eventArg)
        {
            if (eventArg.targets != null && eventArg.targets.Length > 0 && eventArg.targets[0] is var target)
            {
                if (table.tryGetServant(target, out var targetServant))
                {
                    if (!_targetingTimer.isStarted)
                    {
                        targetServant.animator.Play("Targeted");
                        _targetingTimer.start();
                    }
                    if (!_targetingTimer.isExpired())
                        return true;
                }
            }
            return false;
        }
    }
}

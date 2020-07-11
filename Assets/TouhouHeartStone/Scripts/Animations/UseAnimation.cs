using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using UI;
namespace Game
{
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
                    if (selectTarget(table, eventArg))
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
                    if (selectTarget(table, eventArg))
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

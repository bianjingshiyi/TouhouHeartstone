using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using System.Linq;
using UI;
namespace Game
{
    class UseAnimation : EventAnimation<THHPlayer.UseEventArg>
    {
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = .6f };
        public override bool update(TableManager table, THHPlayer.UseEventArg eventArg)
        {
            if (eventArg.card.define is ServantCardDefine)
            {
                if (eventArg.player == table.player)
                {
                    if (!table.ui.ServantPlaceHolder.Servant.isDisplaying)
                    {
                        HandListItem item = table.getHand(eventArg.card);
                        if (!_timer.isStarted)
                        {
                            _startPosition = item.Card.rectTransform.position;
                            _timer.start();
                        }
                        item.Card.rectTransform.position = Vector3.Lerp(_startPosition, table.ui.ServantPlaceHolder.rectTransform.position, item.Card.useCurve.Evaluate(_timer.progress));
                        if (!_timer.isExpired())
                            return false;
                    }
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
                        if (!_timer.isStarted)
                        {
                            table.ui.EnemyFieldList.addChild(table.ui.ServantPlaceHolder.rectTransform);
                            table.ui.EnemyFieldList.defaultItem.rectTransform.SetAsFirstSibling();
                            table.ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(eventArg.position + 1);
                            table.ui.ServantPlaceHolder.display();
                            _startPosition = hand.Card.rectTransform.position;
                            _timer.start();
                        }
                        hand.Card.rectTransform.position = Vector3.Lerp(_startPosition, table.ui.ServantPlaceHolder.rectTransform.position, hand.Card.useCurve.Evaluate(_timer.progress));
                        if (!_timer.isExpired())
                            return false;
                    }
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
                }
            }
            return true;
        }
    }
}

using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using System.Linq;
namespace UI
{
    class UseServantAnimation : UIAnimation
    {
        public THHPlayer.UseEventArg eventArg { get; }
        public UseServantAnimation(THHPlayer.UseEventArg eventArg)
        {
            this.eventArg = eventArg;
        }
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = .6f };
        public override bool update(Table table)
        {
            if (eventArg.player == table.player)
            {
                if (!table.ServantPlaceHolder.Servant.isDisplaying)
                {
                    Card card = table.usingHand.Card;
                    if (!_timer.isStarted)
                    {
                        _startPosition = card.rectTransform.position;
                        _timer.start();
                    }
                    card.rectTransform.position = Vector3.Lerp(_startPosition, table.ServantPlaceHolder.rectTransform.position, card.useCurve.Evaluate(_timer.progress));
                    if (!_timer.isExpired())
                        return false;
                }
                table.SelfHandList.removeItem(table.usingHand.Card.GetComponentInParent<HandListItem>());
                table.addChild(table.ServantPlaceHolder.rectTransform);
                table.ServantPlaceHolder.hide();
            }
            else
            {
                var hand = table.EnemyHandList.FirstOrDefault(i => i.Card.card == eventArg.card);
                if (hand == null)
                    throw new ActorNotFoundException(eventArg.card);
                if (!table.ServantPlaceHolder.Servant.isDisplaying)
                {
                    //敌方使用随从
                    if (!_timer.isStarted)
                    {
                        table.EnemyFieldList.addChild(table.ServantPlaceHolder.rectTransform);
                        table.EnemyFieldList.defaultItem.rectTransform.SetAsFirstSibling();
                        table.ServantPlaceHolder.rectTransform.SetSiblingIndex(eventArg.position + 1);
                        table.ServantPlaceHolder.display();
                        _startPosition = hand.Card.rectTransform.position;
                        _timer.start();
                    }
                    hand.Card.rectTransform.position = Vector3.Lerp(_startPosition, table.ServantPlaceHolder.rectTransform.position, hand.Card.useCurve.Evaluate(_timer.progress));
                    if (!_timer.isExpired())
                        return false;
                }
                table.EnemyHandList.removeItem(hand);
                table.addChild(table.ServantPlaceHolder.rectTransform);
                table.ServantPlaceHolder.hide();
            }
            return true;
        }
    }
}

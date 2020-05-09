using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using System.Linq;
namespace UI
{
    class UseSpellAnimation : Animation
    {
        public THHPlayer.UseEventArg eventArg { get; }
        public UseSpellAnimation(THHPlayer.UseEventArg eventArg)
        {
            this.eventArg = eventArg;
        }
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = 1 };
        public override bool update(Table table)
        {
            if (eventArg.player == table.player)
            {
                table.SelfHandList.removeItem(table.SelfHandList.placingCard.GetComponentInParent<HandListItem>());
            }
            else
            {
                var hand = table.EnemyHandList.FirstOrDefault(i => i.Card.card == eventArg.card);
                if (hand == null)
                    throw new ActorNotFoundException(eventArg.card);
                if (!_timer.isStarted)
                {
                    _startPosition = hand.Card.rectTransform.position;
                    _timer.start();
                }
                if (_timer.progress <= .4f)
                    hand.Card.rectTransform.position = Vector3.Lerp(_startPosition, table.getChild("SpellDisplay").position, hand.Card.useCurve.Evaluate(_timer.progress / .4f));
                else
                    hand.Card.rectTransform.position = table.getChild("SpellDisplay").position;
                if (!_timer.isExpired())
                    return false;
                table.EnemyHandList.removeItem(hand);
            }
            return true;
        }
    }
}

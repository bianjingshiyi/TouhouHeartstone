using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore;
namespace UI
{
    class DrawAnimation : Animation<THHPlayer.DrawEventArg>
    {
        public DrawAnimation(THHPlayer.DrawEventArg eventArg) : base(eventArg)
        {
        }
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = .4f };
        HandListItem _item;
        public override bool update(Table table)
        {
            if (!_timer.isStarted)
            {
                if (eventArg.player == table.player)
                {
                    _item = table.SelfHandList.addItem();
                    _item.Card.update(eventArg.card, table.getSkin(eventArg.card));
                    _item.Card.rectTransform.position = table.SelfDeck.rectTransform.position;
                    _startPosition = table.SelfDeck.rectTransform.position;
                }
                else
                {
                    _item = table.EnemyHandList.addItem();
                    _item.Card.update(eventArg.card, null);
                    _item.Card.rectTransform.position = table.EnemyDeck.rectTransform.position;
                    _startPosition = table.EnemyDeck.rectTransform.position;
                }
                _timer.start();
            }
            _item.Card.rectTransform.position = Vector3.Lerp(_startPosition, _item.rectTransform.position, _item.Card.drawCurve.Evaluate(_timer.progress));
            if (!_timer.isExpired())
                return false;
            return true;
        }
    }
}

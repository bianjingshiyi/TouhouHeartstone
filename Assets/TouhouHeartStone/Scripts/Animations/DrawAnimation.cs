using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore;
using UI;
namespace Game
{
    class DrawAnimation : EventAnimation<THHPlayer.DrawEventArg>
    {
        Vector3 _startPosition;
        Timer _timer = new Timer() { duration = .4f };
        HandListItem _item;
        public override bool update(TableManager table, THHPlayer.DrawEventArg eventArg)
        {
            if (!_timer.isStarted)
            {
                _item = table.createHand(eventArg.card);
                if (eventArg.player == table.player)
                {
                    _item.Card.rectTransform.position = table.ui.SelfDeck.rectTransform.position;
                    _startPosition = table.ui.SelfDeck.rectTransform.position;
                }
                else
                {
                    _item.Card.rectTransform.position = table.ui.EnemyDeck.rectTransform.position;
                    _startPosition = table.ui.EnemyDeck.rectTransform.position;
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

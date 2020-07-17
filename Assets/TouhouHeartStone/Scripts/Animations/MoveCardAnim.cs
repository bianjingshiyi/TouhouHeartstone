using TouhouHeartstone;
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
        public override bool update(TableManager table, Pile.MoveCardEventArg eventArg)
        {
            if (eventArg.from == null)
            {
                if (eventArg.to.name == PileName.HAND)
                {
                    //发现，印卡
                    if (eventArg.to == table.player.hand)
                    {
                        table.createHand(eventArg.card);
                    }
                }
            }
            else if (eventArg.from.name == PileName.DECK)
            {
                if (eventArg.to.name == PileName.HAND)
                {
                    //抽牌
                    if (!_timer.isStarted)
                    {
                        _item = table.createHand(eventArg.card);
                        if (eventArg.from.owner == table.player)
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
                }
            }
            return true;
        }
    }
}

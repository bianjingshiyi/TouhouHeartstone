using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore;
using UI;
namespace Game
{
    class BurnAnimation : EventAnimation<THHPlayer.BurnEventArg>
    {
        Timer _timer = new Timer() { duration = .2f };
        Card _card;
        public override bool update(TableManager table, THHPlayer.BurnEventArg eventArg)
        {
            if (!_timer.isStarted)
            {
                if (eventArg.player == table.player)
                    _card = UnityEngine.Object.Instantiate(table.ui.SelfDeck.Card_5, table.ui.SelfDeck.rectTransform.position, Quaternion.identity, table.ui.rectTransform);
                else
                    _card = UnityEngine.Object.Instantiate(table.ui.EnemyDeck.Card_5, table.ui.EnemyDeck.rectTransform.position, Quaternion.identity, table.ui.rectTransform);
                table.setCard(_card, eventArg.card, true);
                _timer.start();
            }
            if (eventArg.player == table.player)
                _card.rectTransform.position = Vector3.Lerp(table.ui.SelfDeck.rectTransform.position, table.ui.SelfGraveDeck.rectTransform.position, _timer.progress);
            else
                _card.rectTransform.position = Vector3.Lerp(table.ui.EnemyDeck.rectTransform.position, table.ui.EnemyGraveDeck.rectTransform.position, _timer.progress);
            if (!_timer.isExpired())
                return false;
            UnityEngine.Object.Destroy(_card.gameObject);
            return true;
        }
    }
}

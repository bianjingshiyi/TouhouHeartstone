using TouhouHeartstone;
using UnityEngine;
using UI;
using TouhouCardEngine;
using Timer = BJSYGameCore.Timer;
using System.Collections;

namespace Game
{
    using Arg = HearthStoneRule.BurnEventArg;
    class BurnAnimation : EventAnimation<Arg>
    {
        Timer _timer = new Timer() { duration = .2f };

        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            yield break; // 以MoveCardAnim暂时代替

            while (!record.isCompleted)
                yield return null;
            if (record.isCanceled)
                yield break;

            var player = arg.player;
            var card = table.getRecordCardSnapshot(record, EVENTVAR.CARD);

            var deck = player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck;
            var grave = player == table.player ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;

            var cardItem = Object.Instantiate(deck.Card_5, deck.rectTransform.position, Quaternion.identity, table.ui.rectTransform);
            table.setCard(cardItem, card, true);
            _timer.start();
            while (!_timer.isExpired())
            {
                cardItem.rectTransform.position = Vector3.Lerp(deck.rectTransform.position, grave.rectTransform.position, _timer.progress);
                yield return null;
            }
            Object.Destroy(cardItem.gameObject);
        }
    }
}

using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using Timer = BJSYGameCore.Timer;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.BurnEventArg;
    class BurnAnimation : EventAnimation<Arg>
    {
        Timer _timer = new Timer() { duration = .2f };

        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            yield break; // 以MoveCardAnim暂时代替

            while (!record.isCompleted)
                yield return null;
            if (record.isCanceled)
                yield break;

            var player = arg.player;
            var card = table.getRecordCardSnapshot(record, EVENTVAR.CARD);

            var deck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
            var grave = player == table.player ? table.table.SelfGrave : table.table.EnemyGrave;

            var cardItem = table.createCard(card, deck.transform.position, -1, true);
            _timer.start();
            while (!_timer.isExpired())
            {
                cardItem.transform.position = Vector3.Lerp(deck.transform.position, grave.transform.position, _timer.progress);
                yield return null;
            }
            Object.Destroy(cardItem.gameObject);
        }
    }
}

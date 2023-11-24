using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using Timer = BJSYGameCore.Timer;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.BurnEventArg;

    internal class BurnAnimation : EventAnimation<Arg>
    {
        private Timer _timer = new Timer() { duration = .2f };

        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            yield break; // 以MoveCardAnim暂时代替

            var player = record.getVar<Player>(EVENTVAR.PLAYER);
            var card = table.getEventCardSnapshot(record, EVENTVAR.CARD);

            var deck = player == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
            var grave = player == table.player ? table.table.SelfGrave : table.table.EnemyGrave;

            var cardPos = table.WorldToTablePosition(deck.HoverTransform.position);
            var cardItem = table.createCard(card, cardPos, -1, true);
            _timer.start();
            while (!_timer.isExpired())
            {
                cardItem.transform.position = Vector3.Lerp(deck.HoverTransform.position, grave.HoverTransform.position, _timer.progress);
                yield return null;
            }
            Object.Destroy(cardItem.gameObject);
        }
    }
}
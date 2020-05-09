using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore;
namespace UI
{
    class BurnAnimation : Animation<THHPlayer.BurnEventArg>
    {
        public BurnAnimation(THHPlayer.BurnEventArg eventArg) : base(eventArg)
        {
        }
        Timer _timer = new Timer() { duration = .2f };
        Card _card;
        public override bool update(Table table)
        {
            if (!_timer.isStarted)
            {
                if (eventArg.player == table.player)
                    _card = UnityEngine.Object.Instantiate(table.SelfDeck.Card_5, table.SelfDeck.rectTransform.position, Quaternion.identity, table.rectTransform);
                else
                    _card = UnityEngine.Object.Instantiate(table.EnemyDeck.Card_5, table.EnemyDeck.rectTransform.position, Quaternion.identity, table.rectTransform);
                _card.update(eventArg.card, table.getSkin(eventArg.card));
                _timer.start();
            }
            if (eventArg.player == table.player)
                _card.rectTransform.position = Vector3.Lerp(table.SelfDeck.rectTransform.position, table.SelfGraveDeck.rectTransform.position, _timer.progress);
            else
                _card.rectTransform.position = Vector3.Lerp(table.EnemyDeck.rectTransform.position, table.EnemyGraveDeck.rectTransform.position, _timer.progress);
            if (!_timer.isExpired())
                return false;
            UnityEngine.Object.Destroy(_card.gameObject);
            return true;
        }
    }
}

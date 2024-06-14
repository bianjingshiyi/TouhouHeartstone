using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace Game.BattleTable
{
    public abstract class CardMoveAnim : TableAnimation
    {
        public CardMoveAnim(CardDescriptor cardDesc, Vector3 startTablePos, Pile from, Pile to, bool startVisible)
        {
            _cardDesc = cardDesc;
            _startTablePos = startTablePos;
            _from = from;
            _to = to;
            _startVisible = startVisible;
        }
        public virtual IEnumerator startUpdate(BattleTableManager table, float deltaTime)
        {
            yield break;
        }
        public virtual IEnumerator logicUpdate(BattleTableManager table, float deltaTime)
        {
            yield break;
        }
        public virtual IEnumerator cancelUpdate(BattleTableManager table, float deltaTime)
        {
            yield break;
        }
        public sealed override IEnumerator update(BattleTableManager table)
        {
            yield return startUpdate(table, _deltaTime);
            yield return logicUpdate(table, _deltaTime);
        }
        public virtual Vector3 getCurrentPosition(BattleTableManager table)
        {
            var transform = table.getCardTransform(_cardDesc.id);
            if (transform)
                return table.WorldToTablePosition(transform.position);
            return _startTablePos;
        }
        protected Vector3 getMiddleTargetPosition(BattleTableManager table, Vector3 startWorldPos, Vector3 targetWorldPos)
        {
            if (_from?.owner == table.player || _to?.owner == table.player)
            {
                return table.getCardMoveMiddleWorldPosition(startWorldPos, targetWorldPos);
            }
            return (startWorldPos + targetWorldPos) * 0.5f;
        }
        protected void updateStartFaceup(BattleTableManager table, TableCard card)
        {
            bool startFaceup = isPileOnTable(_from) && table.canSeeCard(_cardDesc);
            bool midFaceup = startFaceup || willThisMoveShowTheCard(table);

            card.setFaceup(startFaceup);
            if (startFaceup != midFaceup)
            {
                card.animateFaceup(midFaceup);
            }
        }
        protected void updateEndFaceup(BattleTableManager table, TableCard card)
        {
            var endFaceup = willThisMoveShowTheCard(table) && isPileOnTable(_to);
            if (card.isFaceup != endFaceup)
            {
                card.animateFaceup(endFaceup);
            }
        }
        protected int removeCard(BattleTableManager table)
        {
            var cardId = _cardDesc.id;
            int removed = 0;
            if (table.tryGetHand(cardId, out _))
            {
                removed += table.removeHand(cardId) ? 1 : 0;
            }
            if (table.tryGetMovingCard(cardId, out _))
            {
                removed += table.removeMovingCard(cardId) ? 1 : 0;
            }
            if (table.tryGetServant(cardId, out _))
            {
                removed += table.removeServant(cardId) ? 1 : 0;
            }
            if (table.tryGetDeckOfCard(cardId, out _))
            {
                removed += table.removeCardFromDeck(cardId) ? 1 : 0;
            }
            if (table.tryGetGraveOfCard(cardId, out _))
            {
                removed += table.removeCardFromGrave(cardId) ? 1 : 0;
            }
            if (table.tryGetItem(cardId, out _))
            {
                removed += table.removeItem(cardId) ? 1 : 0;
            }
            if (table.tryGetSecret(cardId, out _))
            {
                removed += table.removeSecret(cardId) ? 1 : 0;
            }
            return removed;
        }
        private bool willThisMoveShowTheCard(BattleTableManager table)
        {
            return _to.isVisiblePileTo(table.player) || table.canSeeCard(_cardDesc);
        }
        /// <summary>
        /// 牌堆是否位于桌面上，并且有卡牌可以显示（牌库和墓地不算）
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        private static bool isPileOnTable(Pile pile)
        {
            return pile.isGeneralizedBattlefield() || pile?.name == PILE.HAND;
        }
        public Vector3 startPosition
        {
            get => _startTablePos;
            set => _startTablePos = value;
        }
        protected bool _startVisible;
        protected CardDescriptor _cardDesc;
        protected Vector3 _startTablePos;
        protected Pile _from;
        protected Pile _to;
    }
}

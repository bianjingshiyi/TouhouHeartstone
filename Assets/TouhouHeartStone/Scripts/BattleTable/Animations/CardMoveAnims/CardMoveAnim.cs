using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace Game.BattleTable
{
    public abstract class CardMoveAnim : TableAnimation
    {
        public CardMoveAnim(CardDescriptor cardDesc, Vector3 startTablePos, Pile from, Pile to)
        {
            _cardDesc = cardDesc;
            _startTablePos = startTablePos;
            _from = from;
            _to = to;
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
        public abstract Vector3 getCurrentPosition(BattleTableManager table);
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
            bool startFaceup = getPileFaceup(table, _from);
            bool endFaceup = table.canSeeCardOfOwner(_from?.owner) || table.canSeeCardOfOwner(_to?.owner) || _from.isGeneralizedBattlefield() || _to.isGeneralizedBattlefield();

            card.setFaceup(startFaceup);
            if (startFaceup != endFaceup)
            {
                card.animateFaceup(endFaceup);
            }
        }
        protected void updateEndFaceup(BattleTableManager table, TableCard card)
        {
            var endFaceup = getPileFaceup(table, _to);
            if (card.isFaceup != endFaceup)
            {
                card.animateFaceup(endFaceup);
            }
        }
        protected bool removeCard(BattleTableManager table)
        {
            var cardId = _cardDesc.id;
            if (table.tryGetHand(cardId, out _))
            {
                return table.removeHand(cardId);
            }
            else if (table.tryGetServant(cardId, out _))
            {
                return table.removeServant(cardId);
            }
            else if (table.tryGetDeckOfCard(cardId, out _))
            {
                return table.removeCardFromDeck(cardId);
            }
            else if (table.tryGetGraveOfCard(cardId, out _))
            {
                return table.removeCardFromGrave(cardId);
            }
            else if (table.tryGetItem(cardId, out _))
            {
                return table.removeItem(cardId);
            }
            else if (table.tryGetSecret(cardId, out _))
            {
                return table.removeSecret(cardId);
            }
            return false;
        }
        private static bool getPileFaceup(BattleTableManager table, Pile pile)
        {
            var faceUp = false;
            if (pile.isGeneralizedBattlefield())
            {
                faceUp = true;
            }
            else if (pile?.name == PILE.HAND)
            {
                faceUp = table.canSeeCardOfOwner(pile.owner);
            }
            return faceUp;
        }
        protected CardDescriptor _cardDesc;
        protected Vector3 _startTablePos;
        protected Pile _from;
        protected Pile _to;
    }
}

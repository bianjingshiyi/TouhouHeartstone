using System.Collections.Generic;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace Game.BattleTable
{
    public partial class BattleTableManager
    {
        public CardSnapshot newSnapshot(Card card)
        {
            return game.snapshotCard(card);
        }
        public CardSnapshot getEventCardSnapshot(EventAnimStartData animData, string varName)
        {
            var state = animData.getVar<CardState>(varName);
            if (state.card == null || state.stateIndex < 0)
                return null;
            var snapshot = newSnapshot(state.card);
            state.card.revertToHistory(snapshot, state.stateIndex);
            return snapshot;
        }
        public CardSnapshot getEventCardSnapshot(EventRecord animData, string varName)
        {
            var state = animData.getVar<CardState>(varName);
            if (state.card == null || state.stateIndex < 0)
                return null;
            var snapshot = newSnapshot(state.card);
            state.card.revertToHistory(snapshot, state.stateIndex);
            return snapshot;
        }
        public CardDescriptor getCardDescriptor(Card card)
        {
            return getCardDescriptor(card.id);
        }
        public CardDescriptor getCardDescriptor(int id)
        {
            if (_descriptors.TryGetValue(id, out var desc))
            {
                return desc;
            }
            return null;
        }
        public void setCardDescriptor(CardDescriptor desc)
        {
            if (_descriptors.ContainsKey(desc.id))
            {
                _descriptors[desc.id] = desc;
            }
            else
            {
                _descriptors.Add(desc.id, desc);
            }
        }
        public CardDescriptor updateCardDescriptor(ICardData cardData)
        {
            if (cardData == null)
                return null;
            if (_descriptors.TryGetValue(cardData.id, out CardDescriptor desc))
            {
                desc.Update(game, cardData);
            }
            else
            {
                desc = new CardDescriptor(game, cardData);
                _descriptors.Add(cardData.id, desc);
            }
            return desc;
        }
        private Dictionary<int, CardDescriptor> _descriptors = new Dictionary<int, CardDescriptor>();
    }
}

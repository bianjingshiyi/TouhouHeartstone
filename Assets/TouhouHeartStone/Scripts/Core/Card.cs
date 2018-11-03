using System;
using System.Collections;

using UnityEngine;

namespace TouhouHeartstone
{
    class Card
    {
        public Card(CardInstance instance)
        {
            this.instance = instance;
        }
        public CardInstance instance { get; private set; }
    }
    [Serializable]
    public class CardInstance
    {
        public int instanceId { get; }
        public int cardId { get; }
        public CardInstance(int instanceId, int cardId)
        {
            this.instanceId = instanceId;
            this.cardId = cardId;
        }
    }
}
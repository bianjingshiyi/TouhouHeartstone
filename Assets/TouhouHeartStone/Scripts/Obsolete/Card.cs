using System;
using System.Collections;

using UnityEngine;

namespace TouhouHeartstone
{
    class CardLogic
    {
        public CardLogic(CardInstance instance)
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
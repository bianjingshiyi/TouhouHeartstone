using System;
using System.Collections;

using UnityEngine;

namespace TouhouHeartstone
{
    public class Card : MonoBehaviour
    {
        public static Card create(CardInstance instance)
        {
            Card card = new GameObject("Card (" + instance.instanceId + ")").AddComponent<Card>();
            card._instance = instance;
            return card;
        }
        public static Card create(int instanceId, int cardId)
        {
            Card card = new GameObject("Card (" + instanceId + ")").AddComponent<Card>();
            card._instance = new CardInstance(instanceId, cardId);
            return card;
        }
        public CardInstance instance
        {
            get { return _instance; }
        }
        [SerializeField]
        CardInstance _instance = null;
    }
    [Serializable]
    public class CardInstance
    {
        public int instanceId
        {
            get { return _instanceId; }
        }
        [SerializeField]
        int _instanceId;
        public int cardId
        {
            get { return _cardId; }
        }
        [SerializeField]
        int _cardId;
        public CardInstance(int instanceId, int cardId)
        {
            _instanceId = instanceId;
            _cardId = cardId;
        }
    }
}
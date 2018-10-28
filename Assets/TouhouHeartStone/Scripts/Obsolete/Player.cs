using UnityEngine;

namespace TouhouHeartstone
{
    public abstract class Player : MonoBehaviour
    {
        public abstract int id
        {
            get;
        }
        public Region hand
        {
            get
            {
                if (_hand == null)
                {
                    _hand = new GameObject("Hand").AddComponent<Region>();
                    _hand.transform.parent = transform;
                }
                return _hand;
            }
        }
        [SerializeField]
        Region _hand;
        public Region deck
        {
            get
            {
                if (_deck == null)
                {
                    _deck = new GameObject("Deck").AddComponent<Region>();
                    _deck.transform.parent = transform;
                }
                return _deck;
            }
        }
        [SerializeField]
        Region _deck;
    }
}
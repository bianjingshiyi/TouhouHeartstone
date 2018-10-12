using UnityEngine;

namespace TouhouHeartstone
{
    public abstract class Player : MonoBehaviour
    {
        public abstract int id
        {
            get;
        }
        public Deck deck
        {
            get
            {
                if (_deck == null)
                {
                    _deck = new GameObject("Deck").AddComponent<Deck>();
                    _deck.transform.parent = transform;
                }
                return _deck;
            }
        }
        [SerializeField]
        Deck _deck;
    }
}
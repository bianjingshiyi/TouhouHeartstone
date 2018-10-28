using UnityEngine;

namespace TouhouHeartstone
{
    public class THManager : MonoBehaviour
    {
        public Game game
        {
            get
            {
                if (_game == null)
                    _game = GetComponentInParent<Game>();
                return _game;
            }
        }
        [SerializeField]
        Game _game;
    }
}
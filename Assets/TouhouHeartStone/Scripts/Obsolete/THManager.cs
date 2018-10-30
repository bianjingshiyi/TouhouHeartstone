using UnityEngine;

namespace TouhouHeartstone
{
    public class THManager : MonoBehaviour
    {
        public GameContainer game
        {
            get
            {
                if (_game == null)
                    _game = GetComponentInParent<GameContainer>();
                return _game;
            }
        }
        [SerializeField]
        GameContainer _game;
    }
}
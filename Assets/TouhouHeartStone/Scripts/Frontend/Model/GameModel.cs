
using UnityEngine;

using TouhouHeartstone.Backend;

namespace TouhouHeartstone.Frontend.Model
{
    public class GameModel : MonoBehaviour
    {
        Game game = new Game();
        public Game Game => game;
    }
}

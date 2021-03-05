using System.Linq;
using System.Threading.Tasks;
using TouhouHeartstone;
using Game.Replays;
using NitoriNetwork.Common;

namespace Game
{
    public partial class GameManager
    {
        public Task startReplayGame(ReplayData replay)
        {
            THHGame game = createGame(replay.option, replay.players.Select(p =>
            {
                RoomPlayerInfo player = new RoomPlayerInfo();
                player.setDeck();
                return player;
            }));
            localPlayer = game.players.First();
            displayGameUI(game, localPlayer);
            mode = GameMode.replay;
            return runGame(game);
        }
    }
}

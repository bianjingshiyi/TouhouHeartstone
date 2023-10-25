using System.Linq;
using System.Threading.Tasks;
using TouhouHeartstone;
using Game.Replays;
using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using Game.BattleTable;

namespace Game
{
    public partial class GameManager
    {
        public async Task startReplayGame(ReplayData replay)
        {
            CardEngine game = await createGame(replay.option, replay.players.Select(p =>
            {
                RoomPlayerInfo player = new RoomPlayerInfo();
                player.RoomID = p.id;
                player.name = p.name;
                player.setDeck(p.getDeck());
                return player;
            }));
            _replayPlayer = new ReplayDataPlayer(game, replay);
            await startGame(game, GameMode.replay, game.players.First().id);
        }
        void onReplayGameUpdate()
        {
            BattleTableManager table = battleTableManager;
            if (!table.isPlayingAnimation() &&//没有其他动画在播放
                game.answers.getAllRequests().FirstOrDefault() is IRequest request &&//有询问
                !_replayPlayer.isReplayEnd//没放完
                )
            {
                _replayPlayer.playNextMove(request);
            }
        }
        //void onReplayGameRequest(IRequest request)
        //{
        //    TableManager table = getManager<TableManager>();
        //    table.onAllAnimsFinished += onReplayGameAllAnimFinished;
        //    //if (table.getAnimQueue().Length > 0)

        //    //else
        //    //    _replayPlayer.playNextMove(request);
        //}
        //void onReplayGameAllAnimFinished()
        //{
        //    TableManager table = getManager<TableManager>();
        //    _replayPlayer.playNextMove(game.answers.getAllRequests()[0]);
        //    table.onAllAnimsFinished -= onReplayGameAllAnimFinished;
        //}
        ReplayDataPlayer _replayPlayer = null;
        ReplaySystem _replaySystem = new ReplaySystem();
    }
}

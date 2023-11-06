using System.Linq;
using System.Threading.Tasks;
using Game.BattleTable;
using Game.Replays;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UnityEngine;

namespace Game
{
    public partial class GameManager
    {
        public async Task startReplayGame(ReplayData replay)
        {
            var players = new RoomPlayerData[]
            {
                replay.playerA.toRoomData(),
                replay.playerB.toRoomData(),
            };
            CardEngine game = await createGame(replay.option, players);
            _replayPlayer = new ReplayDataPlayer(game, replay);
            await startGame(game, GameMode.replay, game.players.First().id);
        }
        void onReplayGameUpdate()
        {
            BattleTableManager table = battleTableManager;
            // 如果还没放完
            if (!_replayPlayer.isReplayEnd)
            {
                // 有询问，并且UI正在等待该询问的回应
                var requests = game.answers.getAllRequests();
                foreach (var request in requests)
                {
                    if (request == null)
                        continue;
                    if (_replayCooldown <= 0)
                    {
                        IResponse response = _replayPlayer.getNextResponse();
                        if (request.isValidResponse(response))
                        {
                            game.answers.answer(response.playerId, response);
                            _replayPlayer.Next();
                        }
                    }
                }
                if (requests.Count() <= 0)
                {
                    _replayCooldown = 1;
                }
                else
                {
                    _replayCooldown -= Time.deltaTime;
                }
            }
        }
        private async void onSaveReplayButtonClickCallback()
        {
            await main.replays.saveReplay(_replaySystem.meta, _replaySystem.replay);
            var title = main.lang._(StringTable.HINT_TITLE);
            var content = main.lang._(GAME_REPLAY_SAVED);
            main.ui.dialog.ShowConfirm(title, content);
        }
        private void initReplay(CardEngine game, RoomData roomData, int localPlayerId)
        {
            _replaySystem.startRecord(game, roomData, localPlayerId);
        }
        private float _replayCooldown = 1;
        ReplayDataPlayer _replayPlayer = null;
        ReplaySystem _replaySystem = new ReplaySystem();
    }
}

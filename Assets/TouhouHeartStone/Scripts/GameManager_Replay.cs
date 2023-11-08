using System.Linq;
using System.Threading.Tasks;
using Game.Replays;
using TouhouCardEngine;

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
            // 如果还没放完
            if (!_replayPlayer.isReplayEnd)
            {
                _replayPlayer.update(game.answers);
            }
        }
        private async void onSaveReplayButtonClickCallback()
        {
            await main.replays.saveReplay(_replayRecorder.meta, _replayRecorder.replay);
            var title = main.lang._(StringTable.HINT_TITLE);
            var content = main.lang._(GAME_REPLAY_SAVED);
            main.ui.dialog.ShowConfirm(title, content);
        }
        private void initReplay(CardEngine game, RoomData roomData, int localPlayerId)
        {
            _recordingReplay = true;
            _replayRecorder.startRecord(game, roomData, localPlayerId);
        }
        private bool _recordingReplay;
        ReplayDataPlayer _replayPlayer = null;
        ReplayRecorder _replayRecorder = new ReplayRecorder();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game.Replays
{
    public class ReplayRecorder
    {
        #region 公有方法
        public void startRecord(CardEngine game, RoomData roomData, int localPlayerId)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            this.game = game;

            var players = roomData.playerDataList.ToArray();
            var localPlayer = players.FirstOrDefault(p => p.id == localPlayerId);
            var otherPlayer = players.FirstOrDefault(p => p.id != localPlayerId);
            meta = new ReplayMeta()
            {
                playerAMasterDefRef = localPlayer.getDeck().masterCardRef,
                playerBMasterDefRef = otherPlayer.getDeck().masterCardRef,
                requiredCardPools = roomData.getCardPools(),
                name = ReplayManager.getNowReplayFileName()
            };
            replay = new ReplayData()
            {
                version = REPLAY_VERSION,
                option = game.getOption(),
                players = players.Select(p => new ReplayPlayerData(p)).ToArray(),
                playerAId = localPlayer.id,
                playerBId = otherPlayer.id,
                responses = new List<IResponse>()
            };
            game.answers.onResponse += onPlayerResponseCallback;
        }
        public void stopRecord()
        {
            game.answers.onResponse -= onPlayerResponseCallback;
            game = null;
        }
        #endregion

        #region 私有方法

        #region 事件回调
        private void onPlayerResponseCallback(IResponse response)
        {
            replay.responses.Add(response);
        }
        #endregion

        #endregion

        #region 属性字段
        public const int REPLAY_VERSION = 0;
        public bool isRecording => game != null;
        CardEngine game { get; set; } = null;
        public ReplayMeta meta { get; private set; } = null;
        public ReplayData replay { get; private set; } = null;
        #endregion
    }
}

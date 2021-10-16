using BJSYGameCore;
using Game;
using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    class RoomCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public void display(RoomData roomData)
        {
            _currentRoomData = roomData;

            //_ui.ResetEvents();
            _ui.SetMode(RoomPanel.Mode.Local);
            _ui.SetSelfID(scene.game.network.logic.localPlayer.id);
            _ui.SetRightText("");
            _ui.SetInfo(roomData);

            _ui.gameObject.SetActive(true);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<RoomPanel>();
            }
            _ui.onStartClick += onStartButtonClick;
            _ui.onQuitClick += onQuitButtonClick;
            _ui.onRoomPropUpdate += onRoomPropUpdate;
            _ui.onPlayerRemove += onPlayerRemove;
            _ui.onAIPlayerAdd += onAddAIPlayer;
            _ui.onSetPlayerDeck += onSetPlayerDeck;
        }
        #endregion
        #region 事件回调
        private void onStartButtonClick()
        {
            game.game.startLocalGame(_currentRoomData);
        }
        private void onQuitButtonClick()
        {
            _currentRoomData = null;
            game.network.quitRoom();

            _ui.gameObject.SetActive(false);
            scene.menu.display();
        }
        private void onRoomPropUpdate(string propName, object value)
        {
            _currentRoomData.setProp(propName, value);
            _ui.SetInfo(_currentRoomData);
        }
        private void onPlayerRemove(int playerId)
        {
            for (int i = 0; i < _currentRoomData.playerDataList.Count; i++)
            {
                if (_currentRoomData.playerDataList[i].id == playerId)
                {
                    _currentRoomData.playerDataList.RemoveAt(i--);
                }
            }
            _ui.SetInfo(_currentRoomData);
        }
        private async void onAddAIPlayer()
        {
            await game.network.logic.AddAIPlayer();
            _ui.SetInfo(_currentRoomData);
        }
        private void onSetPlayerDeck(int playerId, int[] deck)
        {
            for (int i = 0; i < _currentRoomData.playerDataList.Count; i++)
            {
                var player = _currentRoomData.playerDataList[i];
                if (player.id == playerId)
                {
                    _currentRoomData.playerDataList[i].setDeck(deck);
                    if (player.type == RoomPlayerType.ai)
                    {
                        player.setReady(true);
                    }
                }
            }
            _ui.SetInfo(_currentRoomData);
        }
        #endregion
        #endregion
        [SerializeField]
        RoomPanel _ui;
        [SerializeField]
        RoomData _currentRoomData;
    }
}

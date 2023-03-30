using Game;
using System;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    class MainMenuCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public void display()
        {
            gameObject.SetActive(true);
        }
        public void hide()
        {
            gameObject.SetActive(false);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<MainMenu>();
            }

            _ui.onPVEButtonClick += onEntryPVEMode;
            _ui.onVsBotButtonClick += onClickVsBotButton;
            _ui.onBuildCardButtonClick += onBuildButtonClick;
            _ui.onEditorButtonClick += onEditorButtonClick;
        }
        #endregion
        #region 事件回调
        private async void onClickVsBotButton()
        {
            //Main main = _ui.getObject<Main>();
            await scene.game.network.logic.CreateLocalRoom();
            RoomData currentRoomData = scene.game.network.logic.CurrentRoom;
            currentRoomData.maxPlayerCount = 2; // 炉石两个人

            HearthstoneOption option = new HearthstoneOption();
            currentRoomData.setOption(option);

            hide();
            await scene.room.display(currentRoomData, true);
        }
        private void onBuildButtonClick()
        {
            _ui.gameObject.SetActive(false);
            scene.build.display();
        }

        /// <summary>
        /// 进入PVE模式界面
        /// </summary>
        private async void onEntryPVEMode()
        {
            // await scene.game.network.logic.CreateLocalRoom();
            // RoomData currentRoomData = scene.game.network.logic.CurrentRoom;
            // currentRoomData.maxPlayerCount = 2; // 炉石两个人
            //
            // HearthstoneOption option = new HearthstoneOption();
            // var rand = new byte[4];
            // System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(rand);
            // option.randomSeed = BitConverter.ToInt32(rand, 0);
            // currentRoomData.setOption(option);
            //
            // hide();
            // scene.room.display(currentRoomData, true);
            _ui.gameObject.SetActive(false);
            scene.pveRoom.display();
        }
        
        private void onEditorButtonClick()
        {
            _ui.gameObject.SetActive(false);
            scene.editor.display();
        }
        #endregion
        #endregion
        [SerializeField]
        MainMenu _ui;
    }
}

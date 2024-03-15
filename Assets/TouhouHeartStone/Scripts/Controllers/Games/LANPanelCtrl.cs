using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using UI;
using UnityEngine;

namespace Controllers
{
    internal class LANPanelCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public void display(bool online)
        {
            network.logic.SwitchMode(!online);
            display();
        }
        public void display()
        {
            var logic = network.logic;
            _ui.setLoadingImageVisible(false);
            _ui.setCreateRoomButtonInteractable(!logic.RoomIsValid);
            _ui.setIPFieldVisible(isLan);
            _ui.setRoomInfos(null, null, null);

            if (logic.RoomIsValid)
            {
                displayRoomPanel();
            }
            else
            {
                refreshRooms();
            }
            gameObject.SetActive(true);
        }
        public void hide()
        {
            gameObject.SetActive(false);
        }
        public void clearRooms()
        {
            _ui.clearRooms();
        }
        public void refreshRooms()
        {
            clearRooms();
            network.logic.ReloadRoomList();
        }
        #endregion
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _ui.onRefreshRoomList += onRefreshRoomListCallback;
            _ui.onCreateRoomClicked += onCreateRoomClickedCallback;
            _ui.onReturnButtonClicked += onReturnButtonClickedCallback;
            _ui.onConnectButtonClicked += onConnectButtonClickedCallback;
            _ui.onConfirmPassword += onConfirmPasswordCallback;
            _ui.onCancelPassword += onCancelPasswordCallback;
            network.onRoomListChanged += onRoomListChangedCallback;
        }
        #endregion
        #region 事件回调
        private async void onConfirmPasswordCallback(string password)
        {
            _ui.HidePasswordDialog();
            if (_enteringRoomID != null)
            {
                await joinRoom(_enteringRoomID, password);
            }
            else
            {
                await joinRoom(_enteringRoomAddress, _enteringRoomPort, password);
            }
            _enteringRoomAddress = null;
            _enteringRoomPort = 0;
            _enteringRoomID = null;
        }
        private void onCancelPasswordCallback()
        {
            _ui.HidePasswordDialog();
            _enteringRoomAddress = null;
            _enteringRoomPort = 0;
            _enteringRoomID = null;
        }
        private void onRoomListChangedCallback(LobbyRoomDataList list)
        {
            updateRoomList(list);
        }
        private void onRoomListItemClickedCallback(RoomListItem item)
        {
            var data = getRoomDataByListItem(item);
            if (data == null)
            {
                _ui.setRoomInfos(null, null, null);
            }
            else
            {
                var name = data.RoomID.ToString();
                var ip = isLan ? data.IP : "???";
                var desc = lang._(NETMANAGER_DESCRIBE);
                _ui.setRoomInfos(name, ip, desc);
            }
        }
        private async void onJoinRoomButtonClickedCallback(RoomListItem item)
        {
            var data = getRoomDataByListItem(item);
            if (data.IsLocked)
            {
                _ui.ShowPasswordDialog();
                _enteringRoomID = data.RoomID;
            }
            else
            {
                await joinRoom(data.RoomID);
            };
        }
        private void onRefreshRoomListCallback()
        {
            refreshRooms();
        }
        private async void onCreateRoomClickedCallback()
        {
            string name = lang._(NETMANAGER_HOME, network.logic.GetSelfInfo().name);
            await createRoom(name);
        }
        private void onReturnButtonClickedCallback()
        {
            game.ui.mainUI.display(game.ui.mainUI.MainMenu);
        }
        private void onConnectButtonClickedCallback(string ip)
        {
            try
            {
                var ports = network.getPorts();
                network.parseAddress(ip, ports[0], out string address, out int port);
                //if (RoomNeedsPassword(address, port))
                //{
                //}
                //else
                //{
                //    await joinRoom(address, port);
                //}
                _ui.ShowPasswordDialog();
                _enteringRoomAddress = address;
                _enteringRoomPort = port;
            }
            catch (UriFormatException)
            {
                dialog.ShowConfirm(lang._(StringTable.ERROR), lang._(INVALID_ADDRESS), null);
            }
        }
        #endregion
        private void updateRoomList(LobbyRoomDataList list)
        {
            clearRooms();
            _roomDataList.Clear();
            _roomListItemList.Clear();
            _roomDataList.AddRange(list.Values);
            foreach (var roomKV in list)
            {
                var dispItem = _ui.addRoomItem();
                var room = roomKV.Value;
                updateRoomListItem(dispItem, room);
                _roomListItemList.Add(dispItem);
            }
        }
        private void updateRoomListItem(RoomListItem item, LobbyRoomData obj)
        {
            item.onItemClicked += onRoomListItemClickedCallback;
            item.onJoinButtonClicked += onJoinRoomButtonClickedCallback;

            var viewData = new RoomItemViewData()
            {
                roomName = obj.RoomName,
                canJoin = obj.PlayerCount < obj.MaxPlayerCount,
                locked = obj.IsLocked
            };
            item.update(viewData);
        }
        private LobbyRoomData getRoomDataByListItem(RoomListItem item)
        {
            var index = _roomListItemList.IndexOf(item);
            return _roomDataList[index];
        }
        private async Task createRoom(string name = "", string password = "")
        {
            _ui.setLoadingImageVisible(true);
            try
            {
                await network.createRoom(name, password);
                displayRoomPanel();
            }
            catch (Exception e)
            {
                showCreateRoomException(e);
                Debug.LogException(e);
            }
            finally
            {
                _ui.setLoadingImageVisible(false);
            }
        }
        private async Task joinRoom(string roomID, string password = "")
        {
            _ui.setLoadingImageVisible(true);
            try
            {
                await network.joinRoom(roomID, password);
                displayRoomPanel();
            }
            catch (Exception e)
            {
                showJoinRoomException(e);
                Debug.LogException(e);
            }
            finally
            {
                _ui.setLoadingImageVisible(false);
            }
        }
        private async Task joinRoom(string addr, int port, string password = "")
        {
            _ui.setLoadingImageVisible(true);
            try
            {
                await network.joinRoom(addr, port, password);
                displayRoomPanel();
            }
            catch (Exception e)
            {
                showJoinRoomException(e);
                Debug.LogException(e);
            }
            finally
            {
                _ui.setLoadingImageVisible(false);
            }
        }
        /// <summary>
        /// 显示房间页面
        /// </summary>
        private void displayRoomPanel()
        {
            hide();
            RoomData roomData = network.logic.CurrentRoom;
            var roomMode = isLan ? RoomMode.Lan : RoomMode.Server;
            string text = isLan ? "IP：" + string.Join(", ", network.getAddresses()) : string.Empty;
            scene.room.display(roomData, text, roomMode);
            //  加载聊天框
            scene.chatCtrl.display(roomData);
        }
        private void showCreateRoomException(Exception e)
        {
            dialog.ShowConfirm(lang._(CANNOT_CREATE_ROOM), e.Message, null);
        }

        private void showJoinRoomException(Exception e)
        {
            dialog.ShowConfirm(lang._(CANNOT_JOIN_ROOM), e.Message, null);
        }
        #endregion


        [TranslateMsg]
        private const string NETMANAGER_DESCRIBE = "描述";
        [TranslateMsg("参数为玩家名称")]
        private const string NETMANAGER_HOME = "{0}的房间";
        [TranslateMsg]
        private const string CANNOT_CREATE_ROOM = "无法创建房间";
        [TranslateMsg]
        private const string CANNOT_JOIN_ROOM = "无法加入房间";
        [TranslateMsg]
        private const string INVALID_ADDRESS = "输入的地址格式有误";
        private NetworkingManager network => game.network;
        private UIDialog dialog => game.ui.dialog;
        private LangManager lang => game.lang;
        private List<LobbyRoomData> _roomDataList = new List<LobbyRoomData>();
        private List<RoomListItem> _roomListItemList = new List<RoomListItem>();
        private bool isLan => game.network.logic.isLan;
        #region 密码相关
        private string _enteringRoomAddress;
        private string _enteringRoomID;
        private int _enteringRoomPort;
        #endregion 密码相关
        [SerializeField]
        private LANPanel _ui;
    }
}

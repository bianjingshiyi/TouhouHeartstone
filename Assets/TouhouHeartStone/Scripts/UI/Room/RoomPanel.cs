
using BJSYGameCore;
using Game;
using System;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace UI
{
    partial class RoomPanel
    {
        RoomInfo roomInfo = new RoomInfo();

        partial void onAwake()
        {
            StartButtonButtonBlack.asButton.setSelectable(false);
            initEvents();
        }

        void initEvents()
        {
            RandomSeedContainerGameOptionItemText.InputField.onValueChanged.set(value =>
            {
                if (int.TryParse(value, out int i))
                {
                    roomInfo.getOption().randomSeed = i;
                    OnRoomInfoUpdate?.Invoke(roomInfo);
                }
                else
                {
                    RandomSeedContainerGameOptionItemText.InputField.text = roomInfo.getOption().randomSeed.ToString();
                }
            });

            ShuffleContainerGameOptionItemToggle.Toggle.onValueChanged.set(value =>
            {
                roomInfo.getOption().shuffle = value;
                OnRoomInfoUpdate?.Invoke(roomInfo);
            });

            InitReplacementContainerGameOptionItemText.InputField.onEndEdit.set(value =>
            {
                if (int.TryParse(value, out int i) && 5 < i)
                {
                    roomInfo.getOption().timeoutForInitReplace = i;
                    OnRoomInfoUpdate?.Invoke(roomInfo);
                }
                else
                {
                    InitReplacementContainerGameOptionItemText.InputField.text = roomInfo.getOption().timeoutForInitReplace.ToString();
                }
            });

            TurnTimeContainerGameOptionItemText.InputField.onEndEdit.set(value =>
            {
                if (int.TryParse(value, out int i) && 5 < i)
                {
                    roomInfo.getOption().timeoutForTurn = i;
                    OnRoomInfoUpdate?.Invoke(roomInfo);
                }
                else
                {
                    TurnTimeContainerGameOptionItemText.InputField.text = roomInfo.getOption().timeoutForTurn.ToString();
                }
            });

            IsSortedToggleGameOptionItemToggle.Toggle.onValueChanged.set((value) => {
                Debug.Log("更新房间设定" + value);
                roomInfo.setOption(new GameOption(roomInfo.getOption())
                {
                    sortedPlayers = value ? roomInfo.playerList.Select(p => p.id).ToArray() : null
                });
                OnRoomInfoUpdate?.Invoke(roomInfo);
            });

            StartButtonButtonBlack.asButton.onClick.set(() =>
            {
                OnStartClick?.Invoke();
            });

            QuitButtonButtonBlack.asButton.onClick.set(() =>
            {
                OnQuitClick?.Invoke();
            });
        }

        bool isHost = false;

        /// <summary>
        /// 设置是Host方还是Client方
        /// </summary>
        /// <param name="host"></param>
        public void SetSide(bool host)
        {
            RandomSeedContainerGameOptionItemText.InputField.setSelectable(host);
            ShuffleContainerGameOptionItemToggle.Toggle.setSelectable(host);
            InitReplacementContainerGameOptionItemText.InputField.setSelectable(host);
            TurnTimeContainerGameOptionItemText.InputField.setSelectable(host);
            IsSortedToggleGameOptionItemToggle.Toggle.setSelectable(host);

            isHost = host;
        }

        /// <summary>
        /// 设置房间信息
        /// </summary>
        /// <param name="room"></param>
        public void SetInfo(RoomInfo room)
        {
            roomInfo = room;

            RoomPlayerList.clearItems();
            foreach (var player in room.playerList)
            {
                var item = RoomPlayerList.addItem();
                item.NameText.text = player.name;
                item.CharacterText.text = this.findInstance<CardManager>().getSkin(player.getProp<int[]>(RoomPlayerInfoName.DECK_INTARRAY)[0]).name;
                item.ViewButtonBlack.asButton.onClick.AddListener(() =>
                {
                    //TODO:查看卡组
                });
            }
            RandomSeedContainerGameOptionItemText.InputField.text = room.getOption().randomSeed.ToString();
            ShuffleContainerGameOptionItemToggle.Toggle.isOn = room.getOption().shuffle;
            InitReplacementContainerGameOptionItemText.InputField.text = room.getOption().timeoutForInitReplace.ToString();
            TurnTimeContainerGameOptionItemText.InputField.text = room.getOption().timeoutForTurn.ToString();
            Debug.Log("刷新房间设定" + IsSortedToggleGameOptionItemToggle.Toggle.isOn);
            IsSortedToggleGameOptionItemToggle.Toggle.isOn = room.getOption().sortedPlayers != null;

            StartButtonButtonBlack.asButton.setSelectable(isHost && room.playerList.Count > 1);
        }

        public void SetRightText(string text)
        {
            IPText.text = text;
        }

        public event Action OnStartClick;
        public event Action OnQuitClick;
        public event Action<RoomInfo> OnRoomInfoUpdate;
    }
}

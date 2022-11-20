using BJSYGameCore.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using UI;
using UnityEngine;

namespace Controllers
{
    class ChatCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 公共方法
        protected new void Start()
        {
            game.network.logic.OnRecvChat += OnRecvChat;
        }

        public Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                isDisplay = true;
                if (ui == null)
                {
                    ui = transform.GetComponent<ChatRoom>();
                }
                ui.gameObject.SetActive(true);

                chatLog = info.ChatLog;
                players = info.Players;
                resetChatPanel();
            }
            return Task.CompletedTask;
        }

        private void OnRecvChat(ChatMsg obj)
        {
            onReceiveMessage(DateTime.Now, obj.Message, getPlayerName(obj.Sender));
        }

        public UIDisplayInfo getCurrentDisplayInfo()
        {
            return new DisplayInfo(chatLog, players);
        }

        public Task hide()
        {
            isDisplay = false;
            ui.gameObject.SetActive(false);

            chatLog.Clear();
            players.Clear();
            return Task.CompletedTask;
        }

        public Task display(RoomData data)
        {
            return display(new DisplayInfo(data));
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected void Awake()
        {
            if (ui == null)
            {
                ui = transform.GetComponent<ChatRoom>();
            }
            registerMethodCallBack();
        }
        #endregion

        #region 方法回调
        private void onSendButtonClicked(string message)
        {
            if (message.Length == 0)
            {
                return;
            }

            var pm = processMessage(message);
            if (!trySendMessage(pm))
            {
                Debug.LogError("发送失败");
                updateChatPanel();
            }
        }

        private void onReceiveMessage(DateTime time, string message, string player)
        {
            chatLog.Add(new ChatLine
            {
                time = time,
                speaker = player,
                content = message
            });
            updateChatPanel();
        }
        #endregion
        /// <summary>
        /// 重置聊天框
        /// </summary>
        private void resetChatPanel()
        {
            updateChatPanel();
        }

        /// <summary>
        /// 更新聊天框
        /// </summary>
        private void updateChatPanel()
        {
            var text = string.Empty;
            foreach (var cl in chatLog)
            {
                if (!string.IsNullOrEmpty(text))
                    text += "\n";
                text += chatLineToString(cl);
            }
            ui.SetContent(text);
            ui.ScrollToEnd();
            ui.EmptyInputField();
        }

        /// <summary>
        /// 注册回调
        /// </summary>
        private void registerMethodCallBack()
        {
            ui.onSendButtonClicked += onSendButtonClicked;
        }

        /// <summary>
        /// 处理消息的方法
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string processMessage(string message)
        {
            return message.Trim();
        }

        /// <summary>
        /// 尝试发送消息
        /// 总之得先有这么一个方法
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool trySendMessage(string message)
        {
            game.network?.logic?.SendChat(0, message);
            return true;
        }
        /// <summary>
        /// 获取本地玩家
        /// </summary>
        /// <returns></returns>
        private Player getLocalPlayerData()
        {
            return players.Find(p => p.Id == game.network.logic.localPlayer.id);
        }

        private string getPlayerName(int playerID)
        {
            return players.Find(p => p.Id == playerID)?.Name;
        }

        private string chatLineToString(ChatLine cl) 
        {
            return $"[{cl.time.ToShortTimeString()}] {cl.speaker}: {cl.content}";
        }
        #endregion
        #region 属性字段
        public bool IsDisplay { get => isDisplay; }
        bool isDisplay = false;
        /// <summary>
        /// 聊天记录
        /// </summary>
        List<ChatLine> chatLog;
        /// <summary>
        /// 玩家列表
        /// </summary>
        List<Player> players;
        ChatRoom ui;
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(RoomData data)
            {
                ChatLog = new List<ChatLine>();
                data.playerDataList.ForEach(p =>
                {
                    Players.Add(new Player(p));
                });
            }

            public DisplayInfo(List<ChatLine> chatLog, List<Player> players)
            {
                ChatLog = chatLog;
                Players = players;
            }
            /// <summary>
            /// 传入的聊天记录
            /// </summary>
            public List<ChatLine> ChatLog = new List<ChatLine>();
            /// <summary>
            /// 参与聊天的玩家列表
            /// </summary>
            public List<Player> Players = new List<Player>();
        }

        public struct ChatLine
        {
            /// <summary>
            /// 发送时间
            /// </summary>
            public DateTime time;
            /// <summary>
            /// 发送玩家ID
            /// </summary>
            public string speaker;
            /// <summary>
            /// 所讲内容
            /// </summary>
            public string content;
        }

        public class Player
        {
            public int Id;
            public string Name;

            public Player(int id,string name)
            {
                Id = id;
                Name = name;
            }

            public Player(RoomPlayerData rp)
            {
                Id = rp.id;
                Name = rp.name;
            }
        }

        #endregion
    }
}



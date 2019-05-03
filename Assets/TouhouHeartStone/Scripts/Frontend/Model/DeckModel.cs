
using UnityEngine;
using IGensoukyo.Utilities;
using TouhouHeartstone.Frontend.ViewModel;

namespace TouhouHeartstone.Frontend.Model
{
    public class DeckModel : ItemSpawner<DeckController>
    {
        GameModel gm;

        [SerializeField]
        PrivacyProtectorViewModel protector;

        [SerializeField]
        bool hotSeatMode = true;

        private void Awake()
        {
            gm = GetComponentInParent<GameModel>();
            prefab.gameObject.SetActive(false);

            protector.OnComfirmEvent += switchToNext;
        }

        /// <summary>
        /// 切换至下一个人
        /// </summary>
        /// <param name="obj"></param>
        private void switchToNext(int obj)
        {
            itemList[obj].gameObject.SetActive(true);
        }

        /// <summary>
        /// 显示切换按钮准备切换
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void switchPrepare(int from, int to)
        {
            DebugUtils.Log($"当前玩家{from}，下一个玩家{to}.");
            itemList[from].gameObject.SetActive(false);
            protector.PlayerID = to;
            protector.PlayerName = $"玩家{to}";
            protector.Visable = true;
        }

        private void Start()
        {
            // 添加两个玩家做热座测试
            AddPlayer();
            AddPlayer();

            gm.Game.init();
        }

        /// <summary>
        /// 添加玩家
        /// </summary>
        void AddPlayer()
        {
            var item = SpawnItem();
            gm.Game.addPlayer(item, new int[] { 1000, 1, 1, 1, 1, 1, 1, 1 });
            item.selfID = ItemCount - 1;
            item.gameObject.name = $"Deck{item.selfID}";

            if (hotSeatMode)
            {
                item.OnTurnEndEvent += Item_OnTurnEndEvent;
            }

            // 仅启用0号玩家
            item.gameObject.SetActive(item.selfID == 0);
        }

        /// <summary>
        /// 回合结束事件
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Item_OnTurnEndEvent(int arg1, int arg2)
        {
            switchPrepare(arg1, arg2);
        }

        /// <summary>
        /// 初始替换卡牌
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="cards"></param>
        public void InitReplace(int playerID, int[] cards)
        {
            // 热座模式下，先按添加播放，全部换牌完毕后再按游戏顺序播放
            if (hotSeatMode)
            {
                int to = playerID + 1;
                if (to == ItemCount)
                {
                    to = itemList[0].PlayerOrder[0];
                }
                switchPrepare(playerID, to);
            }
            gm.Game.initReplace(playerID, cards);
        }

        /// <summary>
        /// 结束回合
        /// </summary>
        /// <param name="playerID"></param>
        public void Roundend(int playerID)
        {
            gm.Game.turnEnd(playerID);
        }

        /// <summary>
        /// 用卡
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="cardRuntimeID"></param>
        /// <param name="args"></param>
        public void UseCard(int playerID, int cardRuntimeID, UseCardEventArgs args)
        {
            UberDebug.LogDebugChannel("Frontend", $"玩家{playerID}使用卡牌{cardRuntimeID}，{args}");
            if (args is UseCardWithPositionArgs)
            {
                var arg = args as UseCardWithPositionArgs;
                gm.Game.use(playerID, cardRuntimeID, arg.Position, 0);
            }
            else if (args is UseCardWithTargetArgs)
            {
                var arg = args as UseCardWithTargetArgs;
                gm.Game.use(playerID, cardRuntimeID, -1, arg.TargetCardRuntimeID);
            }
            else if (args is UseCardWithTargetPositionArgs)
            {
                var arg = args as UseCardWithTargetPositionArgs;
                gm.Game.use(playerID, cardRuntimeID, arg.Position, arg.TargetCardRuntimeID);
            }
            else
            {
                gm.Game.use(playerID, cardRuntimeID, -1, 0);
            }
        }
    }
}

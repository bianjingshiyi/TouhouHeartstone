
using UnityEngine;
using IGensoukyo.Utilities;
using TouhouHeartstone.Frontend.ViewModel;

using System.Linq;

namespace TouhouHeartstone.Frontend.Model
{
    public class DeckModel : ItemSpawner<DeckController>
    {
        GameModel gm;

        [SerializeField]
        PrivacyProtectorViewModel protector = null;

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
            item.OnDeckAction += onDeckAction;

            if (hotSeatMode)
            {
                item.OnTurnEndEvent += Item_OnTurnEndEvent;
            }

            // 仅启用0号玩家
            item.gameObject.SetActive(item.selfID == 0);
        }

        private void onDeckAction(object sender, System.EventArgs args)
        {
            if (args is AutoPlayerCardEventArgs)
            {
                AutoPlayerCardEventArgs gArgs = args as AutoPlayerCardEventArgs;
                if (gArgs.eventName == "attack")
                    gm.Game.attack(gArgs.getProp<int>("playerIndex"), gArgs.getProp<int>("cardRID"), gArgs.getProp<int>("targetCardRID"));
            }
            else if (args is UseCardEventArgs)
            {
                UseCard(args as UseCardEventArgs);
            }
            else if (args is RoundEventArgs)
            {
                gm.Game.turnEnd((args as RoundEventArgs).PlayerID);
            }
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


        public void ReplaceCards(int playerID, int[] cards)
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
        /// 用卡
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="cardRuntimeID"></param>
        /// <param name="args"></param>
        [System.Obsolete("使用DoAction来达到目的")]
        public void UseCard(int playerID, int cardRuntimeID, UseCardEventArgs args)
        {
            args.PlayerID = playerID;
            args.CardRID = cardRuntimeID;
            UseCard(args);
        }

        /// <summary>
        /// 用卡
        /// </summary>
        /// <param name="args"></param>
        void UseCard(UseCardEventArgs args)
        {
            UberDebug.LogDebugChannel("Frontend", $"玩家{args.PlayerID}使用卡牌{args.CardRID}，{args}");
            int position = -1, target = 0;
            if (args is UseCardWithPositionArgs)
            {
                position = (args as UseCardWithPositionArgs).Position;
            }
            else if (args is UseCardWithTargetArgs)
            {
                var arg = args as UseCardWithTargetArgs;
                target = arg.TargetCardRuntimeID;
            }
            else if (args is UseCardWithTargetPositionArgs)
            {
                var arg = args as UseCardWithTargetPositionArgs;
                position = arg.Position;
                target = arg.TargetCardRuntimeID;
            }
            gm.Game.use(args.PlayerID, args.CardRID, position, target);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

using TouhouHeartstone.Frontend.Controller;

using TouhouHeartstone.Frontend.Model.Witness;
using System;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.Model
{
    /// <summary>
    /// 桌面部件
    /// </summary>
    public class DeckController : MonoBehaviour, IFrontend
    {
        [SerializeField]
        CommonDeckController common;

        [SerializeField]
        UserDeckController userPrefab;

        [SerializeField]
        Transform spawnRoot;

        List<UserDeckController> users = new List<UserDeckController>();

        int _id = 0;

        public int id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _selfID;
        public int selfID
        {
            get { return _selfID; }
            set { _selfID = value; }
        }

        #region witness
        Queue<EventWitness> witnessQueue = new Queue<EventWitness>();
        bool queueEmpty = true;

        public void sendWitness(EventWitness witness)
        {
            witnessQueue.Enqueue(witness);

            // 当前活动且没有执行witness时，开始执行
            if (gameObject.activeSelf)
            {
                if (queueEmpty)
                    executeNext();
                else
                    DebugUtils.Debug($"{this.name}收到事件，当前Active但有事件block");
            }
            queueEmpty = false;
        }

        void OnEnable()
        {
            // 启用后检查是否有等待播放的witness
            if (!queueEmpty)
                executeNext();
        }

        void executeNext()
        {
            if (witnessQueue.Count == 0)
            {
                queueEmpty = true;
                return;
            }
            var witness = witnessQueue.Dequeue();
            EventWitnessExecutor.ExecuteWitness(witness.Flattern(), this, (a, b) => { executeNext(); });
        }
        #endregion

        int[] playerOrder = new int[0];
        /// <summary>
        /// 玩家顺序
        /// </summary>
        public int[] PlayerOrder => playerOrder;

        /// <summary>
        /// 设置本场游戏的玩家
        /// </summary>
        /// <param name="players"></param>
        /// <param name="characters"></param>
        public void SetPlayer(int[] players, CardID[] characters)
        {
            if (players.Length != characters.Length)
                throw new System.Exception("两个数据不匹配");

            playerOrder = players;

            for (int i = 0; i < players.Length; i++)
            {
                var u = Instantiate(userPrefab, spawnRoot);
                u.Init(i, characters[i], selfID == i);
                u.gameObject.SetActive(true);
                users.Add(u);
            }
        }

        /// <summary>
        /// 设置初始抽卡
        /// </summary>
        /// <param name="cards"></param>
        public void SetInitHandcard(int[] selfCardsDefine, int[][] cardsRuntime)
        {
            for (int i = 0; i < cardsRuntime.Length; i++)
            {
                users[i].InitDraw(CardID.ToCardIDs(i == selfID ? selfCardsDefine : new int[cardsRuntime[i].Length], cardsRuntime[i]));
            }
        }

        /// <summary>
        /// 设置自己的卡牌库
        /// </summary>
        /// <param name="cards"></param>
        public void SetSelfDeck(int[] cards)
        {
            users[selfID].SetDeck(cards);
        }

        /// <summary>
        /// 设置初始替换
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="cards"></param>
        /// <param name="callback"></param>
        public void SetInitReplace(int uid, CardID[] cards, GenericAction callback = null)
        {
            users[uid].DrawCard(cards, callback);
        }

        /// <summary>
        /// 丢掉初始手牌
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="cards"></param>
        public void InitReplace(int uid, int[] cards)
        {
            GetComponentInParent<DeckModel>().InitReplace(uid, cards);
        }

        private void Awake()
        {
            userPrefab.gameObject.SetActive(false);
            common.OnRoundendBtnClick += OnRoundendBtnClick;
        }

        /// <summary>
        /// 回合结束按钮按下
        /// </summary>
        /// <remarks>
        /// 回合结束的事件传递如下
        /// <see cref="ViewModel.RoundEndViewModel.RoundEndEvent"/> -> 
        /// <see cref="CommonDeckController.OnRoundendBtnClick"/> ->
        /// <see cref="OnRoundendBtnClick"/> ->
        /// <see cref="DeckModel.Roundend"/> ->
        /// <see cref="Backend.Game.turnEnd"/> ->
        /// <see cref="sendWitness"/> -> 进入事件处理程序
        /// <see cref="OnTurnEnd"/> ->
        /// <see cref="onTurnEnd"/> ->
        /// <see cref="CommonDeckController.RoundEnd"/> // 设置按钮禁用
        /// </remarks>
        private void OnRoundendBtnClick()
        {
            GetComponentInParent<DeckModel>().Roundend(selfID);
        }

        /// <summary>
        /// 回合开始
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="maxGem"></param>
        /// <param name="currentGem"></param>
        public void TurnStart(int playerID, int maxGem, int currentGem)
        {
            users[playerID].SetGem(maxGem, currentGem);
            common.RoundStart(playerID == selfID);
        }

        /// <summary>
        /// 当前回合结束事件
        /// param1: 当前回合PID, param2: 下一回合PID
        /// </summary>
        public event Action<int, int> OnTurnEndEvent;

        /// <summary>
        /// 回合结束后的处理
        /// </summary>
        /// <param name="playerID"></param>
        public void onTurnEnd(int playerID)
        {
            if (playerID == selfID)
            {
                common.RoundEnd();
                OnTurnEndEvent?.Invoke(playerID, GetNextPlayerID(playerID));
            }
        }

        private int GetNextPlayerID(int current)
        {
            for (int i = 0; i < playerOrder.Length; i++)
            {
                if (playerOrder[i] == current)
                {
                    return playerOrder[(i + 1) % playerOrder.Length];
                }
            }
            return -1;
        }

        /// <summary>
        /// 抽一张卡
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="card"></param>
        public void DrawCard(int playerID, CardID card, GenericAction callback)
        {
            users[playerID].DrawCard(card, callback);
        }
    }

    public class CardID
    {
        public static CardID[] ToCardIDs(int[] define, int[] runtime)
        {
            if (define.Length != runtime.Length)
                throw new LengthNotMatchException();

            CardID[] cards = new CardID[define.Length];
            for (int i = 0; i < define.Length; i++)
            {
                cards[i] = new CardID(define[i], runtime[i]);
            }
            return cards;
        }
        public CardID(int define, int runtime)
        {
            DefineID = define;
            RuntimeID = runtime;
        }
        public CardID() { }

        public int DefineID;
        public int RuntimeID;
    }


    [System.Serializable]
    public class LengthNotMatchException : System.Exception
    {
        public LengthNotMatchException() { }
        public LengthNotMatchException(int expected, int actual) : base($"Length is not match. Expect {expected} but actual {actual}") { }
    }
}

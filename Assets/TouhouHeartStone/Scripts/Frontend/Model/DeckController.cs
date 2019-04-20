using System.Collections.Generic;
using UnityEngine;

using TouhouHeartstone.Frontend.Controller;

using TouhouHeartstone.Frontend.Model.Witness;

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

        public void sendWitness(EventWitness witness)
        {
            witness.foreachDo(witnessHandler);
        }

        bool witnessHandler(EventWitness witness)
        {
            var result = WitnessLibrary.CreateHandler(witness.eventName).HandleWitness(witness, this);
            return result;
        }

        int[] playerOrder = new int[0];

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

        public void TurnEnd(int playerID)
        {
            if (playerID == selfID)
            {
                common.RoundEnd();
            }
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

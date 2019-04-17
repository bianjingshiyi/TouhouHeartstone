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
            return WitnessLibrary.CreateHandler(witness.eventName).HandleWitness(witness, this);
        }

        int[] playerOrder = new int[0];

        /// <summary>
        /// 设置本场游戏的玩家
        /// </summary>
        /// <param name="players"></param>
        /// <param name="characters"></param>
        public void SetPlayer(int[] players, int[] characters)
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
        public void SetInitHandcard(int[] cards)
        {
            users[selfID].InitDraw(cards);
        }

        /// <summary>
        /// 设置初始替换
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="cards"></param>
        /// <param name="callback"></param>
        public void SetInitReplace(int uid, int[] cards, GenericAction callback = null)
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
        }
    }
}

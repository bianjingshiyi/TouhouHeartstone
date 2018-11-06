using System;
using UnityEngine;
using UnityEngine.Events;

namespace TouhouHeartstone.Frontend.Manager
{
    public class FrontendWitnessEventDispatcher : FrontendSubManager
    {
        int selfID;

        private new void Awake()
        {
            base.Awake();

            var witness = Frontend.Game.witness;
            witness.onWitnessAdded.AddListener(onWitness);

            selfID = Frontend.Game.network.localPlayerId;
        }

        void onWitness(Witness witness)
        {
            if (witness is SetOrderWitness)
            {
                // 设置行动顺序。此witness含有一个包含了玩家行动顺序的array.
            }
            else if (witness is SetDeckWitness)
            {
                // 设置玩家的卡组大小
            }
            else if (witness is InitDrawWitness)
            {
                // 初始抽卡
                var iw = witness as InitDrawWitness;

                // todo: 考虑对方玩家的动画播放
                if (iw.playerId == selfID)
                {
                    getSiblingManager<FrontendCardManager>().InitDrawCard(iw.cards);
                }

            }
            else if (witness is InitReplaceWitness)
            {
                var rpw = witness as InitReplaceWitness;
                if (rpw.playerId == selfID)
                {
                    // todo: 这里给了原有的卡牌，是否要做个容错？
                    getSiblingManager<FrontendCardManager>().NormalDrawCard(rpw.replaceCards);
                }
            }
        }

        /// <summary>
        /// 替换初始手牌
        /// </summary>
        public UnityAction<int[]> ReplaceInitDrawAction;

        /// <summary>
        /// 回合结束事件
        /// </summary>
        public event Action EndRoundEventAction;
        internal void InvokeEndRoundEvent()
        {
            EndRoundEventAction?.Invoke();
        }


#if UNITY_EDITOR

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 100, 25), "抽张卡"))
            {
                getSiblingManager<FrontendCardManager>().NormalDrawCard(new CardInstance(UnityEngine.Random.Range(0, 2333), 0));
            }
        }

#endif
    }
}

using System;
using UnityEngine;

namespace TouhouHeartstone.Frontend.Manager
{
    public class FrontendWitnessEventDispatcher : FrontendSubManager
    {
        private new void Awake()
        {
            base.Awake();

            var witness = Frontend.Game.witness;
            witness.onWitnessAdded.AddListener(onWitness);
        }

        private void Start()
        {
            // test: 测试一下
            onWitness(new InitDrawWitness(0, new CardInstance[] { new CardInstance(0, 0), new CardInstance(1, 0), new CardInstance(2, 0) }));
        }

        void onWitness(Witness witness)
        {
            if (witness is SetOrderWitness)
            {
                // 设置行动顺序。此witness含有一个包含了玩家行动顺序的array.
                Debug.Log(witness.ToString());
            }
            else if (witness is SetDeckWitness)
            {
                // 设置玩家的卡组大小
            }
            else if (witness is InitDrawWitness)
            {
                // 初始抽卡
                var iw = witness as InitDrawWitness;
                getSiblingManager<FrontendCardManager>().InitDrawCard(iw.cards);
            }
        }
    }
}

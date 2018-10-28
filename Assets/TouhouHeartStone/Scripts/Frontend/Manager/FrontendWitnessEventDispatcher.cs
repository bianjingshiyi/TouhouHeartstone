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

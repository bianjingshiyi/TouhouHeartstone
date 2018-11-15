using System;
using UnityEngine;
using UnityEngine.Events;

namespace TouhouHeartstone.Frontend.Manager
{
    public class FrontendWitnessEventDispatcher : FrontendSubManager
    {
        int selfID => Frontend.PlayerID;

        public override void Init()
        {
            base.Init();

            var witness = Frontend.Game.witness;
            witness.onWitnessAdded.AddListener(onWitness);
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
            else if (witness is AddCrystalWitness)
            {
                // todo: 水晶的增加
                var acw = witness as AddCrystalWitness;
                if (acw.playerId == selfID)
                {
                    var stoneBar = getSiblingManager<FrontendUIManager>().StoneBar;
                    stoneBar.Set(acw.count, acw.count);
                }
            }
            else if (witness is DrawWitness)
            {
                // todo: 抽卡
                var dw = witness as DrawWitness;
                if (dw.playerId == selfID)
                {
                    getSiblingManager<FrontendCardManager>().NormalDrawCard(dw.cards);
                }
            }
            else if (witness is DuelStartWitness)
            {
                // todo: 对局开始
                DebugUtils.Log("对局开始。");
            }
            else if (witness is RemoveCrystalWitness)
            {
                var rcw = witness as RemoveCrystalWitness;
                // todo: 移出水晶
                DebugUtils.Log("移除水晶：" + rcw.count);
            }
            else if (witness is SetHandWitness)
            {
                var shw = witness as SetHandWitness;
                // todo: 直接设置手牌
                // 玛德还有这种操作？
                DebugUtils.Log("设置手牌");
            }
            else if (witness is TurnStartWitness)
            {
                var tsw = witness as TurnStartWitness;
                if(tsw.playerId == selfID)
                {
                    getSiblingManager<FrontendUIManager>().RoundStart();
                    DebugUtils.Log("你的回合");
                }
            }
            else
            {
                DebugUtils.Log("没有找到对应的Witness，类型：" + witness.GetType());
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

        /// <summary>
        /// 卡牌使用事件
        /// [instanceID, position, target]
        /// 详情看Remark
        /// </summary>
        /// <remarks>
        /// <![CDATA[
        /// instanceID: 对应卡牌的ID
        /// position: 表示了卡牌对应实体的生成位置，仅用于实体卡。非实体卡设为-1
        /// target: 表示了卡牌对应目标的ID，敌方本体设为0。仅用于目标卡，非目标卡设为-1
        /// ]]>
        /// </remarks>
        public event Action<int, int, int> UseCardEventAction;

        internal void InvokeUseCardEvent(int cardID, int position = -1, int target = -1)
        {
            DebugUtils.Log($"使用卡片。id: {cardID}, pos: {position}, target: {target}");
            UseCardEventAction?.Invoke(cardID, position, target);
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

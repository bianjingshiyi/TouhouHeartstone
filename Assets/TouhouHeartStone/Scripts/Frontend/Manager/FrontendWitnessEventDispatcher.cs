using System;
using System.Collections.Generic;
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

        Queue<Witness> witnessQueue = new Queue<Witness>();

        private void OnEnable()
        {
            while(witnessQueue.Count > 0)
            {
                witnessExecutor(witnessQueue.Dequeue());
            }
        }

        void onWitness(Witness witness)
        {
            if (!gameObject.activeInHierarchy || witnessQueue.Count > 0)
            {
                witnessQueue.Enqueue(witness);
                DebugUtils.LogDebug($"[{selfID}]Buff a witness.");
            }
            else
            {
                witnessExecutor(witness);
            }
        }

        private void witnessExecutor(Witness witness)
        {
            DebugUtils.LogDebug($"[{selfID}]{witness.ToString()}");

            if (witness is SetOrderWitness)
            {
                witnessExec(witness as SetOrderWitness);
            }
            else if (witness is SetDeckWitness)
            {
                witnessExec(witness as SetDeckWitness);
            }
            else if (witness is InitDrawWitness)
            {
                witnessExec(witness as InitDrawWitness);
            }
            else if (witness is InitReplaceWitness)
            {
                witnessExec(witness as InitReplaceWitness);
            }
            else if (witness is AddCrystalWitness)
            {
                witnessExec(witness as AddCrystalWitness);
            }
            else if (witness is DrawWitness)
            {
                witnessExec(witness as DrawWitness);
            }
            else if (witness is DuelStartWitness)
            {
                witnessExec(witness as DuelStartWitness);
            }
            else if (witness is RemoveCrystalWitness)
            {
                witnessExec(witness as RemoveCrystalWitness);
            }
            else if (witness is SetHandWitness)
            {
                witnessExec(witness as SetHandWitness);
            }
            else if (witness is TurnStartWitness)
            {
                witnessExec(witness as TurnStartWitness);
            }
            else
            {
                DebugUtils.Log("没有找到对应的Witness，类型：" + witness.GetType());
            }
        }

        /// <summary>
        /// 设置行动顺序
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(SetOrderWitness witness)
        {
            Frontend.PlayerOrder = witness.orderedPlayerId;
            return true;
        }

        /// <summary>
        /// 设置玩家卡组大小
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(SetDeckWitness witness)
        {
            return true;
        }

        /// <summary>
        /// 初始抽卡
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(InitDrawWitness witness)
        {
            // todo: 考虑对方玩家的动画播放
            if (witness.playerId == selfID)
            {
                getSiblingManager<FrontendCardManager>().InitDrawCard(witness.cards);
            }
            return false;
        }

        /// <summary>
        /// 替换初始卡牌
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(InitReplaceWitness witness)
        {
            if (witness.playerId == selfID)
            {
                // todo: 这里给了原有的卡牌，是否要做个容错？
                getSiblingManager<FrontendCardManager>().NormalDrawCard(witness.replaceCards);
            }
            return false;
        }

        /// <summary>
        /// 添加水晶
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(AddCrystalWitness witness)
        {
            if (witness.playerId == selfID)
            {
                var stoneBar = getSiblingManager<FrontendUIManager>().StoneBar;

                // 这里就直接回满了
                stoneBar.MaxStone += witness.count;
                stoneBar.CurrentStone = stoneBar.MaxStone;
            }
            return true;
        }

        /// <summary>
        /// 抽卡
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(DrawWitness witness)
        {
            if (witness.playerId == selfID)
            {
                getSiblingManager<FrontendCardManager>().NormalDrawCard(witness.cards);
            }
            return false;
        }

        /// <summary>
        /// 对局开始
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(DuelStartWitness witness)
        {
            DebugUtils.Log("对局开始。");
            return true;
        }

        /// <summary>
        /// 移除水晶
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(RemoveCrystalWitness witness)
        {
            // todo: 移出水晶
            DebugUtils.Log("移除水晶：" + witness.count);
            return true;
        }

        /// <summary>
        /// 直接设置手牌
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(SetHandWitness witness)
        {
            // todo: 设置手牌
            // 玛德还有这种操作？
            DebugUtils.Log("设置手牌");
            return true;
        }

        /// <summary>
        /// 回合开始
        /// </summary>
        /// <param name="witness"></param>
        /// <returns></returns>
        private bool witnessExec(TurnStartWitness witness)
        {
            if (witness.playerId == selfID)
            {
                getSiblingManager<FrontendUIManager>().RoundStart();
                DebugUtils.Log("你的回合");
            }
            return true;
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
            DebugUtils.LogDebug($"使用卡片。id: {cardID}, pos: {position}, target: {target}");
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

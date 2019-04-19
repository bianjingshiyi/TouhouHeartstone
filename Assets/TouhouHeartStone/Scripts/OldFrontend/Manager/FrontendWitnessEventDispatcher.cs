using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.OldFrontend.Manager
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

        Queue<IWitness> witnessQueue = new Queue<IWitness>();

        private void OnEnable()
        {
            if(playOnEnable)
            {
                playOnEnable = false;
                DequeueWitness();
            }
        }

        private void DequeueWitness()
        {
            while (witnessQueue.Count > 0)
            {
                var wit = witnessQueue.Dequeue();
                bool hasAnime = new WitnessHandler.WitnessHandler().Exec(wit, this.Frontend);

                // 若上个动画没有播放完毕，则不要在Enable的时候播放下一个动画
                playOnEnable = !hasAnime;
                // debug: 在没有做完动画播放前不要启用这个玩意
                if (hasAnime)
                {
                    DebugUtils.LogDebug("等候动画播放");
                    break;
                }
            }
        }

        /// <summary>
        /// 在启用时播放动画
        /// </summary>
        bool playOnEnable;

        void onWitness(IWitness witness)
        {
            var original = witnessQueue.Count;

            witnessQueue.Enqueue(witness);
            DebugUtils.LogDebug($"[{selfID}]Buff a witness.");

            // 如果是启用状态且这个是第一个，那就立即播放。
            if (gameObject.activeInHierarchy)
            {
                if(original == 0)
                    DequeueWitness();
            }
            else
            {
                playOnEnable = true;
            }
        }
        /// <summary>
        /// witness动画播放完毕后调用这个玩意
        /// </summary>
        public void OnWitnessFinish()
        {
            DebugUtils.LogDebug("动画播放完毕");
            if (gameObject.activeInHierarchy)
            {
                DequeueWitness();
            }
            else
            {
                playOnEnable = true;
            }
        }

        /// <summary>
        /// 替换初始手牌
        /// </summary>
        public event UnityAction<int[]> ReplaceInitDrawAction;

        internal void InvokeReplaceInitDrawEvent(int[] arg)
        {
            ReplaceInitDrawAction?.Invoke(arg);
        }

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

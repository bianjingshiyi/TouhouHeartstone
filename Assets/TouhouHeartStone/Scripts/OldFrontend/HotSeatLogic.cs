using System.Collections.Generic;
using TouhouHeartstone.OldFrontend.Manager;
using UnityEngine;
using System;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.OldFrontend
{
    public class HotSeatLogic : MonoBehaviour
    {
        [SerializeField]
        FrontendInstanceManager instanceManager;

        [SerializeField]
        UIPrivacyProtector privacyProtector;

        private void Awake()
        {
            instanceManager = instanceManager ?? FindObjectOfType<FrontendInstanceManager>();
            privacyProtector.OnHide += OnProtectorHide;
        }

        [SerializeField]
        bool hotSeat;

        List<FrontendInstanceEventHandler> frontendInstanceEventHandlers = new List<FrontendInstanceEventHandler>();

        private void Start()
        {
            if (hotSeat)
            {
                // 添加另一方玩家（默认是disable的）
                instanceManager.AddAnotherInstance();

                foreach (var item in instanceManager.Instances)
                {
                    var evtDispatcher = item.Manager.GetSubManager<FrontendWitnessEventDispatcher>();
                    var handler = new FrontendInstanceEventHandler(this, item.ID);

                    evtDispatcher.EndRoundEventAction += handler.OnRoundEnd;
                    evtDispatcher.ReplaceInitDrawAction += handler.OnCardReplace;
                    // 防止被GC?
                    frontendInstanceEventHandlers.Add(handler);
                }
            }
        }

        int nextID;
        int[] SceneOrder;

        public void OnRoundEnd(int id)
        {
            DebugUtils.Debug($"Instance {id} round end.");
            instanceManager.SetInstanceState(id, false);

            // 使用下一个行动顺序的场景
            nextID = SceneOrder[(id + 1) % SceneOrder.Length];

            privacyProtector.SetCurrentUserID(nextID);
            privacyProtector.gameObject.SetActive(true);
        }

        public void OnReplaceHandCard(int id)
        {
            DebugUtils.Debug($"Instance {id} replace card.");
            instanceManager.SetInstanceState(id, false);

            var instanceCount = instanceManager.Instances.Length;

            // 全部方选择完毕
            if (id == instanceCount - 1)
            {
                // 查找真正的顺序
                var playerOrder = instanceManager.Instances[0].Manager.PlayerOrder;
                var sceneOrder = new int[playerOrder.Length];

                // 映射 playerID => instanceID
                for (int j = 0; j < playerOrder.Length; j++)
                {
                    for (int i = 0; i < sceneOrder.Length; i++)
                    {
                        if (instanceManager.Instances[i].Manager.PlayerID == playerOrder[j])
                        {
                            sceneOrder[j] = i;
                            break;
                        }
                    }
                }

                SceneOrder = sceneOrder;
                nextID = SceneOrder[0];
            }
            else
            {
                nextID = id + 1;
            }

            privacyProtector.SetCurrentUserID(nextID);
            privacyProtector.gameObject.SetActive(true);
        }

        public void OnProtectorHide()
        {
            instanceManager.SetInstanceState(nextID, true);
        }

        class FrontendInstanceEventHandler
        {
            HotSeatLogic HotSeat;
            int InstanceID;
            public FrontendInstanceEventHandler(HotSeatLogic hotSeat, int instanceID)
            {
                HotSeat = hotSeat;
                InstanceID = instanceID;
            }

            public void OnRoundEnd()
            {
                HotSeat.OnRoundEnd(InstanceID);
            }

            public void OnCardReplace(int[] cards)
            {
                HotSeat.OnReplaceHandCard(InstanceID);
            }
        }
    }
}

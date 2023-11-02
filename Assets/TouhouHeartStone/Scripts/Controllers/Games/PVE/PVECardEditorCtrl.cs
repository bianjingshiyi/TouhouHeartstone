using System.Collections.Generic;
using TouhouCardEngine;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Controllers
{
    class PVECardEditorCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 私有方法
        public void display(CardDefine define, CardSkinData skin)
        {
            gameObject.SetActive(true);
            _ui.setCard(define, skin);
            var list = new List<CardGemViewData>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(new CardGemViewData()
                {
                    sprite = null
                });
            }
            _ui.updateGemInventory(list.ToArray());
            _ui.updateGemSlot(new CardGemSlotGroupViewData[]
            {
                new CardGemSlotGroupViewData
                {
                    slotsViewData = new CardGemSlotViewData[]
                    {
                        new CardGemSlotViewData
                        {
                            frameSprite = null,
                            gemViewData = null
                        }
                    }
                }
            });
        }

        #region 生命周期
        private void Awake()
        {
            _ui.onCloseClicked += onCloseClickedCallback;
            _ui.onInventoryGemBeginDrag += onInventoryGemBeginDragCallback;
            _ui.onInventoryGemDrag += onInventoryGemDragCallback;
            _ui.onInventoryGemEndDrag += onInventoryGemEndDragCallback;
            _ui.onSlotGemBeginDrag += onSlotGemBeginDragCallback;
            _ui.onSlotGemDrag += onSlotGemDragCallback;
            _ui.onSlotGemEndDrag += onSlotGemEndDragCallback;
        }
        #endregion

        #region 事件回调
        private void onCloseClickedCallback()
        {
        }
        private void onInventoryGemBeginDragCallback(int index, PointerEventData pointer)
        {
            draggingGemIndex = index;
            _ui.startDragInventoryGem(index);
        }
        private void onInventoryGemDragCallback(int index, PointerEventData pointer)
        {
            _ui.moveInventoryGem(index, pointer.position);
        }
        private void onInventoryGemEndDragCallback(int index, PointerEventData pointer)
        {
            _ui.releaseDragInventoryGem(index);
            draggingGemIndex = -1;
        }
        private void onSlotGemBeginDragCallback(int groupIndex, int index, PointerEventData pointer)
        {
        }
        private void onSlotGemDragCallback(int groupIndex, int index, PointerEventData pointer)
        {
        }
        private void onSlotGemEndDragCallback(int groupIndex, int index, PointerEventData pointer)
        {
        }
        #endregion

        #endregion

        #region 属性字段
        private int draggingGemIndex;
        [SerializeField]
        private PVECardEditor _ui;
        [SerializeField]
        private ActionNodeGraphCtrl _graphCtrl;
        #endregion
    }
}

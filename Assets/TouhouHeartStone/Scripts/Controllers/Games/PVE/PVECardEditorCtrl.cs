using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
using Game.PVE;
using TouhouCardEngine;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using CardGem = Game.PVE.CardGem;
using CardGemSlot = Game.PVE.CardGemSlot;
using CardGemSlotGroup = Game.PVE.CardGemSlotGroup;

namespace Controllers
{
    class PVECardEditorCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 私有方法
        public async Task display(string dataName, SaveData data, CardDefine define, CardManager cardManager)
        {
            gameObject.SetActive(true);
            await _ui.setCard(define, cardManager);
            var effect = define.getGeneratedEffects().FirstOrDefault();
            if (effect == null)
                _effectEditor.hideActionGraph();
            else
                await _effectEditor.display(define.cardPoolId, effect);


            _dataName = dataName;
            var inventory = data.plugins;
            _gemInventory.Clear();
            _gemInventory.AddRange(inventory.Select(d => new CardGem(d)));

            _slotGroups = PVEManager.getCardDefineSlots(define) ?? new CardGemSlotGroup[0];
            await updateAllGems();
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
            _pveRoom.openGamePage();
        }
        private async void onInventoryGemBeginDragCallback(int index, PointerEventData pointer)
        {
            var gem = _gemInventory[index];
            var viewData = await getGemViewData(gem);
            var gemUI = _ui.getInventoryGem(index);
            _draggingGem = _ui.createDraggingGem(viewData);
            _draggingGem.startDrag(gemUI);
        }
        private void onInventoryGemDragCallback(int index, PointerEventData pointer)
        {
            if (!_draggingGem)
                return;
            _draggingGem.move(pointer.position);
        }
        private async void onInventoryGemEndDragCallback(int index, PointerEventData pointer)
        {
            if (!_draggingGem)
                return;
            var (groupIndex, slotIndex) = _ui.getHoveredGemSlotIndex(pointer);
            if (groupIndex >= 0 && slotIndex >= 0)
            {
                var slotGem = getSlotGem(groupIndex, slotIndex);
                if (slotGem != null)
                {
                    // 与背包中的进行交换。
                    var invIndex = removeGemFromSlot(groupIndex, slotIndex);
                    insertGemToSlot(index, groupIndex, slotIndex);
                    invIndex--;
                    await updateAllGems();

                    // 创造一个交换的动画。
                    var viewData = await getGemViewData(slotGem);
                    var newDraggingGem = _ui.createDraggingGem(viewData);
                    var originPos = _ui.getSlot(groupIndex, slotIndex).transform.position;
                    var invSource = _ui.getInventoryGem(invIndex);
                    var slotUI = _ui.getSlotGem(groupIndex, slotIndex);

                    _draggingGem.changeSource(slotUI);
                    newDraggingGem.move(originPos);
                    newDraggingGem.changeSource(invSource);
                }
                else
                {
                    // 镶嵌宝石。
                    insertGemToSlot(index, groupIndex, slotIndex);
                    await updateAllGems();

                    var slotUI = _ui.getSlotGem(groupIndex, slotIndex);
                    _draggingGem.changeSource(slotUI);
                }
            }
            _draggingGem.release();
            _draggingGem = null;
        }
        private async void onSlotGemBeginDragCallback(int groupIndex, int index, PointerEventData pointer)
        {
            var gem = getSlotGem(groupIndex, index);
            if (gem != null)
            {
                _draggingGemSlotGroupIndex = groupIndex;
                _draggingGemSlotIndex = index;
                var viewData = await getGemViewData(gem);
                var gemUI = _ui.getSlotGem(groupIndex, index);
                _draggingGem = _ui.createDraggingGem(viewData);
                _draggingGem.startDrag(gemUI);
            }
        }
        private void onSlotGemDragCallback(int groupIndex, int index, PointerEventData pointer)
        {
            if (!_draggingGem)
                return;
            if (_draggingGemSlotGroupIndex < 0 || _draggingGemSlotIndex < 0)
                return;
            _draggingGem.move(pointer.position);
        }
        private async void onSlotGemEndDragCallback(int groupIndex, int slotIndex, PointerEventData pointer)
        {
            if (!_draggingGem)
                return;
            if (_draggingGemSlotGroupIndex < 0 || _draggingGemSlotIndex < 0)
                return;
            var (hoverGroup, hoverSlot) = _ui.getHoveredGemSlotIndex(pointer);
            if (hoverGroup < 0 || hoverSlot < 0)
            {
                // 移除宝石
                var invIndex = removeGemFromSlot(groupIndex, slotIndex);
                if (invIndex >= 0)
                {
                    await updateAllGems();
                    var source = _ui.getInventoryGem(invIndex);
                    _draggingGem.changeSource(source);
                }
            }
            else if (groupIndex != hoverGroup || hoverSlot != slotIndex)
            {
                var hoverGem = getSlotGem(hoverGroup, hoverSlot);
                moveGemBetweenSlots(groupIndex, slotIndex, hoverGroup, hoverSlot);
                await updateAllGems();
                if (hoverGem != null)
                {
                    // 创造一个交换的动画。
                    var viewData = await getGemViewData(hoverGem);
                    var newDraggingGem = _ui.createDraggingGem(viewData);
                    var originPos = _ui.getSlot(hoverGroup, hoverSlot).transform.position;
                    var curSource = _ui.getSlotGem(groupIndex, slotIndex);

                    newDraggingGem.move(originPos);
                    newDraggingGem.changeSource(curSource);
                }
                // 移动宝石
                var newSource = _ui.getSlotGem(hoverGroup, hoverSlot);
                _draggingGem.changeSource(newSource);
            }
            _draggingGem.release();
            _draggingGem = null;
            _draggingGemSlotGroupIndex = -1;
            _draggingGemSlotIndex = -1;
        }
        #endregion

        private void insertGemToSlot(int index, int groupIndex, int slotIndex)
        {
            var gem = _gemInventory[index];
            _gemInventory.RemoveAt(index);
            var group = _slotGroups[groupIndex];
            var slot = group.getSlotAt(slotIndex);
            slot.setGem(gem);
        }
        private int removeGemFromSlot(int groupIndex, int slotIndex)
        {
            var group = _slotGroups[groupIndex];
            var slot = group.getSlotAt(slotIndex);
            var gem = slot.gem;
            if (gem == null)
                return -1;
            slot.setGem(null);
            var index = _gemInventory.Count;
            _gemInventory.Add(gem);
            return index;
        }
        private void moveGemBetweenSlots(int groupIndex, int slotIndex, int targetGroupIndex, int targetSlotIndex)
        {
            var group = _slotGroups[groupIndex];
            var slot = group.getSlotAt(slotIndex);
            var gem = slot.gem;
            var targetGroup = _slotGroups[targetGroupIndex];
            var targetSlot = targetGroup.getSlotAt(targetSlotIndex);
            var targetGem = targetSlot.gem;
            targetSlot.setGem(gem);
            slot.setGem(targetGem);
        }
        private CardGem getSlotGem(int groupIndex, int slotIndex)
        {
            var group = _slotGroups[groupIndex];
            var slot = group.getSlotAt(slotIndex);
            return slot.gem;
        }
        private async Task<CardGemViewData> getGemViewData(CardGem gem)
        {
            if (gem == null)
                return null;
            var cardDefine = game.card.getCardDefine(gem.cardRef);
            var image = await game.card.loadCardImage(cardDefine);
            return new CardGemViewData()
            {
                sprite = image
            };
        }
        private Sprite getFrameSprite(CardGemSlot slot)
        {
            return null;
        }
        private async Task updateGemInventory()
        {
            var inventoryView = new CardGemViewData[_gemInventory.Count];
            for (int g = 0; g < inventoryView.Length; g++)
            {
                var gem = _gemInventory[g];
                var gemViewData = await getGemViewData(gem);
                inventoryView[g] = gemViewData;
            }
            _ui.updateGemInventory(inventoryView);
        }
        private async Task updateGemSlots()
        {
            var groupViews = new CardGemSlotGroupViewData[_slotGroups.Length];
            for (int g = 0; g < groupViews.Length; g++)
            {
                var group = _slotGroups[g];
                var slotViews = new CardGemSlotViewData[group.slots.Length];
                for (int s = 0; s < slotViews.Length; s++)
                {
                    var slot = group.slots[s];
                    var frameSprite = getFrameSprite(slot);
                    var gemViewData = await getGemViewData(slot.gem);
                    var slotView = new CardGemSlotViewData
                    {
                        frameSprite = frameSprite,
                        gemViewData = gemViewData
                    };
                    slotViews[s] = slotView;
                }
                var groupView = new CardGemSlotGroupViewData
                {
                    slotsViewData = slotViews
                };
                groupViews[g] = groupView;
            }
            _ui.updateGemSlot(groupViews);
        }
        private async Task updateAllGems()
        {
            await updateGemInventory();
            await updateGemSlots();
        }
        #endregion

        #region 属性字段
        private int _draggingGemSlotGroupIndex = -1;
        private int _draggingGemSlotIndex = -1;
        private DraggingCardGem _draggingGem;
        private List<CardGem> _gemInventory = new List<CardGem>();
        private CardGemSlotGroup[] _slotGroups;
        private string _dataName;
        [SerializeField]
        private PVECardEditor _ui;
        [SerializeField]
        private PVERoomCtrl _pveRoom;
        [SerializeField]
        private EffectEditorCtrl _effectEditor;
        [SerializeField]
        private ActionNodeGraphCtrl _graphCtrl;
        #endregion
    }
}

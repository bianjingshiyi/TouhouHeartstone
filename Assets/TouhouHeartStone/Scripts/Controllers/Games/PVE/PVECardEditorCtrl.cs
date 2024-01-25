using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
using Game.PVE;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
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
        #region 公有方法
        public async Task display(string dataName, SaveData data)
        {
            _srcCardDefine = null;
            _cardDefine = null;
            _slotGroups = null;
            _dataName = dataName;
            _saveData = data;

            var env = new List<ICardPoolReference>()
            {
                game.card.getLocalCardPoolReference(CardManager.STANDARD_POOL_ID),
                game.card.getLocalCardPoolReference(CardManager.PVE_POOL_ID),
            };
            var (generatedPoolInfo, _) = await game.pve.getGeneratedCardPool(_dataName);
            env.Add(generatedPoolInfo.ToReference());
            _cardPoolEnv = env.ToArray();

            _ui.setCardSelected(false);
            _ui.displayDeckPage();
            await updateDeck();
            updateInventory();
            await updateAllGems();
            _ui.deckCardList.hidePreviewCard();
            gameObject.SetActive(true);
        }
        public void markUnsaved()
        {
            _hasUnsavedChanges = true;
        }
        #endregion

        #region 私有方法

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

            _ui.onDeckItemClicked += onDeckItemClickedCallback;
            _ui.onDeckItemPointerEnter += onDeckItemPointerEnterCallback;
            _ui.onDeckItemPointerExit += onDeckItemPointerExitCallback;
        }
        #endregion

        #region 事件回调
        private async void onCloseClickedCallback()
        {
            if (_hasUnsavedChanges)
            {
                _hasUnsavedChanges = false;

                // 替换套牌中的这张卡。
                bool removed = _saveData.deck.removeCard(_srcCardDefine.getDefineRef());
                if (removed)
                    _saveData.deck.addCard(_cardDefine.getDefineRef());

                await game.pve.saveData(_dataName, _saveData);
                await game.pve.saveGeneratedCardPool(_dataName);
            }
            _pveRoom.openGamePage();
        }

        #region 宝石拖动
        private async void onInventoryGemBeginDragCallback(int index, PointerEventData pointer)
        {
            var inventory = getInventoryGems();
            var gem = inventory[index];
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
                var inventory = getInventoryGems();
                var invGem = inventory[index];
                var slotGem = getSlotGem(groupIndex, slotIndex);
                if (slotGem != null)
                {
                    // 与背包中的进行交换。
                    removeGemFromSlot(groupIndex, slotIndex);
                    insertGemToSlot(invGem.id, groupIndex, slotIndex);
                    await updateGemEffect(getSlotGroup(groupIndex));
                    await updateAllGems();
                    var invIndex = Array.IndexOf(getInventoryGems(), slotGem);

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
                    insertGemToSlot(invGem.id, groupIndex, slotIndex);
                    await updateGemEffect(getSlotGroup(groupIndex));
                    await updateAllGems();

                    var slotUI = _ui.getSlotGem(groupIndex, slotIndex);
                    _draggingGem.changeSource(slotUI);
                }
                markUnsaved();
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
                var slotGem = getSlotGem(groupIndex, slotIndex);
                if (slotGem != null)
                {
                    removeGemFromSlot(groupIndex, slotIndex);
                    await updateGemEffect(getSlotGroup(groupIndex));
                    await updateAllGems();
                    var invIndex = Array.IndexOf(getInventoryGems(), slotGem);
                    var source = _ui.getInventoryGem(invIndex);
                    _draggingGem.changeSource(source);
                    markUnsaved();
                }
            }
            else if (groupIndex != hoverGroup || hoverSlot != slotIndex)
            {
                var hoverGem = getSlotGem(hoverGroup, hoverSlot);
                moveGemBetweenSlots(groupIndex, slotIndex, hoverGroup, hoverSlot);
                await updateGemEffect(getSlotGroup(groupIndex));
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
                markUnsaved();
            }
            _draggingGem.release();
            _draggingGem = null;
            _draggingGemSlotGroupIndex = -1;
            _draggingGemSlotIndex = -1;
        }

        private async void onDeckItemClickedCallback(int index, PointerEventData pointer)
        {
            var deckItems = getEditorDeckItems();
            var deckItem = deckItems[index];
            await switchToCard(deckItem.cardRef);
            _ui.deckCardList.hidePreviewCard();
        }
        private async void onDeckItemPointerEnterCallback(int index, PointerEventData pointer)
        {
            var deckItems = getEditorDeckItems();
            var deckItem = deckItems[index];
            var cardDefine = game.card.getLocalCardDefine(deckItem.cardRef);
            await _ui.deckCardList.showPreviewCard(cardDefine, index, game.card);
        }
        private void onDeckItemPointerExitCallback(int index, PointerEventData pointer)
        {
            _ui.deckCardList.hidePreviewCard();
        }
        #endregion

        #endregion

        #region 宝石逻辑
        private void insertGemToSlot(int gemId, int groupIndex, int slotIndex)
        {
            var gem = _saveData.getGem(gemId);
            removeInventoryGem(gemId);
            setSlotGem(groupIndex, slotIndex, gem);
        }
        private bool removeGemFromSlot(int groupIndex, int slotIndex)
        {
            var gem = getSlotGem(groupIndex, slotIndex);
            if (gem == null)
                return false;
            setSlotGem(groupIndex, slotIndex, null);
            addInventoryGem(gem.id);
            return true;
        }
        private void moveGemBetweenSlots(int groupIndex, int slotIndex, int targetGroupIndex, int targetSlotIndex)
        {
            var gem = getSlotGem(groupIndex, slotIndex);
            var targetGem = getSlotGem(targetGroupIndex, targetSlotIndex);
            setSlotGem(groupIndex, slotIndex, null);
            setSlotGem(targetGroupIndex, targetSlotIndex, null);
            setSlotGem(groupIndex, slotIndex, targetGem);
            setSlotGem(targetGroupIndex, targetSlotIndex, gem);
        }
        private void setSlotGem(int groupIndex, int slotIndex, CardGem gem)
        {
            var slot = getSlot(groupIndex, slotIndex);
            slot.setGem(gem);

            PVEManager.setCardDefineInsertedGem(_cardDefine, slotIndex, gem);
            markUnsaved();
        }
        private void removeInventoryGem(int gemId)
        {
            _saveData.addUsedGem(gemId);
            updateInventory();
        }
        private void addInventoryGem(int gemId)
        {
            _saveData.removeUsedGem(gemId);
            updateInventory();
        }
        private CardGem getSlotGem(int groupIndex, int slotIndex)
        {
            var slot = getSlot(groupIndex, slotIndex);
            return slot.gem;
        }
        private CardGemSlot getSlot(int groupIndex, int slotIndex)
        {
            return getSlotGroup(groupIndex).getSlotAt(slotIndex);
        }
        private CardGemSlotGroup getSlotGroup(int groupIndex)
        {
            return _slotGroups[groupIndex];
        }
        private void updateInventory()
        {
            _inventoryGems = _saveData.getInventoryGems();
        }
        private CardGem[] getInventoryGems()
        {
            return _inventoryGems;
        }
        #endregion

        #region 节点图操作
        private void resetGraphToSource(ActionGraph sourceGraph, ActionGraph targetGraph)
        {
            targetGraph.Clear();

            // 复制
            var seri = new SerializableActionNodeGraph(sourceGraph);

            // 粘贴
            var definer = getNodeDefiner();
            ActionNodeGraphCtrl.pasteNodes(seri, targetGraph, Vector2.zero, definer, out _);
        }
        private Dictionary<int, int> cloneNodes(IEnumerable<Node> source, ActionGraph targetGraph)
        {
            if (source.Count() <= 0)
                return null;

            // 复制
            var seri = ActionNodeGraphCtrl.copyNodes(source, out var buffer);

            // 粘贴
            var definer = getNodeDefiner();
            var rect = new Rect(targetGraph.centerX - targetGraph.width * 0.5f, targetGraph.centerY - targetGraph.height * 0.5f, targetGraph.width, targetGraph.height);
            var rightTop = new Vector2(rect.xMax, rect.yMax);
            var pos = rightTop + new Vector2(200 + buffer.width * 0.5f, buffer.height * -0.5f);
            ActionNodeGraphCtrl.pasteNodes(seri, targetGraph, pos, definer, out var idMap);
            return idMap;
        }
        private ControlInput getFirstEnterPort(GeneratedEffect effect)
        {
            if (effect is IEntryNodeEffect entryEffect)
            {
                var node = entryEffect.entryNode;
                if (node is IEntryNode entryNode)
                {
                    return entryNode.getActionOutputPort().getConnectedInputPort();
                }
                else if (node is ActionNode action)
                {
                    return action.getExitPort().getConnectedInputPort();
                }
            }
            return null;
        }
        private (string name, ValueInput input)[] getConnectedValuePorts(GeneratedEffect effect)
        {
            if (effect is IEntryNodeEffect entryEffect)
            {
                var node = entryEffect.entryNode;
                var valueOutputs = node.getOutputPorts<ValueOutput>();
                return valueOutputs.SelectMany(o => o.connections.Select(c => (c.source.name, c.destination as ValueInput))).ToArray();
            }
            return null;
        }
        private (ValueOutput output, ValueInput input)[] getShouldConnectValuePorts(GeneratedEffect effect, (string name, ValueInput input)[] oldConnections, Dictionary<int, int> idMap)
        {
            if (effect is IEntryNodeEffect entryEffect)
            {
                var entryNode = entryEffect.entryNode;
                var graph = effect.graph;
                return oldConnections.Select(p =>
                {
                    var src = entryNode.getOutputPort<ValueOutput>(p.name);
                    var destNode = graph.findNodeById(idMap[p.input.node.id]);
                    var dest = destNode.getInputPort<ValueInput>(p.input.name);
                    return (src, dest);
                }).ToArray();
            }
            return null;
        }
        private ControlOutput getLastExitPort(GeneratedEffect effect)
        {
            if (effect is IEntryNodeEffect entryEffect)
            {
                return getExitPortChain(entryEffect.entryNode);
            }
            return null;
        }
        private ControlOutput getExitPortChain(Node node)
        {
            ControlOutput port = null;
            if (node is IEntryNode entryNode)
            {
                port = entryNode.getActionOutputPort();
            }
            else if (node is ActionNode action)
            {
                port = action.getExitPort();
            }
            var connected = port.getConnectedInputPort();
            if (connected != null)
            {
                return getExitPortChain(connected.node);
            }
            return port;
        }
        private async Task updateGemEffect(CardGemSlotGroup group)
        {
            var srcCardDefine = _srcCardDefine;
            var cardDefine = _cardDefine;
            var cardManager = game.card;
            var pveManager = game.pve;

            // 获取复制目标的效果。
            var destEffect = getGemGroupEffect(group);

            // 重置该效果的节点图。
            var destEffectIndex = Array.IndexOf(_cardDefine.getGeneratedEffects(), destEffect);
            var srcCardEffect = srcCardDefine.getGeneratedEffects()[destEffectIndex];
            destEffect.graph.Clear();
            resetGraphToSource(srcCardEffect.graph, destEffect.graph);

            // 根据插槽内的宝石更新效果的节点图。
            foreach (var slot in group.slots)
            {
                var gem = slot.gem;
                if (slot.gem == null)
                    continue;
                // 获取宝石对应的卡牌定义。
                var gemPoolRef = getCardPoolReference(gem.cardRef.cardPoolId);
                await cardManager.getOrLoadCardPool(gemPoolRef);
                var gemCardDefine = cardManager.getCardDefine(gemPoolRef, gem.cardRef.defineId);

                // 获取宝石卡牌定义的效果。
                var srcEffect = PVEManager.getSlotEffect(gemCardDefine, slot.tags);

                // 获取要复制的节点，要连接的动作端口和值端口。
                var nodes = srcEffect.graph.nodes;
                var cloneableNodes = srcEffect.graph.nodes.Where(n => ActionNodeGraphCtrl.CanNodeBeCloned(n));
                var connectedEnterPort = getFirstEnterPort(srcEffect);
                var connectedValuePorts = getConnectedValuePorts(srcEffect);

                // 没有要复制的节点则跳出。
                if (cloneableNodes.Count() <= 0)
                    continue;
                // 复制节点。
                var idMap = cloneNodes(cloneableNodes, destEffect.graph);
                // 没有复制成功则跳出。
                if (idMap == null)
                    continue;

                // 重新连接应该连接的端口。
                var firstEnterPortNodeId = idMap[connectedEnterPort.node.id];
                var firstEnterPortNode = destEffect.graph.findNodeById(firstEnterPortNodeId);
                var firstEnterPort = firstEnterPortNode.getInputPort<ControlInput>(connectedEnterPort.name);
                var valueConnnections = getShouldConnectValuePorts(destEffect, connectedValuePorts, idMap);
                var lastActionPort = getLastExitPort(destEffect);
                // 找不到对应的连接端口，跳出。
                if (valueConnnections == null || lastActionPort == null)
                    continue;

                foreach (var valueConnection in valueConnnections)
                {
                    destEffect.graph.connect(valueConnection.output, valueConnection.input);
                }
                destEffect.graph.connect(lastActionPort, firstEnterPort);
            }
            await _effectEditor.display(_cardDefine.cardPoolId, destEffect);
        }
        public GeneratedEffect getGemGroupEffect(CardGemSlotGroup group)
        {
            return _cardDefine.getGeneratedEffects()[group.effectIndex];
        }
        #endregion

        #region UI
        private async Task<CardGemViewData> getGemViewData(CardGem gem)
        {
            if (gem == null)
                return null;
            var gemPoolRef = getCardPoolReference(gem.cardRef.cardPoolId);
            var cardDefine = game.card.getCardDefine(gemPoolRef, gem.cardRef.defineId);
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
            var inventory = getInventoryGems();
            var inventoryView = new CardGemViewData[inventory.Length];
            for (int g = 0; g < inventoryView.Length; g++)
            {
                var gem = inventory[g];
                var gemViewData = await getGemViewData(gem);
                inventoryView[g] = gemViewData;
            }
            _ui.updateGemInventory(inventoryView);
        }
        private async Task updateGemSlots()
        {
            if (_slotGroups == null)
            {
                _ui.updateGemSlot(Array.Empty<CardGemSlotGroupViewData>());
                return;
            }
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
        private DeckItem[] getEditorDeckItems()
        {
            return _saveData.deck.cardList.Where(c =>
            {
                var cardDefine = game.card.getLocalCardDefine(c.cardRef);
                return PVEManager.cardDefineHasSlots(cardDefine);
            }).ToArray();
        }
        private async Task updateDeck()
        {
            var cardDefines = getEditorDeckItems();
            DeckCardViewData[] viewDatas = await DeckCardViewData.fromDeckItems(cardDefines, game);
            _ui.updateDeck(viewDatas);
        }
        private async Task switchToCard(DefineReference defineRef)
        {
            CardDefine srcCardDefine = null;
            CardDefine cardDefine = null;
            var (genPoolInfo, generatedPool) = await game.pve.getGeneratedCardPool(_dataName);
            var generatedCard = generatedPool.cardDefineList.FirstOrDefault(d => d.cardPoolId == defineRef.cardPoolId && d.id == defineRef.defineId);
            if (generatedCard != null)
            {
                cardDefine = generatedCard;
                var defRef = generatedCard.getProp<DefineReference>(PVEManager.CARD_PROP_TEMPLATE_DEFINE);
                srcCardDefine = game.card.getLocalCardDefine(defRef);
            }
            else
            {
                // 从PVE中找一张有插槽的卡牌定义复制。
                var (pvePoolInfo, pvePool) = await game.pve.getOrLoadPVECardPool();
                var pveCard = pvePool.cardDefineList.FirstOrDefault(d => d.cardPoolId == defineRef.cardPoolId && d.id == defineRef.defineId);
                var clonedCard = game.card.cloneCardDefine(pveCard);
                var cardSkins = game.card.cloneCardSkins(pvePoolInfo, pveCard.id);
                clonedCard.cardPoolId = genPoolInfo.cardPoolId;
                game.card.addCard(genPoolInfo.cardPoolId, clonedCard);
                foreach (var skin in cardSkins)
                {
                    var image = await game.card.loadCardImage(pveCard);
                    skin.imagePath = await game.card.saveImage(genPoolInfo.cardPoolId, image.texture);
                    game.card.addCardSkin(genPoolInfo.cardPoolId, skin);
                }
                clonedCard.setProp(PVEManager.CARD_PROP_TEMPLATE_DEFINE, pveCard.getDefineRef());

                srcCardDefine = pveCard;
                cardDefine = clonedCard;
            }
            _srcCardDefine = srcCardDefine;
            _cardDefine = cardDefine;
            _slotGroups = PVEManager.getCardDefineSlots(_saveData, _cardDefine);

            _ui.setCardSelected(true);
            await _ui.setCard(cardDefine, game.card);
            await updateGemSlots();
            var effect = cardDefine.getGeneratedEffects().FirstOrDefault();
            if (effect == null)
                _effectEditor.hideActionGraph();
            else
                await _effectEditor.display(cardDefine.cardPoolId, effect);
            _slotGroups = PVEManager.getCardDefineSlots(_saveData, cardDefine) ?? new CardGemSlotGroup[0];
            _ui.displayCardPage();
        }
        #endregion

        #region 卡池环境
        private ICardPoolReference[] getCardPoolEnv() => _cardPoolEnv;
        private ICardPoolReference getCardPoolReference(long cardPoolId) => game.card.getCardPoolReferenceInEnv(cardPoolId, getCardPoolEnv());
        private INodeDefiner getNodeDefiner() => game.card.getNodeDefiner(getCardPoolEnv());
        #endregion

        #endregion

        #region 属性字段
        private int _draggingGemSlotGroupIndex = -1;
        private int _draggingGemSlotIndex = -1;
        private DraggingCardGem _draggingGem;
        private CardGemSlotGroup[] _slotGroups;
        private CardDefine _srcCardDefine;
        private CardDefine _cardDefine;
        private SaveData _saveData;
        private CardGem[] _inventoryGems;
        private string _dataName;
        private bool _hasUnsavedChanges;
        private ICardPoolReference[] _cardPoolEnv;
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;

namespace Controllers
{
    partial class EditorCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public async void display(bool switchToView = true, bool reloadCardPoolInfos = true, int cardPoolIndex = -1, bool reloadCardPool = true)
        {
            //显示界面
            //切换模式
            if (switchToView)
            {
                //牌库浏览模式
                if (reloadCardPoolInfos)
                {
                    //重新加载所有牌库信息
                    await getCardPoolsInfoAsync();
                }
                gameObject.SetActive(true);
                clearAllCardPoolsUnsaved();
                _firstOpenedCardPoolId = -1;
                await displayView();
            }
            else
            {
                //牌库编辑模式
                long poolId = _curCardPoolId;
                if (cardPoolIndex >= 0 && cardPoolIndex < _cardPoolIdList.Count)
                {
                    poolId = _cardPoolIdList[cardPoolIndex];
                }
                try
                {
                    //显示卡牌列表
                    if (reloadCardPool)
                    {
                        var poolRef = getCardPoolRef(poolId);
                        var info = game.card.getCardPoolInfo(poolRef);
                        var path = game.card.getCardPoolPath(poolRef);
                        //重新加载牌库中的所有卡牌和皮肤
                        if (string.IsNullOrEmpty(path))
                        {
                            await readCardPoolByIdAsync(poolRef);
                        }
                        else
                        {
                            await readCardPoolByPathAsync(path);
                        }
                    }
                }
                catch (Exception e)
                {
                    game.card.showCardPoolLoadErrorDialog();
                    Debug.LogError(e);
                    return;
                }
                //设置当前卡池
                _curCardPoolId = poolId;
                if (_firstOpenedCardPoolId < 0)
                {
                    _firstOpenedCardPoolId = poolId;
                }
                var poolInfo = getCardPoolInfo(poolId);
                //更新牌库信息
                Sprite image = await game.card.loadCardPoolImage(poolInfo);
                //更新UI
                gameObject.SetActive(true);
                _ui.setDeckListActive(false);
                _ui.setEditDeckPanelActive(true);
                _ui.updateCurCardPoolInfo(poolInfo.cardPoolName, image);
                updatePasteButton();
                await updateDeckCardListUI();
                await _cardPoolViewPanelCtrl.display(cardPoolsRange: new long[] { poolId }, cardFilter: c =>
                {
                    //显示所有卡牌，包括角色，技能和衍生物
                    return true;
                });
            }
        }
        public Task openCardEditor(CardDefine cardDefine)
        {
            var infoData = new CardPoolDisplayObject(cardDefine);
            return openEditorByObject(infoData);
        }
        public Task openActionEditor(GeneratedActionDefine actionDefine)
        {
            var infoData = new CardPoolDisplayObject(actionDefine);
            return openEditorByObject(infoData);
        }
        public Task openBuffEditor(GeneratedBuffDefine buffDefine)
        {
            var infoData = new CardPoolDisplayObject(buffDefine);
            return openEditorByObject(infoData);
        }
        /// <summary>
        /// 关闭当前编辑器。如果正在进行跨卡池编辑，进行一次卡池保存检测。
        /// </summary>
        /// <param name="forceSave">是否必须选择保存或者不保存卡池，用于删除编辑器内容的场合。</param>
        /// <returns></returns>
        public async Task closeEditor(bool forceSave = false)
        {
            if (needToSaveCurrentCardPoolOnEditorClose())
            {
                showSaveCurrentCardPoolDialog(forceSave);
            }
            else
            {
                await closeCurrentEditor();
            }
        }
        public CardPoolDisplayPage getCurrentDisplayPage()
        {
            if (_displayObjectStack.Count == 0)
                return null;
            var key = _displayObjectStack.Peek();
            return getDisplayPage(key);
        }
        public string getTitleText(bool unsaved, string title)
        {
            return game.lang._(EDITOR_TITLE_TEMPLATE, title, unsaved ? game.lang._(EDITOR_UNSAVED_MARK) : string.Empty);
        }
        public async Task openReferedCard(DefineReference defRef, Action<CardDefine> setPropAction)
        {
            var cardPoolId = _curCardPoolId;
            if (defRef == null)
            {
                // 新建一个卡牌并打开
                await createNewCardAndOpen(setPropAction);
            }
            else 
            {
                var cardDefine = game.card.getLocalCardDefine(defRef);
                if (cardDefine == null)
                {
                    // 如果卡牌定义不存在，改为新建一个卡牌并打开。
                    await createNewCardAndOpen(setPropAction);
                }
                else
                {
                    // 卡牌定义存在
                    var curDisplayPage = getCurrentDisplayPage();
                    var curDisplayCardId = curDisplayPage?.displayObj?.cardDefine?.id ?? -1;
                    if (defRef.cardPoolId == cardPoolId && defRef.defineId == curDisplayCardId)
                    {
                        // 是同一张卡，什么也不做
                    }
                    else
                    {
                        // 打开
                        await openCardEditor(cardDefine);
                    }
                }
            }
        }
        public void removeDisplayPageOfObject(object obj)
        {
            _displayPagePool.RemoveAll(p => p.displayObj.compareObj(obj));
        }
        public void copyCard(DefineReference defineRef)
        {
            _copyingCard = defineRef;
            updatePasteButton();
        }
        #region 保存状态
        public void markCardPoolUnsaved(long cardPoolId)
        {
            _unsavedCardPools.Add(cardPoolId);
            updateTitleText();
        }
        public void clearCardPoolUnsaved(long cardPoolId)
        {
            _unsavedCardPools.Remove(cardPoolId);
            updateTitleText();
        }
        public void clearAllCardPoolsUnsaved()
        {
            _unsavedCardPools.Clear();
            updateTitleText();
        }
        public bool isCardPoolUnsaved(long cardPoolId)
        {
            return _unsavedCardPools.Contains(cardPoolId);
        }
        public bool isCurrentCardPoolUnsaved()
        {
            return isCardPoolUnsaved(_curCardPoolId);
        }
        private void updateTitleText()
        {
            _ui.setTitleText(getTitleText(isCurrentCardPoolUnsaved(), game.lang._(EDITOR_TITLE)));
        }
        #endregion
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {

            if (_ui == null)
            {
                _ui = GetComponentInChildren<EditorPanel>();
            }
            _ui.onClickReturnButton += onClickReturnButton;
            _ui.onClickDeckListItem += onClickCardPoolListItem;
            _ui.onClickDeckListItemDeleteButton += onClickDeleteCardPoolButton;
            _ui.onClickDeckListAddButton += onClickAddCardPoolButton;
            _ui.onClickSaveDeckButton += onClickSaveCardPoolButton;
            _ui.onClickReturnToCardPoolViewButton += onClickReturnToCardPoolViewButton;
            _ui.onClickDeleteCurCardPoolButton += onClickDeleteCurCardPoolButton;
            _ui.onClickEditCardPoolImageButton += onClickEditCardPoolImageButtonCallback;
            _ui.onDeckNameInputFieldValueChanged += onDeckNameInputFieldValueChanged;
            _ui.onClickNewCardButton += onClickNewCardButton;
            _ui.onClickPasteCardButton += onClickPasteCardButton;
            _ui.onClickExportCardPoolButton += onClickExportCardPoolButton;
            _ui.onClickImportCardPoolButton += onClickImportCardPoolButton;

            _cardPoolViewPanelCtrl.onClickCard += onClickCard;
            _cardPoolViewPanelCtrl.onUpdateCardListItem += onUpdateCardListItem;
        }
        #endregion
        #region 事件回调
        /// <summary>
        /// 当用户点击返回按钮，返回主菜单
        /// </summary>
        /// <param name="ui"></param>
        private async void onClickReturnButton()
        {
            if (isCurrentCardPoolUnsaved())
            {
                askSaveAndReturn();
            }
            else
            {
                if (_curCardPoolId < 0)
                {
                    _ui.gameObject.SetActive(false);
                    scene.menu.display();
                }
                else
                {
                    await returnToView();
                }
            }

        }
        /// <summary>
        /// 当用户点击卡池列表项，切换右侧栏到编辑卡牌状态，读取对应文件并显示卡牌列表
        /// </summary>
        /// <param name="index"></param>
        private void onClickCardPoolListItem(int index)
        {
            display(false, false, index, true);
        }
        /// <summary>
        /// 当用户点击删除卡池按钮，弹出对话框询问用户是否确定删除卡池，确定则删除并刷新卡池列表
        /// </summary>
        /// <param name="index"></param>
        private void onClickDeleteCardPoolButton(int index)
        {
            long cardPoolId = _cardPoolIdList[index];
            if (game.card.isInnateCardPool(cardPoolId))
            {
                game.ui.dialog.ShowConfirm(game.i18n._(StringTable.INVALID_OPERATION), game.i18n._(StringTable.EDITOR_DIALOG_CANNOT_DELETE_STANDARD_CARDPOOL), null);
            }
            else
            {
                game.ui.dialog.ShowQuestion(game.i18n._(StringTable.WARNING), game.i18n._(StringTable.EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL), async () =>
                {
                    clearAllCardPoolsUnsaved();
                    var poolRef = getCardPoolRef(cardPoolId);
                    game.card.removeCardPool(poolRef);
                    _cardPoolIdList.RemoveAt(index);
                    await updateDeckListUI();
                });
            }
        }
        private void onUpdateCardListItem(int index, EditorCardListItem item)
        {
            item.hideCount();
        }
        /// <summary>
        /// 当点击新增卡池按钮，切换右侧栏到编辑卡池状态
        /// </summary>
        private void onClickAddCardPoolButton()
        {
            game.card.createCardPool(out var cardPoolInfo, out _);
            _curCardPoolId = cardPoolInfo.cardPoolId;
            markCardPoolUnsaved(_curCardPoolId);
            _cardPoolIdList.Add(_curCardPoolId);

            display(false, false, _cardPoolIdList.IndexOf(_curCardPoolId), true);
        }
        /// <summary>
        /// 当卡牌列表被点击，打开卡牌编辑器，如果是新建卡牌则新建卡牌并打开
        /// </summary>
        /// <param name="index"></param>
        private async void onClickCard(CardDefine cardDefine)
        {
            await openCardEditor(cardDefine);
        }
        /// <summary>
        /// 当点击新建卡牌按钮
        /// </summary>
        private async void onClickNewCardButton()
        {
            createCard(out var newCardDefine, out _);
            await openCardEditor(newCardDefine);
        }
        /// <summary>
        /// 当点击粘贴卡牌按钮
        /// </summary>
        private async void onClickPasteCardButton()
        {
            if (_copyingCard == null)
                return;
            var cardPoolId = _curCardPoolId;
            markCardPoolUnsaved(cardPoolId);
            var cardPoolRef = game.card.getLocalCardPoolReference(_copyingCard.cardPoolId);
            var poolInfo = game.card.getCardPoolInfo(cardPoolRef);
            var cardDefine = game.card.getCardDefine(cardPoolRef, _copyingCard.defineId);
            var newCardDefine = game.card.cloneCardDefine(cardDefine);
            var newCardSkins = game.card.cloneCardSkins(poolInfo, cardDefine.id);

            var curCardPoolRef = game.card.getLocalCardPoolReference(cardPoolId);
            var (_, cardPool) = await game.card.getOrLoadCardPool(curCardPoolRef);

            newCardDefine.cardPoolId = cardPoolId;
            newCardDefine.id = game.card.allocCardDefineID(cardPool);
            game.card.addCard(cardPoolId, newCardDefine);
            foreach (var skin in newCardSkins)
            {
                skin.id = newCardDefine.id;
                game.card.addCardSkin(cardPoolId, skin);
            }
            display(false, false, -1, true);
        }
        /// <summary>
        /// 当点击保存按钮，保存当前编辑的卡池
        /// </summary>
        private void onClickSaveCardPoolButton()
        {
            trySaveCardPool();
        }
        /// <summary>
        /// 当点击导出按钮，导出当前编辑的卡池
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private async void onClickExportCardPoolButton()
        {
            if (isCurrentCardPoolUnsaved())
            {
                game.ui.dialog.ShowConfirm(
                    game.i18n._(StringTable.HINT_TITLE),
                    game.i18n._(EXPORT_BEFORE_SAVE), null);
                return;
            }
            try
            {
                var path = await game.card.exportCardPool(getCurrentCardPoolInfo());
                if (path != null)
                {
                    game.ui.dialog.ShowConfirm(game.i18n._(StringTable.HINT_TITLE), game.i18n._(EXPORT_SUCCESS, path));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), e.Message);
            }
        }

        private async void onClickImportCardPoolButton()
        {
            try
            {
                bool success = await game.card.importCardPool();
                if (success)
                {
                    game.ui.dialog.ShowConfirm(game.i18n._(StringTable.HINT_TITLE), game.i18n._(IMPORT_SUCCESS));
                    await getCardPoolsInfoAsync();
                    await updateDeckListUI();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), e.Message);
            }
        }

        /// <summary>
        /// 当点击返回浏览卡池按钮，清空字段并切换回浏览卡池状态
        /// </summary>
        private async void onClickReturnToCardPoolViewButton()
        {
            await returnToView();
        }
        /// <summary>
        /// 当点击删除当前卡池按钮，弹出对话框询问，如果确认则删除当前卡池并切换到浏览状态
        /// </summary>
        private void onClickDeleteCurCardPoolButton()
        {
            var cardPoolId = _curCardPoolId;
            if (game.card.isInnateCardPool(cardPoolId))
            {
                game.ui.dialog.ShowConfirm(
                    game.i18n._(StringTable.INVALID_OPERATION),
                    game.i18n._(StringTable.EDITOR_DIALOG_CANNOT_DELETE_STANDARD_CARDPOOL), null);
            }
            else
            {
                game.ui.dialog.ShowQuestion(game.i18n._(StringTable.WARNING), game.langManager.getString(StringTable.EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL), async () =>
                {
                    clearAllCardPoolsUnsaved();
                    //删除当前卡池
                    var poolRef = getCardPoolRef(cardPoolId);
                    game.card.removeCardPool(poolRef);
                    int index = _cardPoolIdList.IndexOf(cardPoolId);
                    _cardPoolIdList.RemoveAt(index);
                    await updateDeckListUI();
                    await returnToView();
                });
            }
        }
        private async void onClickEditCardPoolImageButtonCallback()
        {
            Sprite sprite = await game.card.selectExternalImage();
            if (sprite != null)
            {
                markCardPoolUnsaved(_curCardPoolId);
                var poolInfo = getCurrentCardPoolInfo();
                poolInfo.imageResPath = game.card.getSpritePath(sprite);
                _ui.updateCurCardPoolInfo(poolInfo.cardPoolName, sprite);
            }
        }
        private void onDeckNameInputFieldValueChanged(string value)
        {
            markCardPoolUnsaved(_curCardPoolId);
            var poolInfo = getCurrentCardPoolInfo();
            poolInfo.cardPoolName = value;
            _cardPoolViewPanelCtrl.updateCardPoolFilterNames();
        }
        #endregion
        /// <summary>
        /// 读取所有卡池文件中的卡池信息。
        /// </summary>
        /// <returns>包含卡池文件相对路径和卡池信息键值对的字典</returns>
        private async Task getCardPoolsInfoAsync()
        {
            _cardPoolIdList.Clear();
            var result = await game.card.loadCardPoolInfosAsync();
            foreach (var pair in result)
            {
                CardPoolInfo cardPool = pair.Value;
                var poolId = cardPool.cardPoolId;
                if (game.card.isCardPoolHidden(poolId))
                    continue;
                int index = _cardPoolIdList.FindIndex(id => id == poolId);
                if (index >= 0)
                {
                    var poolRef = getCardPoolRef(poolId);
                    var conflictPath = game.card.getCardPoolPath(poolRef);
                    Debug.LogWarning($"无法加载卡池{pair.Key}，因为{conflictPath}拥有与其相同的Id{poolId}");
                    continue;
                }
                _cardPoolIdList.Add(poolId);
            }
        }
        /// <summary>
        /// 读取指定路径上的卡池文件中的所有卡牌
        /// </summary>
        /// <param name="cardPoolId">卡池文件相对路径</param>
        /// <returns></returns>
        /// <exception cref="IOException">当读取文件失败的时候抛出这个异常</exception>
        private Task readCardPoolByPathAsync(string cardPoolPath)
        {
            return game.card.getOrLoadCardPool(cardPoolPath);
        }
        /// <summary>
        /// 读取指定id的卡池文件中的所有卡牌
        /// </summary>
        /// <param name="cardPoolRef">卡池引用。</param>
        /// <returns></returns>
        private Task readCardPoolByIdAsync(ICardPoolReference cardPoolRef)
        {
            return game.card.getOrLoadCardPool(cardPoolRef);
        }
        /// <summary>
        /// 尝试返回上层UI。
        /// </summary>
        /// <returns></returns>
        private void askSaveAndReturn()
        {
            string[] options = new string[3]
            {
                game.lang._(EDITOR_OPTION_SAVE),
                game.lang._(EDITOR_OPTION_DONT_SAVE),
                game.lang._(EDITOR_OPTION_CANCEL),
            };
            game.ui.dialog.ShowOptions(game.langManager.getString(StringTable.WARNING), game.langManager.getString(StringTable.EDITOR_WARNING_UNSAVED_CHANGES),
                options,
                async (i) =>
                {
                    switch (i)
                    {
                        case 0:
                            trySaveCardPool(async save =>
                            {
                                if (save)
                                    await returnToView();
                            });
                            break;
                        case 1:
                            await returnToView();
                            break;
                    }
                });
        }

        /// <summary>
        /// 尝试保存卡池，会检查有没有过时节点。
        /// </summary>
        /// <returns></returns>
        private async void trySaveCardPool(Action<bool> onEndAction = null)
        {
            if (isCurrentCardPoolUnsaved())
            {
                if (hasObsoleteNodes(out string desc))
                {
                    game.ui.dialog.ShowQuestion(game.langManager.getString(StringTable.WARNING), desc,
                        async () =>
                        {
                            await saveCardPool();
                            onEndAction?.Invoke(true);
                        },
                        () => {
                            onEndAction?.Invoke(false);
                        });
                }
                else
                {
                    await saveCardPool();
                    onEndAction?.Invoke(true);
                }
            }
        }
        /// <summary>
        /// 保存一个卡池。
        /// </summary>
        /// <returns></returns>
        private async Task<bool> saveCardPool()
        {
            bool success = await game.card.saveCardPool(getCurrentCardPoolInfo());
            if (success)
            {
                clearCardPoolUnsaved(_curCardPoolId);
            }
            return success;
        }
        /// <summary>
        /// 卡尺内是否有过时的节点。
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        private bool hasObsoleteNodes(out string desc)
        {
            // 检查有无过时的节点。
            var cardPoolId = _curCardPoolId;
            var cardPoolRef = getCardPoolRef(cardPoolId);
            if (!game.card.tryGetCardPool(cardPoolRef, out var cardPool))
            {
                throw new NullReferenceException("尝试保存一个不存在的卡池。");
            }
            var obsoleteGraphs = game.card.getObsoleteNodeGraphs(cardPool);

            // 没有过时的节点。
            if (obsoleteGraphs.Length <= 0)
            {
                desc = null;
                return false;
            }
            // 有过时的节点。
            // 获取描述。
            const int MAX_DISPLAY_COUNT = 4;
            List<string> descs = new List<string>();
            for (int i = 0; i < Math.Min(obsoleteGraphs.Length, MAX_DISPLAY_COUNT); i++)
            {
                var (cardId, buffId, actionId) = obsoleteGraphs[i];
                if (cardId != 0)
                {
                    var cardName = game.card.getCardSkin(cardPoolRef, cardId)?.name;
                    descs.Add(cardName ?? StringTable.BUILD_UNKNOW_CARD_NAME);
                }
                else if (buffId != 0)
                {
                    string buffName = null;
                    if (game.card.tryGetBuffDefine(cardPoolRef, buffId, out var buffDefine))
                    {
                        buffName = game.card.getBuffSkin(buffDefine)?.name;
                    }
                    descs.Add(buffName ?? EDITOR_UNKNOWN_BUFF_NAME);
                }
                else if (actionId != 0)
                {
                    var actionDefine = game.card.getActionDefine(cardPoolRef, actionId);
                    string actionName = actionDefine?.editorName ?? EDITOR_UNKNOWN_ACTION_NAME;
                    descs.Add(actionName);
                }
            }
            string obsoleteString = string.Join(",", descs);
            if (obsoleteGraphs.Length > MAX_DISPLAY_COUNT)
            {
                obsoleteString = game.lang._(EDITOR_WARNING_OBSOLETE_NODES_DESC_MAX, obsoleteString);
            }
            desc = game.lang._(EDITOR_WARNING_OBSOLETE_NODES_TEMPLATE, obsoleteString);
            return true;
        }
        /// <summary>
        /// 清空字段并切换到浏览状态
        /// </summary>
        private async Task returnToView()
        {
            await reloadCurrentCardPool();
            clearAllCardPoolsUnsaved();
            _curCardPoolId = -1;
            _firstOpenedCardPoolId = -1;
            await displayView();
        }
        /// <summary>
        /// 重新加载当前卡池。
        /// </summary>
        /// <returns></returns>
        private async Task reloadCurrentCardPool()
        {
            var cardPoolId = _curCardPoolId;
            if (game.card.isCreatingCardPool(cardPoolId)) // 正在创建新卡池，退出时直接把创建的这个卡池删除
            {
                game.card.deleteCreatingCardPool();
                //重新加载所有卡池信息
                await getCardPoolsInfoAsync();
            }
            else if (isCurrentCardPoolUnsaved()) // 退出时重新加载牌库信息，防止出现临时卡牌变更。
            {
                await game.card.loadCardPoolInfosAsync();
                var cardPoolRef = game.card.getLocalCardPoolReference(cardPoolId);
                var path = game.card.getCardPoolPath(cardPoolRef);
                await game.card.loadCardPool(path);
            }
            // 删除所有属于该卡池的页面信息。
            _displayPagePool.RemoveAll(p => p.cardPoolId == _curCardPoolId);
            clearCardPoolUnsaved(_curCardPoolId);
        }
        /// <summary>
        /// 显示卡池列表UI
        /// </summary>
        /// <returns></returns>
        private async Task displayView()
        {
            _ui.setDeckListActive(true);
            _ui.setEditDeckPanelActive(false);
            await _cardPoolViewPanelCtrl.hide();
            await updateDeckListUI();
        }
        /// <summary>
        /// 更新卡池列表UI
        /// </summary>
        /// <returns></returns>
        private async Task updateDeckListUI()
        {
            (string name, Sprite image)[] viewData = new (string name, Sprite image)[_cardPoolIdList.Count];
            for (int i = 0; i < viewData.Length; i++)
            {
                var cardPoolId = _cardPoolIdList[i];
                var cardPoolInfo = getCardPoolInfo(cardPoolId);
                viewData[i].name = cardPoolInfo.cardPoolName;
                viewData[i].image = await game.card.loadCardPoolImage(cardPoolInfo);
            }
            _ui.updateDeckList(viewData);
        }
        private async Task updateDeckCardListUI()
        {
            var cardPoolId = _curCardPoolId;
            var poolRef = getCardPoolRef(cardPoolId);
            CardDefine[] cardDefines = game.card.getCardDefines(poolRef).Select(p => p.Value).ToArray();
            (string name, Sprite image, int count)[] viewData = new (string name, Sprite image, int count)[cardDefines.Length];
            for (int i = 0; i < viewData.Length; i++)
            {
                CardSkinData skin = game.card.getCardSkin(cardDefines[i]);
                viewData[i].name = skin.name;
                viewData[i].image = await game.card.loadCardImage(poolRef, skin);
                viewData[i].count = 0;
            }
            _ui.updateDeckCardList(viewData);
        }
        private void createCard(out CardDefine newCardDefine, out CardSkinData skin)
        {
            var cardPoolId = _curCardPoolId;
            markCardPoolUnsaved(cardPoolId);
            var cardPoolInfo = getCardPoolInfo(cardPoolId);
            game.card.createCard(cardPoolInfo, out newCardDefine, out skin);
            game.card.addCard(cardPoolId, newCardDefine);
            game.card.addCardSkin(cardPoolId, skin);
        }
        private async Task createNewCardAndOpen(Action<CardDefine> setPropAction)
        {
            createCard(out var newDefine, out _);
            setPropAction?.Invoke(newDefine);

            await openCardEditor(newDefine);
        }
        private CardPoolDisplayPage getDisplayPage(CardPoolDisplayObject infoData)
        {
            CardPoolDisplayPage displayInfo = null;
            displayInfo = _displayPagePool.FirstOrDefault(d => d.displayObj.compare(infoData));
            if (displayInfo == null)
            {
                if (infoData.cardDefine is CardDefine cardDefine)
                {
                    var newCard = game.card.cloneCardDefine(cardDefine);
                    var poolInfo = getCardPoolInfo(cardDefine.cardPoolId);
                    var newCardSkins = game.card.cloneCardSkins(poolInfo, infoData.cardDefine.id);
                    var displayObj = new CardPoolDisplayObject(newCard);
                    displayInfo = new CardPoolDisplayPage()
                    {
                        cardPoolId = newCard.cardPoolId,
                        displayObj = displayObj,
                        skins = newCardSkins
                    };
                    _displayPagePool.Add(displayInfo);
                }
                else if (infoData.buffDefine is GeneratedBuffDefine buffDefine)
                {
                    var newBuff = game.card.cloneBuffDefine(buffDefine);
                    var newBuffSkins = game.card.cloneBuffSkins(buffDefine);
                    var displayObj = new CardPoolDisplayObject(newBuff);
                    displayInfo = new CardPoolDisplayPage()
                    {
                        cardPoolId = newBuff.cardPoolId,
                        displayObj = displayObj,
                        skins = newBuffSkins
                    };
                    _displayPagePool.Add(displayInfo);
                }
                else if (infoData.actionDefine is GeneratedActionDefine actionDefine)
                {
                    var newAction = game.card.cloneActionDefine(actionDefine.cardPoolId, actionDefine);
                    var displayObj = new CardPoolDisplayObject(newAction);
                    displayInfo = new CardPoolDisplayPage()
                    {
                        cardPoolId = newAction.cardPoolId,
                        displayObj = displayObj,
                    };
                    _displayPagePool.Add(displayInfo);
                }
            }
            return displayInfo;
        }
        private async Task openEditorByObject(CardPoolDisplayObject data)
        {
            var displayInfo = getDisplayPage(data);
            _displayObjectStack.Push(data);
            if (_currentEditor)
                _currentEditor.hide();
            _ui.gameObject.SetActive(false);
            _currentEditor = await openEditorByPage(displayInfo);
        }
        /// <summary>
        /// 根据卡池显示信息显示卡牌编辑器、增益编辑器等。
        /// </summary>
        /// <param name="displayInfo">卡池显示信息</param>
        /// <returns>显示的编辑器。</returns>
        private async Task<CardPoolEditor> openEditorByPage(CardPoolDisplayPage displayInfo)
        {
            CardPoolEditor editor = null;
            var displayObj = displayInfo.displayObj;
            if (displayObj.cardDefine != null)
                editor = scene.cardEditor;
            else if (displayObj.buffDefine != null)
                editor = scene.buffEditor;
            else if (displayObj.actionDefine != null)
                editor = scene.actionEditor;
            if (editor != null)
            {
                _curCardPoolId = displayInfo.cardPoolId;
                await editor.display(displayInfo);
            }
            return editor;
        }
        private ICardPoolReference getCardPoolRef(long cardPoolId)
        {
            return game.card.getLocalCardPoolReference(cardPoolId);
        }
        private CardPoolInfo getCardPoolInfo(long cardPoolId)
        {
            return game.card.getCardPoolInfo(getCardPoolRef(cardPoolId));
        }
        private CardPoolInfo getCurrentCardPoolInfo()
        {
            return getCardPoolInfo(_curCardPoolId);
        }
        /// <summary>
        /// 在跨卡池编辑中，显示是否保存当前卡池的对话框。
        /// </summary>
        /// <param name="forceSave">是否必须保存或者不保存。</param>
        private void showSaveCurrentCardPoolDialog(bool forceSave)
        {
            string title = game.lang._(StringTable.WARNING);
            CardPoolInfo cardPoolInfo = getCurrentCardPoolInfo();
            string content = null;
            string[] options = null;
            Action<int> clickButtonAction = null;
            if (!Application.isEditor && game.card.isInnateCardPool(_curCardPoolId))
            {
                // 尝试保存经典卡池
                content = game.lang._(EDITOR_TRY_TO_SAVE_STANDARD_CARDPOOL);
                options = new string[2]
                {
                        game.lang._(StringTable.CONFIRM),
                        game.lang._(EDITOR_OPTION_CANCEL)
                };
                clickButtonAction = async (index) =>
                {
                    if (index == 0)
                    {
                        await reloadCurrentCardPool();
                        await closeCurrentEditor();
                    }
                };
            }
            else
            {
                // 经典卡池以外的卡池
                content = game.lang._(EDITOR_NEED_TO_SAVE_CARDPOOL, cardPoolInfo.cardPoolName);
                if (forceSave)
                {
                    // 强制保存或不保存
                    options = new string[2]
                    {
                            game.lang._(EDITOR_OPTION_SAVE),
                            game.lang._(EDITOR_OPTION_DONT_SAVE)
                    };
                    clickButtonAction = async (index) =>
                    {
                        switch (index)
                        {
                            case 0:
                                await saveCardPool();
                                break;
                            case 1:
                                await reloadCurrentCardPool();
                                break;
                        }
                        await closeCurrentEditor();
                    };
                }
                else
                {
                    // 不强制保存
                    options = new string[3]
                    {
                            game.lang._(EDITOR_OPTION_SAVE),
                            game.lang._(EDITOR_OPTION_DONT_SAVE),
                            game.lang._(EDITOR_OPTION_CANCEL)
                    };
                    clickButtonAction = async (index) =>
                    {
                        switch (index)
                        {
                            case 0:
                                trySaveCardPool(async (success) =>
                                {
                                    if (success)
                                        await closeCurrentEditor();
                                });
                                break;
                            case 1:
                                await reloadCurrentCardPool();
                                await closeCurrentEditor();
                                break;
                        }
                    };
                }
            }
            game.ui.dialog.ShowOptions(title, content, options, clickButtonAction);
        }
        /// <summary>
        /// 关闭当前的卡牌/增益/自定义动作编辑器。
        /// </summary>
        private async Task closeCurrentEditor()
        {
            _currentEditor.hide();
            _currentEditor = null;
            if (_displayObjectStack.Count > 1)
            {
                var lastDisplayObj = _displayObjectStack.Pop();
                // 如果所有打开的编辑器中都已经没有这个显示物体了，从显示信息池中移除这个显示物体。
                if (!_displayObjectStack.Contains(lastDisplayObj))
                {
                    removeDisplayPageOfObject(lastDisplayObj);
                }
                var infoObj = _displayObjectStack.Peek();
                var displayInfo = getDisplayPage(infoObj);
                _currentEditor = await openEditorByPage(displayInfo);
            }
            else
            {
                _displayObjectStack.Clear();
                _displayPagePool.Clear();
                display(false, false, -1, true);
            }
        }
        /// <summary>
        /// 在退出该编辑器时，是否要保存当前卡池。
        /// </summary>
        /// <returns>是否要保存当前卡池。</returns>
        private bool needToSaveCurrentCardPoolOnEditorClose()
        {
            if (_curCardPoolId == _firstOpenedCardPoolId) // 编辑的卡池是一开始打开的卡池
                return false;
            if (!isCardPoolUnsaved(_curCardPoolId)) // 没有任何更改
                return false;
            if (_displayObjectStack.Count <= 0) // 当前没有正在打开的编辑器页面
                return false;
            if (_displayObjectStack.Count(obj => getDisplayPage(obj).cardPoolId == _curCardPoolId) > 1) // 目前的编辑器不是栈中最后一个属于目前卡池的编辑器
                return false;
            // 编辑的卡池不是一开始打开的卡池，并且目前的编辑器是栈中最后一个属于目前卡池的编辑器
            return true;
        }
        private void updatePasteButton()
        {
            _ui.setPasteButtonInteractable(_copyingCard != null);
        }
        #endregion
        #region 属性字段
        [TranslateMsg("卡池编辑器/退出跨卡池编辑器/提示经典卡池被修改")]
        public const string EDITOR_TRY_TO_SAVE_STANDARD_CARDPOOL = "您对经典卡池进行了更改，但该卡池无法被修改。\n是否要离开该卡池？"; 
        [TranslateMsg("卡池编辑器/退出跨卡池编辑器/提示要求保存卡池（{0}是卡池名）")]
        public const string EDITOR_NEED_TO_SAVE_CARDPOOL = "您对卡池({0})进行了更改，是否要保存该卡池？";
        [TranslateMsg("卡池编辑器/退出未保存卡池/选项名")]
        public const string EDITOR_OPTION_SAVE = "保存";
        [TranslateMsg("卡池编辑器/退出未保存卡池/选项名")]
        public const string EDITOR_OPTION_DONT_SAVE = "不保存";
        [TranslateMsg("卡池编辑器/退出未保存卡池/选项名")]
        public const string EDITOR_OPTION_CANCEL = "取消";
        [TranslateMsg("编辑器/标题模板，{0}为编辑器名称，{1}为未保存标识（星号）")]
        public const string EDITOR_TITLE_TEMPLATE = "· {0}{1} ·";
        [TranslateMsg("编辑器/未保存标识（星号）")]
        public const string EDITOR_UNSAVED_MARK = "*";
        [TranslateMsg("卡池编辑器/标题")]
        public const string EDITOR_TITLE = "卡牌编辑器";
        [TranslateMsg]
        const string EXPORT_BEFORE_SAVE = "当前卡池含有未保存的修改，请在导出前保存修改";
        [TranslateMsg]
        const string EDITOR_UNKNOWN_BUFF_NAME = "未知增益";
        [TranslateMsg]
        const string EDITOR_UNKNOWN_ACTION_NAME = "未知自定义动作";
        [TranslateMsg("尝试保存具有过时节点的卡池时的警告。（{0}：过时节点列表）")]
        const string EDITOR_WARNING_OBSOLETE_NODES_TEMPLATE = "以下内容包含过时的节点：\n{0}\n是否保存卡池？";
        [TranslateMsg("尝试保存具有过时节点的卡池，并且显示达到上限时的警告。（{0}：过时节点列表）")]
        const string EDITOR_WARNING_OBSOLETE_NODES_DESC_MAX = "{0}...";
        [TranslateMsg("{0}是路径")]
        const string EXPORT_SUCCESS = "卡池已导出至 {0}";
        [TranslateMsg]
        const string IMPORT_SUCCESS = "卡池已导入";



        public string nodeClipboard { get; set; }
        private EditorPanel _ui;
        private List<long> _cardPoolIdList = new List<long>();
        private long _curCardPoolId = -1;
        private DefineReference _copyingCard = null;
        private HashSet<long> _unsavedCardPools = new HashSet<long>();
        long _firstOpenedCardPoolId;
        /// <summary>
        /// 编辑器对于显示物体的引用栈，用于返回上一个编辑器。
        /// </summary>
        private Stack<CardPoolDisplayObject> _displayObjectStack = new Stack<CardPoolDisplayObject>();
        /// <summary>
        /// 显示物体和显示页面的对应，包含当前打开的所有显示物体和显示页面。
        /// </summary>
        private List<CardPoolDisplayPage> _displayPagePool = new List<CardPoolDisplayPage>();
        private CardPoolEditor _currentEditor;
        [SerializeField]
        private CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        [SerializeField]
        private bool _hasUnsavedChanges;
        #endregion
    }
}

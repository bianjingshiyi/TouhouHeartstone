using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
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
                        var poolRef = getLocalCardPoolRef(poolId);
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
                var poolInfo = getCardPoolInfo(poolId);
                setCurrentCardPool(poolInfo);
                if (_firstOpenedCardPoolId < 0)
                {
                    _firstOpenedCardPoolId = poolId;
                }
                //更新牌库信息
                Sprite image = await game.card.loadCardPoolImage(poolInfo);

                // 事件
                updateAllEvents();
                _ui.initEventNames(eventTriggerTimeEditorNames);

                // 自定义规则
                var ruleNames = _gameRuleLogicTypes
                    .Select(t => game.editor.getRuleLogicTypeEditorName(t))
                    .Append(game.lang._(RULE_NAME_EVENT_LOGIC))
                    .ToArray();
                _ui.initRuleNames(ruleNames);
                updateUIRuleList();

                // 卡池依赖
                updateUIDependencyList();

                //更新UI
                gameObject.SetActive(true);
                _ui.setCardPoolListActive(false);
                _ui.setEditCardPoolPanelActive(true);
                _ui.updateCurCardPoolInfo(poolInfo.cardPoolName, image);
                updatePasteButton();
                await _cardPoolViewPanelCtrl.display(cardPoolsRange: new long[] { poolId }, cardFilter: c =>
                {
                    //显示所有卡牌，包括角色，技能和衍生物
                    return true;
                });
            }
        }

        #region 打开编辑器
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
        public Task openRuleEditor(GeneratedRuleLogic ruleLogic)
        {
            var infoData = new CardPoolDisplayObject(ruleLogic);
            return openEditorByObject(infoData);
        }

        #endregion

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
            var curCardPoolId = _curCardPoolId;
            if (defRef == null)
            {
                // 新建一个卡牌并打开
                await createNewCardAndOpen(setPropAction);
            }
            else
            {
                var sourcePoolRef = getEnvCardPoolRef(defRef.cardPoolId);
                var cardDefine = game.card.getCardDefine(sourcePoolRef, defRef.defineId);
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
                    if (defRef.cardPoolId == curCardPoolId && defRef.defineId == curDisplayCardId)
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
        public void updateAllEvents()
        {
            var poolRefs = getCardPoolEnv();
            var mainEventDefines = game.card.getAllEventDefines(includeHidden: false, poolRefs);
            eventTriggerTimes = mainEventDefines.getEventTriggerTimes();
            eventReferences = mainEventDefines.Select(d => d.getReference()).ToArray();
            eventTriggerTimeEditorNames = eventTriggerTimes.Select(t => game.eventManager.translateEventTriggerTimeNameForEditor(t)).ToArray();
            eventEditorNames = mainEventDefines.Select(d => game.eventManager.translateEventDefineNameForEditor(d)).ToArray();
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
            // 卡池列表
            _ui.onClickReturnButton += onClickReturnButton;
            _ui.onClickAddCardPoolButton += onClickAddCardPoolButton;
            _ui.onClickCardPoolListItem += onClickCardPoolListItem;
            _ui.onClickCardPoolListItemDeleteButton += onClickDeleteCardPoolButton;
            _ui.onClickImportCardPoolButton += onClickImportCardPoolButton;

            // 编辑器UI
            _ui.onClickSaveCardPoolButton += onClickSaveCardPoolButton;
            _ui.onClickReturnToCardPoolViewButton += onClickReturnToCardPoolViewButton;
            _ui.onClickDeleteCurCardPoolButton += onClickDeleteCurCardPoolButton;
            _ui.onClickEditCardPoolImageButton += onClickEditCardPoolImageButtonCallback;
            _ui.onCardPoolNameInputFieldValueChanged += onCardPoolNameInputFieldValueChanged;
            _ui.onClickNewCardButton += onClickNewCardButton;
            _ui.onClickPasteCardButton += onClickPasteCardButton;
            _ui.onClickExportCardPoolButton += onClickExportCardPoolButton;

            // 自定义规则
            _ui.onClickAddRuleItem += onClickAddRuleItemCallback;
            _ui.onClickRuleItem += onClickRuleItemCallback;
            _ui.onClickRemoveRuleItem += onClickRemoveRuleItemCallback;
            _ui.onConfrimCreateEventRule += onConfirmCreateEventRuleCallback;

            // 卡池依赖
            _ui.onClickAddCardPoolDepButton += onClickAddCardPoolDepButtonCallback;
            _ui.onClickRemoveCardPoolDepButton += onClickRemoveCardPoolDefButtonCallback;
            _ui.onClickAddCardPoolDepItem += onClickAddCardPoolDepItemCallback;
            _ui.onClickConfirmAddCardPoolDepButton += onClickConfirmAddCardPoolDepButtonCallback;
            _ui.onClickUpdateCardPoolDepsButton += onClickUpdateCardPoolDepsButtonCallback;

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
                    var poolRef = getLocalCardPoolRef(cardPoolId);
                    game.card.removeCardPool(poolRef);
                    _cardPoolIdList.RemoveAt(index);
                    await updateCardPoolListUI();
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
            setCurrentCardPool(cardPoolInfo);
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

            var sourcePoolRef = getLocalCardPoolRef(_copyingCard.cardPoolId);
            var sourceCardDefine = game.card.getCardDefine(sourcePoolRef, _copyingCard.defineId);
            if (sourceCardDefine == null) return;

            var sourcePoolInfo = game.card.getCardPoolInfo(sourcePoolRef);

            var curCardPoolId = _curCardPoolId;
            markCardPoolUnsaved(curCardPoolId);
            var newCardDefine = game.card.cloneCardDefine(sourceCardDefine);
            var newCardSkins = game.card.cloneCardSkins(sourcePoolInfo, sourceCardDefine.id);

            var curCardPoolRef = getCurrentCardPoolRef();
            var (_, cardPool) = await game.card.getOrLoadCardPool(curCardPoolRef);

            newCardDefine.cardPoolId = curCardPoolId;
            newCardDefine.id = game.card.allocCardDefineID(cardPool);
            game.card.addCard(curCardPoolId, newCardDefine);
            foreach (var skin in newCardSkins)
            {
                skin.id = newCardDefine.id;
                game.card.addCardSkin(curCardPoolId, skin);
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
                    await updateCardPoolListUI();
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
            if (game.card.isInnateCardPool(_curCardPoolId))
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
                    var cardPoolId = _curCardPoolId;
                    var poolRef = getCurrentCardPoolRef();
                    game.card.removeCardPool(poolRef);
                    int index = _cardPoolIdList.IndexOf(cardPoolId);
                    _cardPoolIdList.RemoveAt(index);
                    await updateCardPoolListUI();
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
        private void onCardPoolNameInputFieldValueChanged(string value)
        {
            markCardPoolUnsaved(_curCardPoolId);
            var poolInfo = getCurrentCardPoolInfo();
            poolInfo.cardPoolName = value;
            _cardPoolViewPanelCtrl.updateCardPoolFilterNames();
        }

        #region 自定义规则
        private void onClickAddRuleItemCallback(int index)
        {
            if (index < _gameRuleLogicTypes.Length)
            {
                markCardPoolUnsaved(_curCardPoolId);
                var logicType = _gameRuleLogicTypes[index];
                GeneratedRuleLogic logic = game.card.createRuleGameLogic(_curCardPoolId, logicType);
                logic.name = game.editor.getRuleLogicTypeEditorName(logicType);
                game.card.addRuleLogic(_curCardPoolId, logic);
                openRuleEditor(logic);
            }
            else
            {
                _ui.openCreateEventRuleDialog();
            }
        }
        private void onClickRuleItemCallback(int index)
        {
            var ruleLogics = getRuleLogics();
            GeneratedRuleLogic logic = ruleLogics[index];
            openRuleEditor(logic);
        }
        private void onClickRemoveRuleItemCallback(int index)
        {
            var ruleLogics = getRuleLogics();
            var logic = ruleLogics[index];
            var logicName = logic.name;
            var title = game.lang._(StringTable.WARNING);
            var content = game.lang._(EDITOR_WARNING_DELETE_RULE, logicName);
            game.ui.dialog.ShowQuestion(title, content, () =>
            {
                markCardPoolUnsaved(_curCardPoolId);
                game.card.removeRuleLogic(logic);
                updateUIRuleList();
            });
        }
        private void onConfirmCreateEventRuleCallback(int index)
        {
            markCardPoolUnsaved(_curCardPoolId);
            var triggerTime = eventTriggerTimes[index];
            var logic = game.card.createRuleEventLogic(_curCardPoolId, triggerTime);
            logic.name = game.eventManager.translateEventTriggerTimeNameForEditor(triggerTime);
            game.card.addRuleLogic(_curCardPoolId, logic);
            _ui.hideCreateEventRuleDialog();
            openRuleEditor(logic);
        }
        #endregion

        #region 卡池依赖
        private async void onClickAddCardPoolDepButtonCallback()
        {
            _viewingAddCardPoolDependencies.Clear();
            _selectedAddingCardPoolDependencies.Clear();
            var cardPoolInfos = await game.card.loadCardPoolInfosAsync();
            var curDeps = getCurrentCardPoolInfo().dependencyRefs;
            foreach (var info in cardPoolInfos.Values)
            {
                if (info.cardPoolId == _curCardPoolId)
                    continue;
                if (curDeps != null && curDeps.Any(d => d.CompareCardPool(info)))
                    continue;
                _viewingAddCardPoolDependencies.Add(info.ToReference());
            }

            _ui.openSelectCardPoolDependencyDialog();
            await updateAddCardPoolDependenciesDialog();
        }
        private void onClickRemoveCardPoolDefButtonCallback(int index)
        {
            var poolInfo = getCurrentCardPoolInfo();
            var depRefs = poolInfo.dependencyRefs;
            if (depRefs == null)
                return;

            var depRef = depRefs[index];
            var depInfo = game.card.getCardPoolInfo(depRef) ?? getLocalCardPoolInfo(depRef.cardPoolId);
            var depName = depInfo?.cardPoolName ?? game.lang._(EDITOR_UNKNOWN_CARD_POOL);

            var title = game.lang._(StringTable.WARNING);
            var content = game.lang._(EDITOR_WARNING_DELETE_DEPENDENCY, depName, depRef.version);
            game.ui.dialog.ShowQuestion(title, content, () =>
            {
                markCardPoolUnsaved(_curCardPoolId);

                var newRefs = new List<CardPoolReference>();
                newRefs.AddRange(depRefs);
                newRefs.RemoveAt(index);
                poolInfo.dependencyRefs = newRefs.ToArray();

                updateUIDependencyList();
            });
        }
        private async void onClickAddCardPoolDepItemCallback(int index)
        {
            var depRef = _viewingAddCardPoolDependencies[index];
            if (_selectedAddingCardPoolDependencies.Contains(depRef))
            {
                _selectedAddingCardPoolDependencies.Remove(depRef);
            }
            else
            {
                _selectedAddingCardPoolDependencies.Add(depRef);
            }
            await updateAddCardPoolDependenciesDialog();
        }
        private void onClickConfirmAddCardPoolDepButtonCallback()
        {
            markCardPoolUnsaved(_curCardPoolId);

            var poolInfo = getCurrentCardPoolInfo();
            var depRefs = poolInfo.dependencyRefs;

            var newRefs = new List<CardPoolReference>();
            if (depRefs != null)
                newRefs.AddRange(depRefs);
            newRefs.AddRange(_selectedAddingCardPoolDependencies);
            poolInfo.dependencyRefs = newRefs.ToArray();

            _ui.hideSelectCardPoolDependencyDialog();

            updateUIDependencyList();
        }
        private void onClickUpdateCardPoolDepsButtonCallback()
        {
            var updateableDependencies = getUpdateableDependencies();
            if (updateableDependencies.Length > 0)
            {
                markCardPoolUnsaved(_curCardPoolId);
                updateDependencies(updateableDependencies);
                updateUIDependencyList();
            }
        }
        #endregion

        #endregion

        #region 自定义规则
        private void updateUIRuleList()
        {
            var ruleLogics = getRuleLogics();
            var names = new string[ruleLogics.Length];
            for (int i = 0; i < names.Length; i++)
            {
                var logic = ruleLogics[i];
                names[i] = logic.name;
            }
            _ui.updateRules(names);
        }
        #endregion

        #region 卡池依赖
        private ICardPoolReference[] getUpdateableDependencies()
        {
            var poolInfo = getCurrentCardPoolInfo();
            if (poolInfo == null)
                return Array.Empty<ICardPoolReference>();
            var depRefs = poolInfo.dependencyRefs;
            if (depRefs == null)
                return Array.Empty<ICardPoolReference>();
            return depRefs.Where(depRef =>
            {
                var poolInfo = getCardPoolInfo(depRef.cardPoolId);
                if (poolInfo == null)
                    return false;
                if (poolInfo.editTime == depRef.version)
                    return false;
                return true;
            }).ToArray();
        }
        private void updateDependencies(ICardPoolReference[] depRefs)
        {
            var curPoolInfo = getCurrentCardPoolInfo();
            var oldDeps = curPoolInfo.dependencyRefs;
            var count = oldDeps?.Length ?? 0;
            var newDeps = new CardPoolReference[count];
            for (int i = 0; i < count; i++)
            {
                var oldDep = oldDeps[i];
                var newDep = oldDep;
                if (depRefs.Any(d => d.CompareCardPool(oldDep)))
                {
                    var poolInfo = getCardPoolInfo(oldDep.cardPoolId);
                    newDep = poolInfo.ToReference();
                }
                newDeps[i] = newDep;
            }
            curPoolInfo.dependencyRefs = newDeps;
        }
        private async Task updateAddCardPoolDependenciesDialog()
        {
            var deps = _viewingAddCardPoolDependencies;
            var viewDatas = new (string name, Sprite sprite, bool selected)[deps.Count];
            for (int i = 0; i < viewDatas.Length; i++)
            {
                var dep = deps[i];
                var info = game.card.getCardPoolInfo(dep);
                var name = info.cardPoolName;
                var sprite = await game.card.loadCardPoolImage(info);
                var selected = _selectedAddingCardPoolDependencies.Contains(dep);
                viewDatas[i] = (name, sprite, selected);
            }
            _ui.updateSelectCardPoolDependencyDialog(viewDatas);
        }
        private void updateUIDependencyList()
        {
            var poolInfo = getCurrentCardPoolInfo();
            var depRefs = poolInfo.dependencyRefs;
            var count = depRefs?.Length ?? 0;
            var names = new string[count];
            var outdated = new bool[count];
            for (int i = 0; i < names.Length; i++)
            {
                var depRef = depRefs[i];
                var refInfo = game.card.getCardPoolInfo(depRef);
                if (refInfo != null)
                {
                    names[i] = game.lang._(EDITOR_CARD_POOL_DEPENDENCY_TEMPLATE, refInfo.cardPoolName, depRef.version);
                }
                else
                {
                    var localInfo = depRef != null ? game.card.getLocalCardPoolInfo(depRef.cardPoolId) : null;
                    if (localInfo != null)
                    {
                        names[i] = game.lang._(EDITOR_CARD_POOL_DEPENDENCY_TEMPLATE, localInfo.cardPoolName, depRef.version);
                    }
                    else
                    {
                        names[i] = game.lang._(EDITOR_UNKNOWN_CARD_POOL);
                    }
                    outdated[i] = true;
                }
            }
            _ui.updateCardPoolDependencies(names, outdated);
            _ui.setAutoUpdateDepButtonInteractable(getUpdateableDependencies().Length > 0);
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
                    var poolRef = getLocalCardPoolRef(poolId);
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
                if (hasObsoleteNodes(_curCardPoolId, out string desc))
                {
                    game.ui.dialog.ShowQuestion(game.langManager.getString(StringTable.WARNING), desc,
                        async () =>
                        {
                            await saveCardPool();
                            onEndAction?.Invoke(true);
                        },
                        () =>
                        {
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
            var info = getCurrentCardPoolInfo();
            bool success = await game.card.saveCardPool(info);
            if (success)
            {
                clearCardPoolUnsaved(_curCardPoolId);
                game.editor.reloadCardPoolEnv(info);
            }
            return success;
        }
        /// <summary>
        /// 卡尺内是否有过时的节点。
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        private bool hasObsoleteNodes(long cardPoolId, out string desc)
        {
            // 检查有无过时的节点。
            var cardPoolRef = getEnvCardPoolRef(cardPoolId);
            if (!game.card.tryGetCardPool(cardPoolRef, out var cardPool))
            {
                desc = null;
                return false;
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
                var nodeInfo = obsoleteGraphs[i];
                if (nodeInfo.cardId != 0)
                {
                    var cardName = game.card.getCardSkin(cardPoolRef, nodeInfo.cardId)?.name;
                    descs.Add(cardName ?? StringTable.BUILD_UNKNOW_CARD_NAME);
                }
                else if (nodeInfo.buffId != 0)
                {
                    string buffName = null;
                    if (game.card.tryGetBuffDefine(cardPoolRef, nodeInfo.buffId, out var buffDefine))
                    {
                        buffName = game.card.getBuffSkin(buffDefine)?.name;
                    }
                    descs.Add(buffName ?? EDITOR_UNKNOWN_BUFF_NAME);
                }
                else if (nodeInfo.actionId != 0)
                {
                    var actionDefine = game.card.getActionDefine(cardPoolRef, nodeInfo.actionId);
                    string actionName = actionDefine?.editorName ?? EDITOR_UNKNOWN_ACTION_NAME;
                    descs.Add(actionName);
                }
                else if (nodeInfo.ruleId != 0)
                {
                    var ruleLogic = game.card.getRuleLogic(cardPoolRef, nodeInfo.ruleId);
                    string ruleName = ruleLogic?.name ?? EDITOR_UNKNOWN_RULE_NAME;
                    descs.Add(ruleName);
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
            await quitCardPool();
            clearAllCardPoolsUnsaved();
            clearCurrentCardPool();
            _firstOpenedCardPoolId = -1;
            await displayView();
        }
        /// <summary>
        /// 重新加载当前卡池。
        /// </summary>
        /// <returns></returns>
        private async Task quitCardPool()
        {
            var cardPoolId = _curCardPoolId;
            if (game.card.isCreatingCardPool(cardPoolId)) // 正在创建新卡池，退出时直接把创建的这个卡池删除
            {
                game.card.deleteCreatingCardPool();
                //重新加载所有卡池信息
                await getCardPoolsInfoAsync();
            }
            else if (isCurrentCardPoolUnsaved()) // 未保存，退出时重新加载牌库信息，防止出现临时卡牌变更。
            {
                await game.card.loadCardPoolInfosAsync();
                var cardPoolRef = game.card.getLocalCardPoolReference(cardPoolId);
                var path = game.card.getCardPoolPath(cardPoolRef);
                await game.card.loadCardPool(path);
            }
            // 删除所有属于该卡池的页面信息。
            _displayPagePool.RemoveAll(p => p.cardPoolId == cardPoolId);
            _cardPoolInfoClones.RemoveAll(c => c.cardPoolId == cardPoolId);
            clearCardPoolUnsaved(_curCardPoolId);
        }
        /// <summary>
        /// 显示卡池列表UI
        /// </summary>
        /// <returns></returns>
        private async Task displayView()
        {
            _ui.setCardPoolListActive(true);
            _ui.setEditCardPoolPanelActive(false);
            await _cardPoolViewPanelCtrl.hide();
            await updateCardPoolListUI();
        }
        /// <summary>
        /// 更新卡池列表UI
        /// </summary>
        /// <returns></returns>
        private async Task updateCardPoolListUI()
        {
            (string name, Sprite image, bool errorTip)[] viewData = new (string name, Sprite image, bool errorTip)[_cardPoolIdList.Count];
            for (int i = 0; i < viewData.Length; i++)
            {
                var cardPoolId = _cardPoolIdList[i];
                var cardPoolInfo = getLocalCardPoolInfo(cardPoolId);
                viewData[i].name = cardPoolInfo.cardPoolName;
                viewData[i].image = await game.card.loadCardPoolImage(cardPoolInfo);
                viewData[i].errorTip = false;
            }
            _ui.updateCardPoolList(viewData);
        }
        private void createCard(out CardDefine newCardDefine, out CardSkinData skin)
        {
            var cardPoolRef = getCurrentCardPoolRef();
            var cardPoolId = cardPoolRef.CardPoolID;
            markCardPoolUnsaved(cardPoolId);
            game.card.createCard(cardPoolRef, out newCardDefine, out skin);
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
                    var poolRef = getEnvCardPoolRef(cardDefine.cardPoolId);
                    var newCardSkins = game.card.cloneCardSkins(poolRef, infoData.cardDefine.id);
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
                else if (infoData.ruleLogic is GeneratedRuleLogic ruleLogic)
                {
                    var newLogic = game.card.cloneRuleLogic(ruleLogic.cardPoolId, ruleLogic);
                    var displayObj = new CardPoolDisplayObject(newLogic);
                    displayInfo = new CardPoolDisplayPage()
                    {
                        cardPoolId = newLogic.cardPoolId,
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
            switch (displayObj.type)
            {
                case CardPoolDisplayObject.Type.Card:
                    editor = scene.cardEditor;
                    break;
                case CardPoolDisplayObject.Type.Buff:
                    editor = scene.buffEditor;
                    break;
                case CardPoolDisplayObject.Type.Action:
                    editor = scene.actionEditor;
                    break;
                case CardPoolDisplayObject.Type.RuleLogic:
                    editor = scene.ruleEditor;
                    break;

            }
            if (editor != null)
            {
                var poolInfo = getCardPoolInfo(displayInfo.cardPoolId);
                setCurrentCardPool(poolInfo);
                await editor.display(displayInfo);
            }
            return editor;
        }

        #region 卡池环境
        private void setCurrentCardPool(CardPoolInfo poolInfo)
        {
            _curCardPoolId = poolInfo.cardPoolId;
            if (!_cardPoolInfoClones.Exists(c => c.cardPoolId == poolInfo.cardPoolId))
                _cardPoolInfoClones.Add(cloneCardPoolInfo(poolInfo));
            game.editor.reloadCardPoolEnv(poolInfo);
        }
        private void clearCurrentCardPool()
        {
            _curCardPoolId = -1;
            _cardPoolInfoClones.Clear();
            game.editor.clearCardPoolEnv();
        }
        private ICardPoolReference[] getCardPoolEnv()
        {
            return game.editor.getEnvCardPoolRefs();
        }
        private GeneratedRuleLogic[] getRuleLogics()
        {
            var cardPoolRef = getCurrentCardPoolRef();
            return game.card.getRuleLogics(cardPoolRef);
        }
        #endregion

        #region 卡池引用
        private ICardPoolReference getLocalCardPoolRef(long cardPoolId)
        {
            return game.card.getLocalCardPoolReference(cardPoolId);
        }
        private ICardPoolReference getEnvCardPoolRef(long cardPoolId)
        {
            return game.card.getCardPoolReferenceInEnv(cardPoolId, getCardPoolEnv());
        }
        private ICardPoolReference getCurrentCardPoolRef()
        {
            return getCurrentCardPoolInfo().ToReference();
        }
        #endregion

        #region 卡池信息
        /// <summary>
        /// 获取正在编辑中的卡池信息复制。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <returns>获取到的卡池信息复制。</returns>
        private CardPoolInfo getEditingCardPoolInfo(long cardPoolId)
        {
            return _cardPoolInfoClones.FirstOrDefault(c => c.cardPoolId == cardPoolId);
        }
        /// <summary>
        /// 获取尚未更改的，本地文件的卡池信息。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <returns>获取到的卡池信息。</returns>
        private CardPoolInfo getLocalCardPoolInfo(long cardPoolId)
        {
            return game.card.getLocalCardPoolInfo(cardPoolId);
        }
        /// <summary>
        /// 根据卡池ID获取卡池信息。优先获取正在编辑的卡池信息复制，然后才是尚未更改的本地文件的卡池信息。
        /// </summary>
        /// <param name="cardPoolId">卡池ID。</param>
        /// <returns>获取到的卡池信息。</returns>
        private CardPoolInfo getCardPoolInfo(long cardPoolId)
        {
            return getEditingCardPoolInfo(cardPoolId) ?? getLocalCardPoolInfo(cardPoolId);
        }
        /// <summary>
        /// 获取当前正在编辑的卡池信息。
        /// </summary>
        /// <returns>获取到的卡池信息。</returns>
        private CardPoolInfo getCurrentCardPoolInfo()
        {
            return getEditingCardPoolInfo(_curCardPoolId);
        }
        private CardPoolInfo cloneCardPoolInfo(CardPoolInfo poolInfo)
        {
            var seriInfo = new SerializableCardPoolInfo(poolInfo);
            var json = seriInfo.toJson();
            var seriClone = JsonHelper.fromJson<SerializableCardPoolInfo>(json);
            return seriClone.toInfo();
        }
        #endregion
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
                        await quitCardPool();
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
                        }
                        await quitCardPool();
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
                                    {
                                        await quitCardPool();
                                        await closeCurrentEditor();
                                    }
                                });
                                break;
                            case 1:
                                await quitCardPool();
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
            if (_copyingCard != null)
            {
                var cardPoolRef = getLocalCardPoolRef(_copyingCard.cardPoolId);
                var cardDefine = game.card.getCardDefine(cardPoolRef, _copyingCard.defineId);
                if (cardDefine != null)
                {
                    _ui.setPasteButtonInteractable(true);
                    return;
                }
            }
            _ui.setPasteButtonInteractable(false);
            _copyingCard = null;
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
        [TranslateMsg]
        const string EDITOR_UNKNOWN_RULE_NAME = "未知自定义规则";
        [TranslateMsg("尝试保存具有过时节点的卡池时的警告。（{0}：过时节点列表）")]
        const string EDITOR_WARNING_OBSOLETE_NODES_TEMPLATE = "以下内容包含过时的节点：\n{0}\n是否保存卡池？";
        [TranslateMsg("尝试保存具有过时节点的卡池，并且显示达到上限时的警告。（{0}：过时节点列表）")]
        const string EDITOR_WARNING_OBSOLETE_NODES_DESC_MAX = "{0}...";
        [TranslateMsg("{0}是路径")]
        const string EXPORT_SUCCESS = "卡池已导出至 {0}";
        [TranslateMsg]
        const string IMPORT_SUCCESS = "卡池已导入";

        [TranslateMsg("卡池编辑器-创建规则选项")]
        public const string RULE_NAME_EVENT_LOGIC = "事件规则";
        [TranslateMsg("卡池编辑器-删除规则警告，{0}为规则的名字")]
        public const string EDITOR_WARNING_DELETE_RULE = "确定要删除自定义规则“{0}”？";
        [TranslateMsg("卡池编辑器-删除卡池依赖警告，{0}为卡池的名字，{1}为卡池版本")]
        public const string EDITOR_WARNING_DELETE_DEPENDENCY = "确定要删除卡池依赖“{0}（版本{1}）”？";
        [TranslateMsg("卡池编辑器-卡池依赖名称模板（{0}为卡池名称，{1}为卡池版本）")]
        public const string EDITOR_CARD_POOL_DEPENDENCY_TEMPLATE = "·{0}（版本{1}）";
        [TranslateMsg("卡池编辑器-丢失的卡池依赖名称")]
        public const string EDITOR_UNKNOWN_CARD_POOL = "未知卡池";


        public string nodeClipboard { get; set; }
        private EditorPanel _ui;
        private List<long> _cardPoolIdList = new List<long>();
        private long _curCardPoolId = -1;
        private DefineReference _copyingCard = null;
        private HashSet<long> _unsavedCardPools = new HashSet<long>();
        private long _firstOpenedCardPoolId;
        private List<CardPoolInfo> _cardPoolInfoClones = new List<CardPoolInfo>();
        private List<CardPoolReference> _viewingAddCardPoolDependencies = new List<CardPoolReference>();
        private List<CardPoolReference> _selectedAddingCardPoolDependencies = new List<CardPoolReference>();
        /// <summary>
        /// 编辑器对于显示物体的引用栈，用于返回上一个编辑器。
        /// </summary>
        private Stack<CardPoolDisplayObject> _displayObjectStack = new Stack<CardPoolDisplayObject>();
        /// <summary>
        /// 显示物体和显示页面的对应，包含当前打开的所有显示物体和显示页面。
        /// </summary>
        private List<CardPoolDisplayPage> _displayPagePool = new List<CardPoolDisplayPage>();
        private CardPoolEditor _currentEditor;
        private static GameRuleLogicType[] _gameRuleLogicTypes = new GameRuleLogicType[]
        {
            GameRuleLogicType.GameStart,
            GameRuleLogicType.PlayerEnter,
            GameRuleLogicType.PlayerExit,
            GameRuleLogicType.GameEnd
        };
        [SerializeField]
        private CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        [SerializeField]
        private bool _hasUnsavedChanges;

        #region 事件
        public EventTriggerTime[] eventTriggerTimes { get; private set; }
        public EventReference[] eventReferences { get; private set; }
        public string[] eventTriggerTimeEditorNames { get; private set; }
        public string[] eventEditorNames { get; private set; }
        #endregion

        #endregion
    }
}

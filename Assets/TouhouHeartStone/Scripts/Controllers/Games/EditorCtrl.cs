using System;
using System.Collections;
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
using UnityEditor;
using UnityEngine;

namespace Controllers
{
    partial class EditorCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public async void display(bool switchToView = true, int cardPoolIndex = -1, bool reloadCardPool = true)
        {
            //显示界面
            //切换模式
            if (switchToView)
            {
                loadCardPoolInfos();
                gameObject.SetActive(true);
                clearAllCardPoolsUnsaved();
                _firstOpenedCardPoolId = -1;
                await displayView();
            }
            else
            {
                //牌库编辑模式
                long poolId = curCardPoolId;
                if (cardPoolIndex >= 0 && cardPoolIndex < _cardPoolIdList.Count)
                {
                    poolId = _cardPoolIdList[cardPoolIndex];
                }
                try
                {
                    startWaiting();
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
                finally
                {
                    endWaiting();
                }
                _curCardPoolImageLang = LangManager.SOURCE;
                _curCardPoolNameLang = LangManager.SOURCE;
                _ui.setCardPoolNameLanguage(0);
                //设置当前卡池
                var poolInfo = getCardPoolInfo(poolId);
                await setCurrentCardPool(poolInfo);
                if (_firstOpenedCardPoolId < 0)
                {
                    _firstOpenedCardPoolId = poolId;
                }
                //更新牌库信息
                await updateCardPoolImageButton();

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
            var curCardPoolId = this.curCardPoolId;
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
            eventReferences = mainEventDefines.Select(d => d.getReference()).Prepend(EventReference.ANY).ToArray();
            eventTriggerTimeEditorNames = eventTriggerTimes.Select(t => game.eventManager.translateEventTriggerTimeNameForEditor(t)).ToArray();
            eventEditorNames = mainEventDefines.Select(d => game.eventManager.translateEventDefineNameForEditor(d)).Prepend(game.eventManager.getAnyEventDefineNameForEditor()).ToArray();
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
            return isCardPoolUnsaved(curCardPoolId);
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
            _ui.initLanguages(game.lang.getLanguagesWithSource().Select(lang => game.lang.getLanguageShortCode(lang)).ToArray());
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
            _ui.onCardPoolNameInputFieldLanguageChanged += onCardPoolNameInputFieldLanguageChanged;
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
            _ui.onClickRemoveCardPoolDepButton += onClickRemoveCardPoolDepButtonCallback;
            _ui.onClickUpdateCardPoolDepsButton += onClickUpdateCardPoolDepsButtonCallback;

            _cardPoolViewPanelCtrl.onClickCard += onClickCard;
            _cardPoolViewPanelCtrl.onCardHoverEnter += (item, def) => scene.addLocalCardPopup(item.card, def);
            _cardPoolViewPanelCtrl.onCardHoverExit += (item, def) => scene.removeCardPopup(item.card);
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
                if (curCardPoolId < 0)
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
            display(false, index, true);
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
        private async void onClickAddCardPoolButton()
        {
            game.card.createCardPool(out var cardPoolInfo, out _);
            await setCurrentCardPool(cardPoolInfo);
            markCardPoolUnsaved(curCardPoolId);
            _cardPoolIdList.Add(curCardPoolId);

            display(false, _cardPoolIdList.IndexOf(curCardPoolId), true);
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
        private void onClickPasteCardButton()
        {
            if (_copyingCard == null)
                return;

            ICardPoolReference sourcePoolRef = getLocalCardPoolRef(_copyingCard.cardPoolId);
            CardDefine sourceCardDefine = game.card.getCardDefine(sourcePoolRef, _copyingCard.defineId);
            if (sourceCardDefine == null) return;

            StartCoroutine(pasteCardCoroutine(sourceCardDefine));
        }
        IEnumerator pasteCardCoroutine(CardDefine originCardDefine)
        {
            markCardPoolUnsaved(curCardPoolId);
            ActionGraph.defaultNodeDefiner = game.card.getLocalNodeDefiner();
            // 复制卡牌定义
            CardDefine newCardDefine = game.card.pasteCardDefine(curCardPoolId, originCardDefine);
            // 检查是否引用了增益定义
            List<BuffReference> buffRefs = new List<BuffReference>(originCardDefine.collectBuffReferences());
            if (buffRefs.Count > 0)
            {
                List<BuffDefine> buffs = new List<BuffDefine>();
                for (int i = 0; i < buffRefs.Count; i++)
                {
                    BuffReference buffRef = buffRefs[i];
                    if (game.card.tryGetBuffDefine(game.card.getLocalCardPoolReference(buffRef.cardPoolId), buffRef.defineId, out GeneratedBuffDefine buffDefine))
                    {
                        buffs.Add(buffDefine);
                        // 收集增益定义引用的增益定义
                        foreach (BuffReference newBuffRef in buffDefine.collectBuffReferences())
                        {
                            if (!buffRefs.Contains(newBuffRef))
                            {
                                buffRefs.Add(newBuffRef);
                            }
                        }
                    }
                }
                // 询问是否复制增益定义
                List<BuffDefine> newBuffDefines = new();
                Dictionary<BuffReference, BuffReference> replacedBuffRefDict = new Dictionary<BuffReference, BuffReference>();
                if (buffs.Count > 0)
                {
                    foreach (BuffDefine buff in buffs)
                    {
                        bool? isNewBuff = null;
                        game.ui.dialog.ShowQuestion(string.Empty, string.Format(game.i18n._(StringTable.EDITOR_DIALOG_CONFIRM_COPY_BUFF),
                            game.card.getCardName(originCardDefine),
                            game.card.getBuffName(buff)), () =>
                        {
                            isNewBuff = true;
                        }, () =>
                        {
                            isNewBuff = false;
                        });
                        yield return new WaitUntil(() => isNewBuff != null);
                        if (isNewBuff.Value)
                        {
                            // 复制增益定义
                            BuffDefine newBuffDefine = game.card.pasteBuffDefine(curCardPoolId, buff);
                            newBuffDefines.Add(newBuffDefine);
                            replacedBuffRefDict[buff.getDefineRef()] = newBuffDefine.getDefineRef();
                        }
                    }
                }
                // 替换卡牌定义与增益定义中的引用
                foreach (var pair in replacedBuffRefDict)
                {
                    newCardDefine.ReplaceBuffReference(pair.Key, pair.Value);
                    foreach (BuffDefine newBuffDefine in newBuffDefines)
                    {
                        newBuffDefine.ReplaceBuffReference(pair.Key, pair.Value);
                    }
                }
            }

            display(false, -1, true);
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
                startWaiting();
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
            finally
            {
                endWaiting();
            }
        }

        private async void onClickImportCardPoolButton()
        {
            try
            {
                startWaiting();
                bool success = await game.card.importCardPool();
                if (success)
                {
                    game.ui.dialog.ShowConfirm(game.i18n._(StringTable.HINT_TITLE), game.i18n._(IMPORT_SUCCESS));
                    loadCardPoolInfos();
                    await updateCardPoolListUI();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), e.Message);
            }
            finally
            {
                endWaiting();
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
            if (game.card.isInnateCardPool(curCardPoolId))
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
                    var cardPoolId = curCardPoolId;
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
                markCardPoolUnsaved(curCardPoolId);
                var poolInfo = getCurrentCardPoolInfo();
                game.card.setCardPoolImagePath(poolInfo, _curCardPoolImageLang, game.card.getResourcePath(sprite));
                await updateCardPoolImageButton();
            }
        }
        private async void onCardPoolNameInputFieldValueChanged(string value)
        {
            markCardPoolUnsaved(curCardPoolId);
            var poolInfo = getCurrentCardPoolInfo();
            game.card.setCardPoolName(poolInfo, _curCardPoolNameLang, value);
            await updateCardPoolImageButton();
        }
        private async void onCardPoolNameInputFieldLanguageChanged(int index)
        {
            _curCardPoolNameLang = game.lang.getLanguagesWithSource()[index];
            await updateCardPoolImageButton();
        }

        #region 自定义规则
        private void onClickAddRuleItemCallback(int index)
        {
            if (index < _gameRuleLogicTypes.Length)
            {
                markCardPoolUnsaved(curCardPoolId);
                var logicType = _gameRuleLogicTypes[index];
                GeneratedRuleLogic logic = game.card.createRuleGameLogic(curCardPoolId, logicType);
                logic.name = game.editor.getRuleLogicTypeEditorName(logicType);
                game.card.addRuleLogic(curCardPoolId, logic);
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
                markCardPoolUnsaved(curCardPoolId);
                game.card.removeRuleLogic(logic);
                updateUIRuleList();
            });
        }
        private void onConfirmCreateEventRuleCallback(int index)
        {
            markCardPoolUnsaved(curCardPoolId);
            var triggerTime = eventTriggerTimes[index];
            var logic = game.card.createRuleEventLogic(curCardPoolId, triggerTime);
            logic.name = game.eventManager.translateEventTriggerTimeNameForEditor(triggerTime);
            game.card.addRuleLogic(curCardPoolId, logic);
            _ui.hideCreateEventRuleDialog();
            openRuleEditor(logic);
        }
        #endregion

        #region 卡池依赖
        private async void onClickAddCardPoolDepButtonCallback()
        {
            var cardPools = new List<CardPoolReference>();
            var cardPoolInfos = game.card.getLocalCardPoolInfos();
            var curDeps = getCurrentCardPoolInfo().dependencyRefs;
            foreach (var info in cardPoolInfos)
            {
                if (info.cardPoolId == curCardPoolId)
                    continue;
                if (curDeps != null && curDeps.Any(d => d.CompareCardPool(info)))
                    continue;
                cardPools.Add(info.ToReference());
            }
            var source = new CardPoolSelectSourceDependency(this, cardPools)
            {
                title = game.lang._(EDITOR_TITLE_SELECT_CARD_POOL_DEPENDENCY)
            };
            await scene.displayCardPoolMultiSelectPanel(source);
        }
        private void onClickRemoveCardPoolDepButtonCallback(int index)
        {
            var poolInfo = getCurrentCardPoolInfo();
            var depRefs = poolInfo.dependencyRefs;
            if (depRefs == null)
                return;

            var depRef = depRefs[index];
            var depInfo = game.card.getCardPoolInfo(depRef) ?? getLocalCardPoolInfo(depRef.cardPoolId);
            var depName = game.card.getCardPoolName(depInfo);

            var title = game.lang._(StringTable.WARNING);
            var depFullName = game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE, depName, depRef.version);
            var content = game.lang._(EDITOR_WARNING_DELETE_DEPENDENCY, depFullName);
            game.ui.dialog.ShowQuestion(title, content, () =>
            {
                markCardPoolUnsaved(curCardPoolId);

                var newRefs = new List<CardPoolReference>();
                newRefs.AddRange(depRefs);
                newRefs.RemoveAt(index);
                poolInfo.dependencyRefs = newRefs.ToArray();

                updateUIDependencyList();
            });
        }
        private void onClickUpdateCardPoolDepsButtonCallback()
        {
            var updateableDependencies = getUpdateableDependencies();
            if (updateableDependencies.Length > 0)
            {
                markCardPoolUnsaved(curCardPoolId);
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
        private bool setCardPoolDependencies(IEnumerable<ICardPoolReference> selected)
        {
            if (checkDependencyCycle(selected, out var cyclePools))
            {
                showDependencyCycleDialog(cyclePools);
                return false;
            }

            markCardPoolUnsaved(curCardPoolId);

            var poolInfo = getCurrentCardPoolInfo();
            var depRefs = poolInfo.dependencyRefs;

            var newRefs = new List<CardPoolReference>();
            if (depRefs != null)
                newRefs.AddRange(depRefs);
            newRefs.AddRange(selected.Select(s => s.ToReference()));
            poolInfo.dependencyRefs = newRefs.ToArray();

            updateUIDependencyList();
            return true;
        }
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
                    var refInfoName = game.card.getCardPoolName(refInfo);
                    names[i] = game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE_DOT, refInfoName, depRef.version);
                }
                else
                {
                    var localInfo = depRef != null ? game.card.getLocalCardPoolInfo(depRef.cardPoolId) : null;
                    if (localInfo != null)
                    {
                        var cardPoolName = game.card.getCardPoolName(localInfo);
                        names[i] = game.lang._(StringTable.EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE_DOT, cardPoolName, depRef.version);
                    }
                    else
                    {
                        names[i] = game.lang._(StringTable.EDITOR_UNKNOWN_CARD_POOL);
                    }
                    outdated[i] = true;
                }
            }
            _ui.updateCardPoolDependencies(names, outdated);
            _ui.setAutoUpdateDepButtonInteractable(getUpdateableDependencies().Length > 0);
        }
        private bool checkDependencyCycle(IEnumerable<ICardPoolReference> addingDeps, out ICardPoolReference[] cycleDeps)
        {
            var poolInfo = getCurrentCardPoolInfo();
            var newRefs = new List<ICardPoolReference>();
            var depRefs = poolInfo.dependencyRefs;
            if (depRefs != null)
                newRefs.AddRange(depRefs);
            if (addingDeps != null)
                newRefs.AddRange(addingDeps);

            var list = new List<ICardPoolReference>();
            foreach (var depRef in newRefs)
            {
                var depsOfDep = depRef.CardPoolID.GetCardPoolDependencies(game.card);
                if (depsOfDep.Contains(poolInfo.cardPoolId))
                {
                    list.Add(depRef);
                }
            }
            cycleDeps = list.ToArray();
            return list.Count > 0;
        }
        private void showDependencyCycleDialog(ICardPoolReference[] cyclePools)
        {
            const int maxDisplayCount = 5;
            var title = game.lang._(StringTable.ERROR);
            var poolNames = cyclePools.Select(r => game.card.getCardPoolName(game.card.getCardPoolInfo(r)) ?? game.lang._(StringTable.EDITOR_UNKNOWN_CARD_POOL));
            var trim = poolNames.Count() > maxDisplayCount;
            var poolName = string.Join(',', poolNames.Take(maxDisplayCount).ToArray());
            poolName = trim ? game.lang._(StringTable.TRIMMED_COTENTS, poolName) : poolName;
            var desc = game.lang._(EDITOR_WARNING_CYCLE_DEPENDENCY, poolName);
            game.ui.dialog.ShowConfirm(title, desc);
        }
        #endregion

        #region 卡池非法字符
        private bool checkCardPoolNameInvalid()
        {
            var poolInfo = getCurrentCardPoolInfo();
            if (poolInfo == null)
                return false;
            var localizedString = poolInfo.getLocalizedString(poolInfo.cardPoolNameKey);
            if (localizedString == null)
                return false;
            return localizedString.getAllTranslations(true).Any(poolName => poolName != null && Path.GetInvalidFileNameChars().Intersect(poolName).Count() > 0);
        }
        private void showCardPoolNameInvalidDialog()
        {
            var title = game.lang._(StringTable.ERROR);
            var desc = game.lang._(EDITOR_WARNING_CARD_POOL_NAME_INVALID);
            game.ui.dialog.ShowConfirm(title, desc);
        }
        #endregion

        /// <summary>
        /// 读取所有卡池文件中的卡池信息。
        /// </summary>
        /// <returns>包含卡池文件相对路径和卡池信息键值对的字典</returns>
        private void loadCardPoolInfos()
        {
            _cardPoolIdList.Clear();
            var result = game.card.getLocalCardPoolInfos();
            foreach (var cardPoolInfo in result)
            {
                var poolId = cardPoolInfo.cardPoolId;
                int index = _cardPoolIdList.FindIndex(id => id == poolId);
                if (index >= 0)
                {
                    var poolRef = getLocalCardPoolRef(poolId);
                    var conflictPath = game.card.getCardPoolPath(poolRef);
                    Debug.LogWarning($"无法加载卡池{cardPoolInfo}，因为{conflictPath}拥有与其相同的Id{poolId}");
                    continue;
                }
                _cardPoolIdList.Add(poolId);
            }
        }
        private async Task readCardPoolByPathAsync(string path)
        {
            var poolRef = game.card.getCardPoolInfo(path);
            if (!game.card.tryGetCardPool(poolRef, out var _))
            {
                await game.card.loadLocalCardPool(path, true);
            }
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
                game.lang._(StringTable.SAVE),
                game.lang._(StringTable.DONT_SAVE),
                game.lang._(StringTable.CANCEL),
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
            if (!isCurrentCardPoolUnsaved())
                return;
            if (checkDependencyCycle(null, out var cycles))
            {
                showDependencyCycleDialog(cycles);
                return;
            }
            if (checkCardPoolNameInvalid())
            {
                showCardPoolNameInvalidDialog();
                return;
            }
            bool save = true;
            if (hasObsoleteNodes(curCardPoolId, out string desc))
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                game.ui.dialog.ShowQuestion(game.langManager.getString(StringTable.WARNING), desc, () => tcs.SetResult(true), () => tcs.SetResult(false));
                save = await tcs.Task;
            }
            if (save)
            {
                try
                {
                    startWaiting();
                    await saveCardPool();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    game.ui.dialog.ShowConfirm(game.i18n._(StringTable.ERROR), e.Message);
                }
                finally
                {
                    endWaiting();
                }
            }
            onEndAction?.Invoke(save);
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
                clearCardPoolUnsaved(curCardPoolId);
                await game.editor.reloadCardPoolEnv(info);
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
                    var cardName = game.card.getCardName(cardPoolRef, nodeInfo.cardId);
                    descs.Add(cardName ?? StringTable.BUILD_UNKNOWN_CARD_NAME);
                }
                else if (nodeInfo.buffId != 0)
                {
                    string buffName = null;
                    if (game.card.tryGetBuffDefine(cardPoolRef, nodeInfo.buffId, out var buffDefine))
                    {
                        buffName = game.card.getBuffName(cardPoolRef, nodeInfo.buffId);
                    }
                    descs.Add(buffName ?? StringTable.BUFF_NAME_UNKNOWN);
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
                obsoleteString = game.lang._(StringTable.TRIMMED_COTENTS, obsoleteString);
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
            var cardPoolId = curCardPoolId;
            if (game.card.isCreatingCardPool(cardPoolId)) // 正在创建新卡池，退出时直接把创建的这个卡池删除
            {
                game.card.deleteCreatingCardPool();
                //重新加载所有卡池信息
                await game.card.loadAllCardPoolInfosAsync();
                loadCardPoolInfos();
            }
            else if (isCurrentCardPoolUnsaved()) // 未保存，退出时重新加载牌库信息，防止出现临时卡牌变更。
            {
                var cardPoolRef = game.card.getLocalCardPoolReference(cardPoolId);
                var path = game.card.getCardPoolPath(cardPoolRef);
                await game.card.loadLocalCardPoolInfoAsync(path);
                await game.card.loadLocalCardPool(path);
            }
            // 删除所有属于该卡池的页面信息。
            _displayPagePool.RemoveAll(p => p.cardPoolId == cardPoolId);
            _cardPoolInfoClones.RemoveAll(c => c.cardPoolId == cardPoolId);
            clearCardPoolUnsaved(curCardPoolId);
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
            startCardPoolListWaiting();
            await updateCardPoolListUI();
            endCardPoolListWaiting();
        }
        /// <summary>
        /// 更新卡池列表UI
        /// </summary>
        /// <returns></returns>
        private async Task updateCardPoolListUI()
        {
            DeckListItemViewData[] viewData = new DeckListItemViewData[_cardPoolIdList.Count];
            for (int i = 0; i < viewData.Length; i++)
            {
                var cardPoolId = _cardPoolIdList[i];
                var cardPoolInfo = getLocalCardPoolInfo(cardPoolId);
                var cardPoolName = game.card.getCardPoolName(cardPoolInfo);
                viewData[i] = new DeckListItemViewData()
                {
                    name = cardPoolName,
                    image = await game.card.loadCardPoolImage(cardPoolInfo),
                    mode = DeckListItem.Mode.CanDelete
                };
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
                await setCurrentCardPool(poolInfo);
                await editor.display(displayInfo);
            }
            return editor;
        }

        #region 卡池环境
        private async Task setCurrentCardPool(CardPoolInfo poolInfo)
        {
            game.editor.setCurrentCardPoolId(poolInfo.cardPoolId);
            if (!_cardPoolInfoClones.Exists(c => c.cardPoolId == poolInfo.cardPoolId))
                _cardPoolInfoClones.Add(cloneCardPoolInfo(poolInfo));
            await game.editor.reloadCardPoolEnv(poolInfo);
        }
        private void clearCurrentCardPool()
        {
            game.editor.setCurrentCardPoolId(-1);
            _cardPoolInfoClones.Clear();
            game.editor.clearCardPoolEnv();
        }
        private ICardPoolReference[] getCardPoolEnv() => game.editor.getEnvCardPoolRefs();
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
            return getEditingCardPoolInfo(curCardPoolId);
        }
        private CardPoolInfo cloneCardPoolInfo(CardPoolInfo poolInfo)
        {
            return new(poolInfo);
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
            if (!Application.isEditor && game.card.isInnateCardPool(curCardPoolId))
            {
                // 尝试保存经典卡池
                content = game.lang._(EDITOR_TRY_TO_SAVE_STANDARD_CARDPOOL);
                options = new string[2]
                {
                        game.lang._(StringTable.CONFIRM),
                        game.lang._(StringTable.CANCEL)
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
                var cardPoolName = game.card.getCardPoolName(cardPoolInfo);
                content = game.lang._(EDITOR_NEED_TO_SAVE_CARDPOOL, cardPoolName);
                if (forceSave)
                {
                    // 强制保存或不保存
                    options = new string[2]
                    {
                        game.lang._(StringTable.SAVE),
                        game.lang._(StringTable.DONT_SAVE)
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
                            game.lang._(StringTable.SAVE),
                            game.lang._(StringTable.DONT_SAVE),
                            game.lang._(StringTable.CANCEL)
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
                display(false, -1, true);
            }
        }
        /// <summary>
        /// 在退出该编辑器时，是否要保存当前卡池。
        /// </summary>
        /// <returns>是否要保存当前卡池。</returns>
        private bool needToSaveCurrentCardPoolOnEditorClose()
        {
            if (curCardPoolId == _firstOpenedCardPoolId) // 编辑的卡池是一开始打开的卡池
                return false;
            if (!isCardPoolUnsaved(curCardPoolId)) // 没有任何更改
                return false;
            if (_displayObjectStack.Count <= 0) // 当前没有正在打开的编辑器页面
                return false;
            if (_displayObjectStack.Count(obj => getDisplayPage(obj).cardPoolId == curCardPoolId) > 1) // 目前的编辑器不是栈中最后一个属于目前卡池的编辑器
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
        private void startWaiting()
        {
            scene.waitingBlocker.addWaitingTask(WaitingTask.Editor);
        }
        private void endWaiting()
        {
            scene.waitingBlocker.removeWaitingTask(WaitingTask.Editor);
        }
        private void startCardPoolListWaiting()
        {
            _ui.setCardPoolListWaiting(true);
        }
        private void endCardPoolListWaiting()
        {
            _ui.setCardPoolListWaiting(false);
        }
        private async Task updateCardPoolImageButton()
        {
            var poolInfo = getCurrentCardPoolInfo();
            var poolName = game.card.getLocalizedCardPoolName(poolInfo, _curCardPoolNameLang);
            var sprite = await game.card.loadImage(poolInfo, game.card.getLocalizedCardPoolImagePath(poolInfo, _curCardPoolImageLang));
            _ui.updateCurCardPoolInfo(poolName, sprite);
        }
        #endregion

        #region 属性字段
        [TranslateMsg("卡池编辑器/退出跨卡池编辑器/提示经典卡池被修改")]
        public const string EDITOR_TRY_TO_SAVE_STANDARD_CARDPOOL = "您对经典卡池进行了更改，但该卡池无法被修改。\n是否要离开该卡池？";
        [TranslateMsg("卡池编辑器/退出跨卡池编辑器/提示要求保存卡池（{0}是卡池名）")]
        public const string EDITOR_NEED_TO_SAVE_CARDPOOL = "您对卡池({0})进行了更改，是否要保存该卡池？";
        [TranslateMsg("编辑器/标题模板，{0}为编辑器名称，{1}为未保存标识（星号）")]
        public const string EDITOR_TITLE_TEMPLATE = "· {0}{1} ·";
        [TranslateMsg("编辑器/未保存标识（星号）")]
        public const string EDITOR_UNSAVED_MARK = "*";
        [TranslateMsg("卡池编辑器/标题")]
        public const string EDITOR_TITLE = "编辑器";
        [TranslateMsg]
        const string EXPORT_BEFORE_SAVE = "当前卡池含有未保存的修改，请在导出前保存修改";
        [TranslateMsg]
        const string EDITOR_UNKNOWN_ACTION_NAME = "未知自定义动作";
        [TranslateMsg]
        const string EDITOR_UNKNOWN_RULE_NAME = "未知自定义规则";
        [TranslateMsg("尝试保存具有过时节点的卡池时的警告。（{0}：过时节点列表）")]
        const string EDITOR_WARNING_OBSOLETE_NODES_TEMPLATE = "以下内容包含过时的节点：\n{0}\n是否保存卡池？";
        [TranslateMsg("尝试设置循环卡池依赖时的警告。（{0}：过时节点列表）")]
        const string EDITOR_WARNING_CYCLE_DEPENDENCY = "以下卡池依赖包含对该卡池的依赖，会导致卡池循环依赖：\n{0}\n请取消该依赖后再试。";
        [TranslateMsg("卡池名称非法时的警告。")]
        const string EDITOR_WARNING_CARD_POOL_NAME_INVALID = "无法保存卡池：卡池名称中包含非法字符。";
        [TranslateMsg("{0}是路径")]
        const string EXPORT_SUCCESS = "卡池已导出至 {0}";
        [TranslateMsg]
        const string IMPORT_SUCCESS = "卡池已导入";

        [TranslateMsg("卡池编辑器-创建规则选项")]
        public const string RULE_NAME_EVENT_LOGIC = "事件规则";
        [TranslateMsg("卡池编辑器-删除规则警告，{0}为规则的名字")]
        public const string EDITOR_WARNING_DELETE_RULE = "确定要删除自定义规则“{0}”？";
        [TranslateMsg("卡池编辑器-删除卡池依赖警告，{0}为卡池的名字+卡池版本")]
        public const string EDITOR_WARNING_DELETE_DEPENDENCY = "确定要删除卡池依赖“{0}”？";

        [TranslateMsg("卡池编辑器-选择卡池面板的标题")]
        public const string EDITOR_TITLE_SELECT_CARD_POOL = "选择卡池";
        public const string EDITOR_TITLE_SELECT_CARD_POOL_DEPENDENCY = "选择要添加的依赖卡池";

        public string nodeClipboard { get; set; }
        private long curCardPoolId => game.editor.currentCardPoolId;
        private EditorPanel _ui;
        private List<long> _cardPoolIdList = new List<long>();
        private DefineReference _copyingCard = null;
        private HashSet<long> _unsavedCardPools = new HashSet<long>();
        private long _firstOpenedCardPoolId;
        private List<CardPoolInfo> _cardPoolInfoClones = new List<CardPoolInfo>();
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
        private string _curCardPoolNameLang;
        private string _curCardPoolImageLang;

        #region 事件
        public EventTriggerTime[] eventTriggerTimes { get; private set; }
        public EventReference[] eventReferences { get; private set; }
        public string[] eventTriggerTimeEditorNames { get; private set; }
        public string[] eventEditorNames { get; private set; }
        #endregion

        #endregion

        #region 内嵌类
        public class CardPoolSelectSourceDependency : CardPoolSelectSource
        {
            public CardPoolSelectSourceDependency(EditorCtrl editor, IEnumerable<CardPoolReference> poolRef) : base(poolRef)
            {
                _editor = editor;
            }
            public override bool Confirm()
            {
                return _editor.setCardPoolDependencies(_selectedCardPools);
            }
            private EditorCtrl _editor;
        }
        #endregion
    }
}

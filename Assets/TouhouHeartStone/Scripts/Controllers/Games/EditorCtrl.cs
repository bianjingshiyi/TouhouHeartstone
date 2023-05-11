using Game;
using MukioI18n;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
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
                hasUnsavedChanges = false;
                //牌库浏览模式
                if (reloadCardPoolInfos)
                {
                    //重新加载所有牌库信息
                    await getCardPoolsInfoAsync();
                }
                gameObject.SetActive(true);
                _ui.setDeckListActive(true);
                _ui.setEditDeckPanelActive(false);
                await _cardPoolViewPanelCtrl.hide();
                await updateDeckListUI();
            }
            else
            {
                //牌库编辑模式
                //设置当前牌库
                if (0 <= cardPoolIndex && cardPoolIndex < _cardPoolInfoList.Count)
                {
                    _curCardPoolPath = _cardPoolPathList[cardPoolIndex];
                    _curCardPoolInfo = _cardPoolInfoList[cardPoolIndex];
                }
                //更新牌库信息
                Sprite image = await game.card.loadImage(_curCardPoolInfo.imageResPath, _curCardPoolInfo.cardPoolId);
                //显示卡牌列表
                if (reloadCardPool)
                {
                    //重新加载牌库中的所有卡牌和皮肤
                    if (string.IsNullOrEmpty(_curCardPoolPath))
                    {
                        await readCardPoolByIdAsync(_curCardPoolInfo.ToReference());
                    }
                    else
                    {
                        await readCardPoolByPathAsync(_curCardPoolPath);
                    }
                }
                //更新UI
                gameObject.SetActive(true);
                _ui.setDeckListActive(false);
                _ui.setEditDeckPanelActive(true);
                _ui.updateCurCardPoolInfo(_curCardPoolInfo.cardPoolName, image);
                await updateDeckCardListUI();
                await _cardPoolViewPanelCtrl.display(_curCardPoolInfo, cardFilter: c =>
                {
                    if (game.card.isBuiltinCard(c))
                    {
                        //不显示内置的卡牌
                        return false;
                    }
                    //显示所有卡牌，包括角色，技能和衍生物
                    return true;
                });
            }
        }
        /// <summary>
        /// 打开一个卡牌/增益/自定义动作编辑器。
        /// </summary>
        /// <param name="displayInfo"></param>
        public async Task openEditorPanel(CardPoolDisplayInfo displayInfo)
        {
            _ui.gameObject.SetActive(false);
            _currentEditor?.hide();
            _currentEditor = await displayPanel(displayInfo);
            _stack.Push(displayInfo);
        }
        public Task openCardEditor(long cardPoolId, CardDefine cardDefine, CardSkinData[] skins)
        {
            return openEditorPanel(new CardPoolDisplayInfo()
            {
                cardPoolId = cardPoolId,
                displayObj = cardDefine,
                skins = skins
            });
        }
        public Task openActionEditor(long cardPoolId, GeneratedActionDefine actionDefine)
        {
            return openEditorPanel(new CardPoolDisplayInfo()
            {
                cardPoolId = cardPoolId,
                displayObj = actionDefine
            });
        }
        public Task openBuffEditor(long cardPoolId, GeneratedBuffDefine buffDefine, BuffSkin[] skins)
        {
            return openEditorPanel(new CardPoolDisplayInfo()
            {
                cardPoolId = cardPoolId,
                skins = skins,
                displayObj = buffDefine
            });
        }
        /// <summary>
        /// 关闭当前的卡牌/增益/自定义动作编辑器。
        /// </summary>
        public async Task closeEditorPanel()
        {
            _currentEditor.hide();
            _currentEditor = null;
            if (_stack.Count > 1)
            {
                _stack.Pop();
                var displayInfo = _stack.Peek();
                _currentEditor = await displayPanel(displayInfo);
            }
            else
            {
                _stack.Clear();
                display(false, false, -1, true);
            }
        }
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
            _ui.onDeckNameInputFieldValueChanged += onDeckNameInputFieldValueChanged;
            _ui.onClickNewCardButton += onClickNewCardButton;
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
            if (hasUnsavedChanges)
            {
                tryReturn();
            }
            else
            {
                if (_curCardPoolPath == null)
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
            CardPoolInfo cardPool = _cardPoolInfoList[index];
            if (game.card.isStandardCardPool(cardPool))
            {
                game.ui.dialog.ShowConfirm(game.i18n._(StringTable.INVALID_OPERATION), game.i18n._(StringTable.EDITOR_DIALOG_CANNOT_DELETE_STANDARD_CARDPOOL), null);
            }
            else
            {
                game.ui.dialog.ShowQuestion(game.i18n._(StringTable.WARNING), game.i18n._(StringTable.EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL), async () =>
                {
                    hasUnsavedChanges = false;
                    game.card.removeCardPool(_cardPoolInfoList[index].ToReference());
                    _cardPoolPathList.RemoveAt(index);
                    _cardPoolInfoList.RemoveAt(index);
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
            hasUnsavedChanges = true;
            _curCardPoolPath = null;
            game.card.createCardPool(out _curCardPoolInfo, out _);
            _cardPoolInfoList.Add(_curCardPoolInfo);
            _cardPoolPathList.Add(null);

            display(false, false, _cardPoolInfoList.IndexOf(_curCardPoolInfo), true);
        }
        /// <summary>
        /// 当卡牌列表被点击，打开卡牌编辑器，如果是新建卡牌则新建卡牌并打开
        /// </summary>
        /// <param name="index"></param>
        private async void onClickCard(CardDefine cardDefine)
        {
            var define = game.card.cloneCardDefine(cardDefine);
            var skins = game.card.cloneCardSkins(_curCardPoolInfo, cardDefine.id);
            await openCardEditor(_curCardPoolInfo.cardPoolId, define, skins);
        }
        /// <summary>
        /// 当点击新建卡牌按钮
        /// </summary>
        private async void onClickNewCardButton()
        {
            hasUnsavedChanges = true;
            game.card.createCard(_curCardPoolInfo, out CardDefine newCardDefine, out var skin);
            game.card.addCard(_curCardPoolInfo, newCardDefine);
            game.card.addCardSkin(_curCardPoolInfo.cardPoolId, skin);

            var define = game.card.cloneCardDefine(newCardDefine);
            var skins = game.card.cloneCardSkins(_curCardPoolInfo, newCardDefine.id);
            await openCardEditor(_curCardPoolInfo.cardPoolId, define, skins);
        }
        /// <summary>
        /// 当点击保存按钮，保存当前编辑的卡池
        /// </summary>
        private void onClickSaveCardPoolButton()
        {
            trySaveCardPool();
        }

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

        /// <summary>
        /// 当点击导出按钮，导出当前编辑的卡池
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private async void onClickExportCardPoolButton()
        {
            if (hasUnsavedChanges)
            {
                game.ui.dialog.ShowConfirm(
                    game.i18n._(StringTable.HINT_TITLE),
                    game.i18n._(EXPORT_BEFORE_SAVE), null);
                return;
            }
            try
            {
                var path = await game.card.exportCardPool(_curCardPoolInfo);
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
            if (game.card.isStandardCardPool(_curCardPoolInfo))
            {
                game.ui.dialog.ShowConfirm(
                    game.i18n._(StringTable.INVALID_OPERATION),
                    game.i18n._(StringTable.EDITOR_DIALOG_CANNOT_DELETE_STANDARD_CARDPOOL), null);
            }
            else
            {
                game.ui.dialog.ShowQuestion(game.i18n._(StringTable.WARNING), game.langManager.getString(StringTable.EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL), async () =>
                {
                    hasUnsavedChanges = false;
                    //删除当前卡池
                    game.card.removeCardPool(_curCardPoolInfo.ToReference());
                    int index = _cardPoolInfoList.IndexOf(_curCardPoolInfo);
                    _cardPoolPathList.RemoveAt(index);
                    _cardPoolInfoList.RemoveAt(index);
                    await updateDeckListUI();
                    await returnToView();
                });
            }
        }
        private void onDeckNameInputFieldValueChanged(string value)
        {
            hasUnsavedChanges = true;
            _curCardPoolInfo.cardPoolName = value;
        }
        #endregion
        /// <summary>
        /// 读取所有卡池文件中的卡池信息。
        /// </summary>
        /// <returns>包含卡池文件相对路径和卡池信息键值对的字典</returns>
        private async Task getCardPoolsInfoAsync()
        {
            _cardPoolPathList.Clear();
            _cardPoolInfoList.Clear();
            var result = await game.card.loadCardPoolInfosAsync();
            foreach (var pair in result)
            {
                CardPoolInfo cardPool = pair.Value;
                int index = _cardPoolInfoList.FindIndex(p => p.CompareCardPool(cardPool));
                if (index >= 0)
                {
                    Debug.LogWarning("无法加载卡池" + pair.Key + "，因为" + _cardPoolPathList[index] + "拥有与其相同的Id" + cardPool.cardPoolId);
                    continue;
                }
                _cardPoolPathList.Add(pair.Key);
                _cardPoolInfoList.Add(cardPool);
            }
            //尝试修复没有Id的卡池
            //for (int i = 0; i < _cardPoolInfoList.Count; i++)
            //{
            //    if (_cardPoolInfoList[i].cardPoolId == 0)
            //        _cardPoolInfoList[i].cardPoolId = allocCardPoolId();
            //}
        }
        /// <summary>
        /// 读取指定路径上的卡池文件中的所有卡牌
        /// </summary>
        /// <param name="cardPoolId">卡池文件相对路径</param>
        /// <returns></returns>
        /// <exception cref="IOException">当读取文件失败的时候抛出这个异常</exception>
        private async Task readCardPoolByPathAsync(string cardPoolPath)
        {
            if (!game.card.tryGetCardPool(cardPoolPath, out _))
            {
                await game.card.loadCardPool(cardPoolPath);
            }
        }
        /// <summary>
        /// 读取指定id的卡池文件中的所有卡牌
        /// </summary>
        /// <param name="cardPoolRef">卡池引用。</param>
        /// <returns></returns>
        /// <exception cref="IOException">当读取文件失败的时候抛出这个异常</exception>
        private async Task readCardPoolByIdAsync(CardPoolReference cardPoolRef)
        {
            if (!game.card.tryGetCardPool(cardPoolRef, out _))
            {
                CardPoolInfo cardPoolInfo = game.card.getCardPoolInfo(cardPoolRef);
                string cardPoolPath = game.card.getCardPoolPath(cardPoolInfo);
                await game.card.loadCardPool(cardPoolPath);
            }
        }
        /// <summary>
        /// 尝试返回上层UI。
        /// </summary>
        /// <returns></returns>
        private void tryReturn()
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
            if (hasUnsavedChanges)
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
            bool success = await game.card.saveCardPool(_curCardPoolInfo);
            if (success)
            {
                _curCardPoolPath = game.card.getCardPoolPath(_curCardPoolInfo);
                int index = _cardPoolInfoList.IndexOf(_curCardPoolInfo);
                _cardPoolPathList[index] = _curCardPoolPath;
                hasUnsavedChanges = false;//如果已经保存则不需要再提示保存
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
            var cardPoolRef = _curCardPoolInfo.ToReference();
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
                    var actionDefine = game.card.getActionDefine(cardPoolRef.cardPoolId, actionId);
                    string actionName = actionDefine?.defineName ?? EDITOR_UNKNOWN_ACTION_NAME;
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
            // 退出时重新加载牌库信息，防止出现临时卡牌变更。
            if (hasUnsavedChanges)
            {
                var cardPoolRef = game.card.getLocalCardPoolReference(_curCardPoolInfo.cardPoolId);
                await game.card.reloadCardPoolsAndGetCardDefinesAsync(new ICardPoolReference[] { cardPoolRef });
                hasUnsavedChanges = false;
            }
            _curCardPoolPath = null;
            _curCardPoolInfo = null;
            _ui.setDeckListActive(true);
            _ui.setEditDeckPanelActive(false);
            await updateDeckListUI();
            await _cardPoolViewPanelCtrl.hide();
        }
        /// <summary>
        /// 更新卡池列表UI
        /// </summary>
        /// <returns></returns>
        private async Task updateDeckListUI()
        {
            (string name, Sprite image)[] viewData = new (string name, Sprite image)[_cardPoolInfoList.Count];
            for (int i = 0; i < viewData.Length; i++)
            {
                viewData[i].name = _cardPoolInfoList[i].cardPoolName;
                viewData[i].image = await game.card.loadImage(_cardPoolInfoList[i].imageResPath, _cardPoolInfoList[i].cardPoolId);
            }
            _ui.updateDeckList(viewData);
        }
        private async Task updateDeckCardListUI()
        {
            CardDefine[] cardDefines = game.card.getCardDefines(_curCardPoolInfo.ToReference()).Select(p => p.Value).ToArray();
            (string name, Sprite image, int count)[] viewData = new (string name, Sprite image, int count)[cardDefines.Length];
            for (int i = 0; i < viewData.Length; i++)
            {
                CardSkinData skin = game.card.getDefaultSkin(cardDefines[i]);
                viewData[i].name = skin.name;
                viewData[i].image = await game.card.loadImage(skin.imagePath, _curCardPoolInfo.cardPoolId);
                viewData[i].count = 0;
            }
            _ui.updateDeckCardList(viewData);
        }
        /// <summary>
        /// 根据卡池显示信息显示卡牌编辑器、增益编辑器等。
        /// </summary>
        /// <param name="displayInfo">卡池显示信息</param>
        /// <returns>显示的编辑器。</returns>
        private async Task<CardPoolEditor> displayPanel(CardPoolDisplayInfo displayInfo)
        {
            CardPoolEditor editor = null;
            switch (displayInfo.displayObj)
            {
                case CardDefine _:
                    editor = scene.cardEditor;
                    break;
                case GeneratedActionDefine _:
                    editor = scene.actionEditor;
                    break;
                case GeneratedBuffDefine _:
                    editor = scene.buffEditor;
                    break;
            }
            if (editor != null)
            {
                await editor.display(displayInfo);
            }
            return editor;
        }
        #endregion
        #region 属性字段
        [TranslateMsg("卡池编辑器/退出未保存卡池/选项名")]
        public const string EDITOR_OPTION_SAVE = "保存";
        [TranslateMsg("卡池编辑器/退出未保存卡池/选项名")]
        public const string EDITOR_OPTION_DONT_SAVE = "不保存";
        [TranslateMsg("卡池编辑器/退出未保存卡池/选项名")]
        public const string EDITOR_OPTION_CANCEL = "取消";


        public bool hasUnsavedChanges
        {
            get { return _hasUnsavedChanges; }
            set { _hasUnsavedChanges = value; }
        }
        public string nodeClipboard { get; set; }
        public GraphOperationMode graphOperationMode { get; set; }
        public string CurCardPoolPath
        {
            get { return _curCardPoolPath; }
            set { _curCardPoolPath = value; }
        }
        public List<string> CardPoolPathList
        {
            get { return _cardPoolPathList; }

        }

        public List<CardPoolInfo> CardPoolInfoList
        {
            get { return _cardPoolInfoList; }

        }
        public CardPoolInfo CurCardPoolInfo
        {
            get { return _curCardPoolInfo; }

        }
        private EditorPanel _ui;
        [SerializeField]
        private CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        List<string> _cardPoolPathList = new List<string>();
        List<CardPoolInfo> _cardPoolInfoList = new List<CardPoolInfo>();
        string _curCardPoolPath;
        CardPoolInfo _curCardPoolInfo;
        [SerializeField]
        bool _hasUnsavedChanges;
        private Stack<CardPoolDisplayInfo> _stack = new Stack<CardPoolDisplayInfo>();
        private CardPoolEditor _currentEditor;
        #endregion
    }
}

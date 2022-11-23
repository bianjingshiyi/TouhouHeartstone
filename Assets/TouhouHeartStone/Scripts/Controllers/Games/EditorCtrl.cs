using BJSYGameCore;
using Game;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
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
            gameObject.SetActive(true);
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
                        await readCardPoolByIdAsync(_curCardPoolInfo.cardPoolId);
                    }
                    else
                    {
                        await readCardPoolByPathAsync(_curCardPoolPath);
                    }
                }
                //更新UI
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

            _cardPoolViewPanelCtrl.onClickCard += onClickCard;
            _cardPoolViewPanelCtrl.onUpdateCardListItem += onUpdateCardListItem;
        }
        #endregion
        #region 事件回调
        /// <summary>
        /// 当用户点击返回按钮，返回主菜单
        /// </summary>
        /// <param name="ui"></param>
        private void onClickReturnButton()
        {
            if (hasUnsavedChanges)
            {
                game.ui.dialog.ShowQuestion(
                    game.langManager.getString(StringTable.WARNING),
                    game.langManager.getString(StringTable.EDITOR_WARNING_UNSAVED_CHANGES), async () =>
                    {
                        //保存卡池
                        await game.card.saveCardPool(_curCardPoolInfo);
                        //返回主菜单
                        await returnToView();
                        hasUnsavedChanges = false;
                    }, () =>
                    {
                        returnToView();
                        hasUnsavedChanges = false;
                    });
            }
            else
            {
                if (_curCardPoolPath == null)
                {
                    _ui.gameObject.SetActive(false);
                    scene.menu.display();
                }
                else {
                    returnToView();
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
                game.ui.dialog.ShowConfirm(StringTable.INVALID_OPERATION, StringTable.EDITOR_DIALOG_CANNOT_DELETE_STANDARD_CARDPOOL, null);
            }
            else
            {
                _ui.findInstance<UIDialog>().ShowQuestion(null, game.langManager.getString(StringTable.EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL), async () =>
                {
                    hasUnsavedChanges = true;
                    game.card.removeCardPool(_cardPoolInfoList[index].cardPoolId);
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
            await scene.cardEditor.display(_curCardPoolInfo.cardPoolId, cardDefine, getSelectableCharacters());
        }
        /// <summary>
        /// 当点击新建卡牌按钮
        /// </summary>
        private async void onClickNewCardButton()
        {
            hasUnsavedChanges = true;
            game.card.createCard(_curCardPoolInfo, out CardDefine newCardDefine, out _);
            await scene.cardEditor.display(_curCardPoolInfo.cardPoolId, newCardDefine, getSelectableCharacters());
        }
        /// <summary>
        /// 当点击保存按钮，保存当前编辑的卡池
        /// </summary>
        private async void onClickSaveCardPoolButton()
        {
            if (hasUnsavedChanges)
            {
                bool success = await game.card.saveCardPool(_curCardPoolInfo);
                if (success)
                {
                    _curCardPoolPath = game.card.getCardPoolPath(_curCardPoolInfo);
                    int index = _cardPoolInfoList.IndexOf(_curCardPoolInfo);
                    _cardPoolPathList[index] = _curCardPoolPath;
                    hasUnsavedChanges = false;//如果已经保存则不需要再提示保存
                }
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
                    game.langManager.getString(StringTable.INVALID_OPERATION),
                    game.langManager.getString(StringTable.EDITOR_DIALOG_CANNOT_DELETE_STANDARD_CARDPOOL), null);
            }
            else
            {
                game.ui.dialog.ShowConfirm(null, game.langManager.getString(StringTable.EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL), async () =>
                {
                    hasUnsavedChanges = true;
                    //删除当前卡池
                    game.card.removeCardPool(_curCardPoolInfo.cardPoolId);
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
                int index = _cardPoolInfoList.FindIndex(p => p.cardPoolId == cardPool.cardPoolId);
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
        /// <param name="cardPoolId">卡池id</param>
        /// <returns></returns>
        /// <exception cref="IOException">当读取文件失败的时候抛出这个异常</exception>
        private async Task readCardPoolByIdAsync(int cardPoolId)
        {
            if (!game.card.tryGetCardPool(cardPoolId, out _))
            {
                CardPoolInfo cardPoolInfo = game.card.getCardPoolInfo(cardPoolId);
                string cardPoolPath = game.card.getCardPoolPath(cardPoolInfo);
                await game.card.loadCardPool(cardPoolPath);
            }
        }
        /// <summary>
        /// 清空字段并切换到浏览状态
        /// </summary>
        private async Task returnToView()
        {
            // 退出时重新加载牌库信息，防止出现临时卡牌变更。
            if (hasUnsavedChanges)
            {
                await game.card.reloadCardPoolsAndGetCardDefinesAsync(new int[] { _curCardPoolInfo.cardPoolId});
            }
            _curCardPoolPath = null;
            _curCardPoolInfo = null;
            _ui.setDeckListActive(true);
            _ui.setEditDeckPanelActive(false);
            await updateDeckListUI();
            await _cardPoolViewPanelCtrl.hide();
           
        }
        private CardDefine[] getSelectableCharacters()
        {
            List<CardDefine> cardList = new List<CardDefine>();
            cardList.AddRange(game.card.GetCardDefines(c => c.isMaster(), true));
            return cardList.ToArray();
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
            CardDefine[] cardDefines = game.card.getCardDefines(_curCardPoolInfo.cardPoolId);
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
        #endregion
        #region 属性字段


        public bool hasUnsavedChanges
        {
            get { return _hasUnsavedChanges; }
            set { _hasUnsavedChanges = value; }
        }
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
        #endregion
    }
}

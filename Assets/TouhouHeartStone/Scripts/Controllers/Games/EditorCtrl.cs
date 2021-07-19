using BJSYGameCore;
using Game;
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
    partial class EditorCtrl : IController<MainCtrl>
    {
        #region 公有方法
        public EditorCtrl()
        {
        }
        public EditorCtrl(IAppManager app, MainCtrl main, EditorPanel compo)
        {
            this.app = app;
            this.main = main;
            _compo = compo;
            _compo.autoInit();
            _compo.onBackButtonClick += onClickBackButton;
            _compo.onClickCardPoolListItem += onClickCardPoolListItem;
            _compo.onUpdateCardListItem += onUpdateCardListItem;
            _compo.onClickCardListItem += onClickCardListItem;
            _compo.onClickDeleteCardPoolButton += onClickDeleteCardPoolButton;
            _compo.onClickAddCardPoolButton += onClickAddCardPoolButton;
            _compo.onClickSaveCardPoolButton += onClickSaveCardPoolButton;
            _compo.onClickReturnToCardPoolViewButton += onClickReturnToCardPoolViewButton;
            _compo.onClickDeleteCurCardPoolButton += onClickDeleteCurCardPoolButton;
            _compo.setSortTypes(new string[]
            {
                app.langManager.getString(StringTable.CARDVIEW_SORT_COST),
                app.langManager.getString(StringTable.CARDVIEW_SORT_COST_DESCENDING),
                app.langManager.getString(StringTable.CARDVIEW_SORT_RARE),
                app.langManager.getString(StringTable.CARDVIEW_SORT_RARE_DESCENDING)
            });
            _compo.onChangeSortTypeValue += onChangeSortTypeValue;
        }
        public async void display()
        {
            _compo.gameObject.SetActive(true);
            _compo.switchRightPanelToView();
            await getCardPoolsInfoAsync();
            _compo.updateDeckList(_cardPoolInfoList.ToArray());
            _compo.setSortType((int)_sortType);
            //TODO:角色过滤器
            _compo.setCharacterFilters(new string[]
            {
                app.langManager.getString(StringTable.CARDVIEW_CHARACTER_NEUTRAL)
            });
        }
        #endregion
        #region 私有方法
        /// <summary>
        /// 读取所有卡池文件中的卡池信息。
        /// </summary>
        /// <returns>包含卡池文件相对路径和卡池信息键值对的字典</returns>
        public async Task getCardPoolsInfoAsync()
        {
            _cardPoolPathList.Clear();
            _cardPoolInfoList.Clear();
            var files = app.fileManager.getFiles(DIRNAME, "*.ddcp", true);
            var lines = await app.fileManager.readTextFiles(files);
            for (int i = 0; i < files.Length; i++)
            {
                _cardPoolPathList.Add(files[i]);
                _cardPoolInfoList.Add(JsonHelper.fromJson<CardPoolInfo>(lines[i][0]));
            }
            ////测试用数据
            //_cardPoolPaths[files.Length] = "Builtin.ddcp";
            //_cardPoolInfos[files.Length] = new CardPoolInfo()
            //{
            //    cardPoolName = "Test",
            //    imageResPath = "Textures/Patchouli/AgniShine.jpg"
            //};
        }
        /// <summary>
        /// 读取卡池文件中的所有卡牌
        /// </summary>
        /// <param name="cardPoolPath">卡池文件相对路径</param>
        /// <returns></returns>
        /// <exception cref="IOException">当读取文件失败的时候抛出这个异常</exception>
        public async Task readCardPoolAsync(string cardPoolPath)
        {
            _cardDefineList.Clear();
            _cardSkinList.Clear();
            //if (cardPoolPath == "Builtin.ddcp")
            //{
            //    //火神之光
            //    _cardDefineList.AddRange(new CardDefine[]
            //    {
            //        new CardDefine(AgniShine.ID, DEFINE.SPELL, new Dictionary<string, object>()
            //        {
            //            { DEFINE.COST, 5 },
            //            { DEFINE.TAGS, new string[] { Patchouli.FIRE } }
            //        }, new GeneratedEffect[]
            //        {
            //            new GeneratedEffect(null, new TargetChecker[]
            //            {
            //                new TargetChecker("Character", null, "必须以角色为目标")
            //            }, new ActionNode("Damage", new ActionValueRef[]
            //            {
            //                new ActionValueRef(new ActionNode("GetTarget", new object[] { "Target" })),
            //                new ActionValueRef(new ActionNode("GetVariable", new object[] { "Card" })),
            //                new ActionValueRef(new ActionNode("GetSpellDamage", new ActionValueRef[]
            //                {
            //                    new ActionValueRef(new ActionNode("GetOwner", new ActionValueRef[]
            //                    {
            //                        new ActionValueRef(new ActionNode("GetVariable", new object[] { "Card" }))
            //                    })),
            //                    new ActionValueRef(new ActionNode("IntegerConst", new object[] { 7 }))
            //                }))
            //            }), new string[0])
            //        })
            //    });
            //    _cardSkinList.AddRange(new CardSkinData[]
            //    {
            //        new CardSkinData()
            //        {
            //            id = AgniShine.ID,
            //            skinId = 0,
            //            name = "火神之光",
            //            desc = "对目标造成7点伤害",
            //            illustrator = "fafa",
            //            imagePath = "AgniShine.jpg",
            //            language = LangManager.CN
            //        }
            //    });
            //    return;
            //}
            var lines = await app.fileManager.readTextFile(DIRNAME + "/" + cardPoolPath, 1, 3);
            if (lines.Length > 0)
            {
                //卡牌
                string json = lines[0];
                _cardDefineList.AddRange(JsonHelper.fromJson<CardDefine[]>(json));
                //皮肤
                json = lines[1];
                _cardSkinList.AddRange(JsonHelper.fromJson<CardSkinData[]>(json));
                //编辑器信息
                json = lines[2];
                actionNodeGraphInfoList.Clear();
                actionNodeGraphInfoList.AddRange(JsonHelper.fromJson<ActionNodeGraphInfo[]>(json));
            }
            else
                throw new IOException("无法读取文件" + cardPoolPath);
        }
        /// <summary>
        /// 将卡池保存到相对路径下
        /// </summary>
        /// <param name="cardPoolPath">相对路径</param>
        /// <param name="info">卡池信息</param>
        /// <param name="cardDefines">卡池包含的卡片</param>
        /// <returns></returns>
        public Task saveCardPoolAsync(string cardPoolPath, CardPoolInfo info, CardDefine[] cardDefines)
        {
            return app.fileManager.saveFile(DIRNAME + "/" + cardPoolPath, new string[]
            {
                info.toJson(),
                cardDefines.toJson()
            });
        }
        /// <summary>
        /// 获取卡牌定义对应的皮肤数据
        /// </summary>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        private CardSkinData getCardSkinData(CardDefine cardDefine)
        {
            var cardSkinGroup = _cardSkinList.GroupBy(s => s.id).First(g => g.Key == cardDefine.id);
            CardSkinData cardSkin = cardSkinGroup.FirstOrDefault(s => s.language == app.langManager.getLanguage());
            if (cardSkin == null)
                cardSkin = cardSkinGroup.FirstOrDefault();
            return cardSkin;
        }
        /// <summary>
        /// 清空字段并切换到浏览状态
        /// </summary>
        private void returnToView()
        {
            _curCardPoolPath = null;
            _curCardPoolInfo = null;
            _cardDefineList.Clear();
            _cardSkinList.Clear();
            actionNodeGraphInfoList.Clear();
            _compo.switchRightPanelToView();
        }
        #region UI回调
        /// <summary>
        /// 当用户点击返回按钮，返回主菜单
        /// </summary>
        /// <param name="ui"></param>
        void onClickBackButton(EditorPanel ui)
        {
            _compo.gameObject.SetActive(false);
            Main main = ui.GetComponentInParent<Main>();
            main.display(main.MainMenu);
        }
        /// <summary>
        /// 当用户点击卡池列表项，切换右侧栏到编辑卡牌状态，读取对应文件并显示卡牌列表
        /// </summary>
        /// <param name="index"></param>
        async void onClickCardPoolListItem(int index)
        {
            _curCardPoolPath = _cardPoolPathList[index];
            _curCardPoolInfo = _cardPoolInfoList[index];
            //切换到编辑状态
            _compo.switchRightPanelToEdit();
            //更新牌库信息
            Sprite image = await app.resourceManager.loadAsync<Sprite>(_curCardPoolInfo.imageResPath, Path.GetDirectoryName(_curCardPoolPath));
            _compo.updateCurCardPoolInfo(_curCardPoolInfo.cardPoolName, image);
            //显示卡牌列表
            await readCardPoolAsync(_curCardPoolPath);
            CardSkinData[] cardSkins = _cardDefineList.Select(d => getCardSkinData(d)).ToArray();
            string[] cardNames = new string[cardSkins.Length];
            for (int i = 0; i < cardNames.Length; i++)
            {
                cardNames[i] = cardSkins[i].name;
            }
            Sprite[] cardImages = new Sprite[cardSkins.Length];
            for (int i = 0; i < cardImages.Length; i++)
            {
                cardImages[i] = await app.resourceManager.loadAsync<Sprite>(cardSkins[i].imagePath, Path.GetDirectoryName(_curCardPoolPath));
            }
            _compo.updateCardPoolCardList(cardNames, cardImages);
            //+1是为了显示新建卡牌按钮
            _compo.updateCardListCount(_cardDefineList.Count + 1);
        }
        /// <summary>
        /// 当用户点击删除卡池按钮，弹出对话框询问用户是否确定删除卡池，确定则删除并刷新卡池列表
        /// </summary>
        /// <param name="index"></param>
        void onClickDeleteCardPoolButton(int index)
        {
            _compo.findInstance<UIDialog>().ShowQuestion(null, app.langManager.getString(StringTable.EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL), () =>
            {
                app.fileManager.deleteFile(_cardPoolPathList[index]);
                _cardPoolPathList.RemoveAt(index);
                _cardPoolInfoList.RemoveAt(index);
                _compo.updateDeckList(_cardPoolInfoList.ToArray());
            });
        }
        /// <summary>
        /// 当卡牌列表需要更新显示，更新对应物体的显示
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        void onUpdateCardListItem(int index, RectTransform item)
        {
            EditorCardListItem cardListItem = item.GetComponentInChildren<EditorCardListItem>();
            if (index < _cardDefineList.Count)
            {
                //显示卡牌
                CardDefine cardDefine = _cardDefineList[index];
                CardSkinData cardSkin = getCardSkinData(cardDefine);
                cardListItem.card.setCard(cardDefine, cardSkin);
            }
            else
            {
                //新建卡牌按钮
                cardListItem.switchToAddButton();
            }
        }
        /// <summary>
        /// 当点击新增卡池按钮，切换右侧栏到编辑卡池状态
        /// </summary>
        void onClickAddCardPoolButton()
        {
            _compo.switchRightPanelToEdit();
            _curCardPoolPath = null;
            _cardDefineList.Clear();
            _cardSkinList.Clear();
            _compo.updateCardPoolCardList(new string[0], new Sprite[0]);
            //1是为了显示新建卡牌按钮
            _compo.updateCardListCount(1);
        }
        /// <summary>
        /// 当卡牌列表被点击，打开卡牌编辑器，如果是新建卡牌则新建卡牌并打开
        /// </summary>
        /// <param name="index"></param>
        void onClickCardListItem(int index)
        {
            if (index < _cardDefineList.Count)
            {
                CardDefine cardDefine = _cardDefineList[index];
                CardSkinData[] cardSkins = _cardSkinList.Where(s => s.id == cardDefine.id).ToArray();
                main.lobbySceneCtrl.cardEditorCtrl.displayCard(cardDefine, cardSkins);
            }
            else
            {

            }
        }
        void onChangeSortTypeValue(int index)
        {
            _sortType = (CardSortType)index;
        }
        /// <summary>
        /// 当点击保存按钮，保存当前编辑的卡池
        /// </summary>
        async void onClickSaveCardPoolButton()
        {
            if (string.IsNullOrEmpty(_curCardPoolPath))
            {
                //没有指定地址
            }
            else
            {
            }
            await app.fileManager.saveFile(_curCardPoolPath, new string[]
                {
                    _curCardPoolInfo.toJson(),//文件头信息
                    _cardDefineList.ToArray().toJson(),//卡牌
                    _cardSkinList.ToArray().toJson(),//皮肤
                    actionNodeGraphInfoList.ToArray().toJson()//编辑器信息
                });
        }
        /// <summary>
        /// 当点击返回浏览卡池按钮，清空字段并切换回浏览卡池状态
        /// </summary>
        void onClickReturnToCardPoolViewButton()
        {
            returnToView();
        }
        /// <summary>
        /// 当点击删除当前卡池按钮，弹出对话框询问，如果确认则删除当前卡池并切换到浏览状态
        /// </summary>
        void onClickDeleteCurCardPoolButton()
        {
            _compo.findInstance<UIDialog>().ShowConfirm(null, app.langManager.getString(StringTable.EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL), () =>
            {
                if (!string.IsNullOrEmpty(_curCardPoolPath))
                {
                    //删除当前卡池
                    app.fileManager.deleteFile(_curCardPoolPath);
                    int index = _cardPoolPathList.IndexOf(_curCardPoolPath);
                    _cardPoolPathList.RemoveAt(index);
                    _cardPoolInfoList.RemoveAt(index);
                    _compo.updateDeckList(_cardPoolInfoList.ToArray());
                }
                else
                {
                    //当前卡池还没存过，不用删
                }
                returnToView();
            });
        }
        #endregion
        #endregion
        public IAppManager app { get; }
        public MainCtrl main { get; }
        public List<ActionNodeGraphInfo> actionNodeGraphInfoList { get; } = new List<ActionNodeGraphInfo>();
        EditorPanel _compo;
        List<string> _cardPoolPathList = new List<string>();
        List<CardPoolInfo> _cardPoolInfoList = new List<CardPoolInfo>();
        string _curCardPoolPath;
        CardPoolInfo _curCardPoolInfo;
        readonly List<CardDefine> _cardDefineList = new List<CardDefine>();
        readonly List<CardSkinData> _cardSkinList = new List<CardSkinData>();
        CardSortType _sortType;
        int[] _masterIds;
        int _masterFilterIndex;
        /// <summary>
        /// 卡牌排序类型
        /// </summary>
        enum CardSortType
        {
            /// <summary>
            /// 费用升序
            /// </summary>
            cost,
            /// <summary>
            /// 降序
            /// </summary>
            costDescending,
            /// <summary>
            /// 稀有度升序
            /// </summary>
            rare,
            /// <summary>
            /// 稀有度降序
            /// </summary>
            rareDescending
        }
        const string DIRNAME = "Cards";
    }
}

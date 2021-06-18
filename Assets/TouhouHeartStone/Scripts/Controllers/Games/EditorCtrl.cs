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
    public partial class EditorCtrl : IController<MainCtrl>
    {
        #region 公有方法
        public EditorCtrl(IAppManager app, MainCtrl main, EditorPanel compo)
        {
            this.app = app;
            this.main = main;
            _compo = compo;
            _compo.onBackButtonClick += onClickBackButton;
            _compo.backButtonBlackIcon.asButton.onClick.AddListener(() => uiModeSwitch(false));
            _compo.onClickCardPoolListItem += onClickCardPoolListItem;
        }
        public async void display()
        {
            await getCardPoolsInfoAsync();
            _compo.displayCardPoolList();
            _compo.updateCardPoolList(_cardPoolInfos);
        }
        #endregion
        #region 私有方法
        /// <summary>
        /// 读取所有卡池文件中的卡池信息。
        /// </summary>
        /// <returns>包含卡池文件相对路径和卡池信息键值对的字典</returns>
        public async Task getCardPoolsInfoAsync()
        {
            var files = app.fileManager.getFiles(DIRNAME, "*.ddcp", true);
            var lines = await app.fileManager.readTextFiles(files);
            _cardPoolPaths = new string[files.Length + 1];
            _cardPoolInfos = new CardPoolInfo[files.Length + 1];
            for (int i = 0; i < files.Length; i++)
            {
                _cardPoolPaths[i] = files[i];
                _cardPoolInfos[i] = JsonHelper.fromJson<CardPoolInfo>(lines[i][0]);
            }
            //测试用数据
            _cardPoolPaths[files.Length] = "Builtin.ddcp";
            _cardPoolInfos[files.Length] = new CardPoolInfo()
            {
                cardPoolName = "Test",
                imageResPath = "Textures/Patchouli/AgniShine.jpg"
            };
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
            if (cardPoolPath == "Builtin.ddcp")
            {
                //火神之光
                _cardDefineList.AddRange(new CardDefine[]
                {
                    new CardDefine(AgniShine.ID, DEFINE.SPELL, new Dictionary<string, object>()
                    {
                        { DEFINE.COST, 5 },
                        { DEFINE.TAGS, new string[] { Patchouli.FIRE } }
                    }, new GeneratedEffect[]
                    {
                        new GeneratedEffect(null, new TargetChecker[]
                        {
                            new TargetChecker("Character", null, "必须以角色为目标")
                        }, new ActionNode("Damage", new ActionValueRef[]
                        {
                            new ActionValueRef(new ActionNode("GetTarget", new object[] { "Target" })),
                            new ActionValueRef(new ActionNode("GetVariable", new object[] { "Card" })),
                            new ActionValueRef(new ActionNode("GetSpellDamage", new ActionValueRef[]
                            {
                                new ActionValueRef(new ActionNode("GetOwner", new ActionValueRef[]
                                {
                                    new ActionValueRef(new ActionNode("GetVariable", new object[] { "Card" }))
                                })),
                                new ActionValueRef(new ActionNode("IntegerConst", new object[] { 7 }))
                            }))
                        }), new string[0])
                    })
                });
                _cardSkinList.AddRange(new CardSkinData[]
                {
                    new CardSkinData()
                    {
                        id = AgniShine.ID,
                        skinId = 0,
                        name = "火神之光",
                        desc = "对目标造成7点伤害",
                        illustrator = "fafa",
                        imagePath = "AgniShine.jpg",
                        language = LangManager.CN
                    }
                });
            }
            var lines = await app.fileManager.readTextFile(DIRNAME + "/" + cardPoolPath, 1, 2);
            if (lines.Length > 0)
            {
                //卡牌
                string json = lines[0];
                _cardDefineList.AddRange(JsonHelper.fromJson<CardDefine[]>(json));
                //皮肤
                json = lines[1];
                _cardSkinList.AddRange(JsonHelper.fromJson<CardSkinData[]>(json));
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
        #region UI回调
        /// <summary>
        /// 当用户点击返回按钮，返回主菜单
        /// </summary>
        /// <param name="ui"></param>
        void onClickBackButton(EditorPanel ui)
        {
            Main main = ui.GetComponentInParent<Main>();
            main.display(main.MainMenu);
        }
        /// <summary>
        /// 当用户点击卡池列表项，读取对应文件并显示卡牌列表
        /// </summary>
        /// <param name="index"></param>
        async void onClickCardPoolListItem(int index)
        {
            _curCardPoolPath = _cardPoolPaths[index];
            await readCardPoolAsync(_curCardPoolPath);
            _compo.updateCardListCount(_cardDefineList.Count);
        }
        /// <summary>
        /// 当卡牌列表需要更新显示，更新对应物体的显示
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        void onUpdateCardListItem(int index, RectTransform item)
        {
            //TODO:更新列表项
        }
        #endregion
        #endregion
        string[] _cardPoolPaths;
        CardPoolInfo[] _cardPoolInfos;
        string _curCardPoolPath;
        readonly List<CardDefine> _cardDefineList = new List<CardDefine>();
        readonly List<CardSkinData> _cardSkinList = new List<CardSkinData>();

        CardManager cm => _compo.ui.getManager<CardManager>();
        THHResourceManager TRm => _compo.ui.getManager<THHResourceManager>();

        string CurrentKey;

        static List<CardDefine> CurrentCardPoolList = new List<CardDefine>();
        public void onStart()
        {
            _compo.autoInit();
            uiModeSwitch(false);
        }

        /// <summary>
        /// 更新右侧卡池表列
        /// </summary>
        /// <param name="now">更新后的卡池数据</param>
        private async void UpdateDeckList(Dictionary<string, CardPoolInfo> now)
        {
            //_cardPoolsInfoDict = now;
            //_compo.fileList.clearItems();
            //foreach (var i in now.Keys)
            //{
            //    DeckListItem item = _compo.fileList.addItem();
            //    Texture2D texture = await TRm.loadTexture(DIRNAME + "/" + now[i].imageResPath, null);
            //    item.SetContent(i, now[i].cardPoolName, Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100));
            //    item.onFileRemove += OnFileItemRemove;
            //    item.onFileSelect += editorEnterEditMode;
            //}
        }

        void uiModeSwitch(bool deckMode)
        {
            _compo.cardPoolEditViewImage.gameObject.SetActive(deckMode);
            _compo.cardPoolListViewImage.gameObject.SetActive(!deckMode);

            isInDeckEditMode = !deckMode;

            // 退出组卡时清理
            if (!deckMode)
            {
                CurrentCardPoolList.Clear();
                displayedCardList = new BuildCardViewData[0];
                onSortChange();
            }
        }

        /// <summary>
        /// 更新编辑模式下右侧卡片表列内容
        /// </summary>
        /// <param name="CardList"></param>
        private void UpdateUICardPoolList()
        {
            _compo.buildDeckList.clearItems();
            foreach (var card in CurrentCardPoolList)
            {
                var item = _compo.buildDeckList.addItem();
                var skin = cm.getSkin(card.defineNumber);
                item.update(card, skin, CurrentCardPoolList.Count);
                item.DraggingAble = false;
            }
            _compo.buildDeckList.sortItems((a, b) =>
            {
                if (a.card.getCost() != b.card.getCost())
                    return a.card.getCost() - b.card.getCost();
                else
                    return a.card.defineNumber - b.card.defineNumber;
            });
        }

        private async void InitCardPoolList(string key)
        {
            //Texture2D texture = await TRm.loadTexture(DIRNAME + "/" + _cardPoolsInfoDict[key].imageResPath, null);
            //_compo.headImage.sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
            //_compo.text.text = _cardPoolsInfoDict[key].cardPoolName;
        }

        private void LoadAllViewCard()
        {
            List<BuildCardViewData> list = new List<BuildCardViewData>();
            foreach (var card in CurrentCardPoolList)
            {
                if (cm.tryGetSkin(card.defineNumber, out var skin))
                {
                    //if (_compo.filterPanel.cardFilter(card, skin))
                    //{
                    list.Add(new BuildCardViewData()
                    {
                        define = card,
                        skin = skin,
                        useCount = 0,
                        maxCount = card.hasKeyword(KEYWORD.UNIQUE) ? 1 : 2
                    });
                    //}
                }
                else
                    UberDebug.LogErrorChannel("UI", "无法找到" + card + "的皮肤");
            }

            displayedCardList = list.ToArray();
            onSortChange();
        }

        /// <summary>
        /// 比较方法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        int sortCompareMethod(BuildCardViewData a, BuildCardViewData b)
        {
            return _compo.filterPanel.sortCompareMethod(a.define, b.define);
        }

        /// <summary>
        /// 修改了排序
        /// </summary>
        private void onSortChange()
        {
            Array.Sort(displayedCardList, sortCompareMethod);
            _compo.buildCardList.SetList(displayedCardList.ToArray());
        }

        #region UI回调
        private async void OnFileItemRemove(string key)
        {
            //这里需要一个删除对应文件方法

            //UpdateDeckList(await getCardPoolsInfoAsync());
        }

        private async void editorEnterEditMode(string key)
        {
            //CurrentKey = key;
            //CurrentCardPoolList.Clear();
            //CurrentCardPoolList.AddRange(await readCardPoolAsync(CurrentKey));
            //UpdateUICardPoolList();
            //uiModeSwitch(true);
            //InitCardPoolList(key);
            //LoadAllViewCard();
        }
        #endregion
        public IAppManager app { get; }
        public MainCtrl main { get; }
        EditorPanel _compo;
        private bool isInDeckEditMode;
        private BuildCardViewData[] displayedCardList = new BuildCardViewData[0];
        const string DIRNAME = "Cards";
    }
}

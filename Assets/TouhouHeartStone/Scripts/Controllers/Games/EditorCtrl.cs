using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UI;
using BJSYGameCore;
using Game;
using UnityEngine;
using System.Linq;
using System;

namespace Controllers
{
    public partial class EditorCtrl : IController<MainCtrl>
    {
        CardManager cm => _compo.ui.getManager<CardManager>();
        THHResourceManager TRm => _compo.ui.getManager<THHResourceManager>();

        string CurrentKey;

        static List<CardDefine> CurrentCardPoolList = new List<CardDefine>();

        static Dictionary<string, CardPoolInfo> CurrentCardPool;

        public EditorCtrl(IAppManager app, MainCtrl main, EditorPanel compo)
        {
            this.app = app;
            this.main = main;
            _compo = compo;
            _compo.onAckButtonClick += onClickBackButton;
            _compo.backButtonBlackIcon.asButton.onClick.AddListener(() => uiModeSwitch(false));
        }
        public async void onStart()
        {
            _compo.autoInit();
            uiModeSwitch(false);
            UpdateDeckList(await getCardPoolsInfoAsync());
        }
        /// <summary>
        /// 读取所有卡池文件中的卡池信息。
        /// </summary>
        /// <returns>包含卡池文件相对路径和卡池信息键值对的字典</returns>
        public async Task<Dictionary<string, CardPoolInfo>> getCardPoolsInfoAsync()
        {
            Dictionary<string, CardPoolInfo> dict = new Dictionary<string, CardPoolInfo>();
            //测试用数据
            dict.Add("Builtin.ddcp", new CardPoolInfo()
            {
                cardPoolName = "Test",
                imageResPath = "Textures/Patchouli/AgniShine.jpg"
            });
            var files = app.fileManager.getFiles(DIRNAME, "*.ddcp", true);
            var lines = await app.fileManager.readTextFiles(files);
            for (int i = 0; i < files.Length; i++)
            {
                dict.Add(files[i], JsonHelper.fromJson<CardPoolInfo>(lines[i][0]));
            }
            return dict;
        }
        /// <summary>
        /// 读取卡池文件中的所有卡牌
        /// </summary>
        /// <param name="cardPoolPath">卡池文件相对路径</param>
        /// <returns></returns>
        /// <exception cref="IOException">当读取文件失败的时候抛出这个异常</exception>
        public async Task<CardDefine[]> readCardPoolAsync(string cardPoolPath)
        {
            if (cardPoolPath == "Builtin.ddcp")
            {
                return new CardDefine[]
                {
                    //火神之光
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
                };
            }
            var lines = await app.fileManager.readTextFile(DIRNAME + "/" + cardPoolPath, 1);
            if (lines.Length > 0)
            {
                string json = lines[0];
                var cardDefines = JsonHelper.fromJson<CardDefine[]>(json);
                return cardDefines;
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
        /// 更新右侧卡池表列
        /// </summary>
        /// <param name="now">更新后的卡池数据</param>
        private async void UpdateDeckList(Dictionary<string, CardPoolInfo> now)
        {
            CurrentCardPool = now;
            _compo.fileList.clearItems();
            foreach (var i in now.Keys)
            {
                DeckListItem item = _compo.fileList.addItem();
                Texture2D texture = await TRm.loadTexture(DIRNAME + "/" + now[i].imageResPath, null);
                item.SetContent(i, now[i].cardPoolName, Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100));
                item.onFileRemove += OnFileItemRemove;
                item.onFileSelect += editorEnterEditMode;
            }
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
            Texture2D texture = await TRm.loadTexture(DIRNAME + "/" + CurrentCardPool[key].imageResPath, null);
            _compo.headImage.sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
            _compo.text.text = CurrentCardPool[key].cardPoolName;
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
            return _compo.filterPanel.sortCompareMethod(a.define,b.define);
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
        void onClickBackButton(EditorPanel ui)
        {
            Main main = ui.GetComponentInParent<Main>();
            main.display(main.MainMenu);
        }
        private async void OnFileItemRemove(string key)
        {
            //这里需要一个删除对应文件方法

            UpdateDeckList(await getCardPoolsInfoAsync());
        }

        private async void editorEnterEditMode(string key)
        {
            CurrentKey = key;
            CurrentCardPoolList.Clear();
            CurrentCardPoolList.AddRange(await readCardPoolAsync(CurrentKey));
            UpdateUICardPoolList();
            uiModeSwitch(true);
            InitCardPoolList(key);
            LoadAllViewCard();
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

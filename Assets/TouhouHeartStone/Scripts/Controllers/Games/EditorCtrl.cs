using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UI;
using BJSYGameCore;

namespace Controllers
{
    public partial class CardEditorCtrl : IController<MainCtrl>
    {
        public CardEditorCtrl(IAppManager app, MainCtrl main, CardEditorPanel compo)
        {
            this.app = app;
            this.main = main;
            _compo = compo;
        }
        /// <summary>
        /// 打开一张卡片进行编辑
        /// </summary>
        public void displayCard(CardDefine cardDefine, CardSkinData cardSkin)
        {
        }
        public IAppManager app { get; }
        public MainCtrl main { get; }
        CardEditorPanel _compo;
    }
    public partial class EditorCtrl : IController<MainCtrl>
    {
        public EditorCtrl(IAppManager app, MainCtrl main, EditorPanel compo)
        {
            this.app = app;
            this.main = main;
            _compo = compo;
            _compo.onAckButtonClick += onClickBackButton;
        }
        public async void onStart()
        {
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
        private void UpdateDeckList(Dictionary<string, CardPoolInfo> now)
        {
            _compo.fileList.clearItems();
            foreach (var i in now.Values)
            {
                FileItem item = _compo.fileList.addItem();
                item.nameText.text = i.cardPoolName;
                //item.iconImage.sprite = ;
                item.dateText.text = null;
                item.onDeckRemove += OnFileItemRemove;
            }
        }
        #region UI回调
        void onClickBackButton(EditorPanel ui)
        {
            Main main = ui.GetComponentInParent<Main>();
            main.display(main.MainMenu);
        }
        private async void OnFileItemRemove(int id)
        {
            //这里需要一个删除对应文件方法
            UpdateDeckList(await getCardPoolsInfoAsync());
        }
        #endregion
        public IAppManager app { get; }
        public MainCtrl main { get; }
        EditorPanel _compo;
        const string DIRNAME = "Cards";
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UI;

namespace Controllers
{
    partial class EditorCtrl : IController<MainCtrl>
    {
        /// <summary>
        /// 读取所有卡池文件中的卡池信息。
        /// </summary>
        /// <returns>包含卡池文件相对路径和卡池信息键值对的字典</returns>
        public Task<Dictionary<string, CardPoolInfo>> getCardPoolsInfoAsync()
        {
            Dictionary<string, CardPoolInfo> dict = new Dictionary<string, CardPoolInfo>();
            //TODO:在实现卡池保存为JSON文件后接入文件读取。
            //现在先提供一些测试用的数据。
            dict.Add("Builtin.ddcp", new CardPoolInfo()
            {
                cardPoolName = "Test",
                imageResPath = "Textures/Patchouli/AgniShine.jpg"
            });
            return Task.FromResult(dict);
        }
        /// <summary>
        /// 读取卡池文件中的所有卡牌
        /// </summary>
        /// <param name="cardPoolPath">卡池文件相对路径</param>
        /// <returns></returns>
        public Task<CardDefine[]> readCardPoolAsync(string cardPoolPath)
        {
            if (cardPoolPath == "Builtin.ddcp")
            {
                return Task.FromResult(new CardDefine[]
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
                });
            }
            throw new NotImplementedException();
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
        public IAppManager app { get; }
        public MainCtrl main { get; }
        const string DIRNAME = "Cards";
    }
}

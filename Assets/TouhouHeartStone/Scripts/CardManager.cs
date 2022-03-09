using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TouhouCardEngine;
using BJSYGameCore;
using UI;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using IGensoukyo.Utilities;
using TouhouHeartstone;
using MongoDB.Bson;
using ILogger = TouhouCardEngine.Shared.ILogger;
using IGensoukyo;
using System.Text.RegularExpressions;
using Controllers;

namespace Game
{
    public partial class CardManager : Manager
    {
        #region 生命周期
        protected override void onAwake()
        {
            base.onAwake();
            onAwake_CardPool();
        }
        protected void Reset()
        {
            _game = TransformHelper.findComponentInScene<MainManager>(gameObject.scene);
        }
        #endregion
        [SerializeField]
        string _defaultImagePath = "Textures/DrBoom.png";
        [SerializeField]
        Sprite _defaultImage;
        /// <summary>
        /// 获取默认卡片贴图
        /// </summary>
        /// <returns></returns>
        public async Task<Sprite> getDefaultSprite()
        {
            if (_defaultImage == null)
            {
                Texture2D texture = await getManager<THHResourceManager>().loadTexture(_defaultImagePath, null);
                _defaultImage = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
            }
            return _defaultImage;
        }
        public void setDefaultImagePath(string path)
        {
            _defaultImagePath = path;
        }
        [SerializeField]
        CardSkinData[] _skins = new CardSkinData[0];
        Dictionary<int, CardSkinData> skinDic { get; } = new Dictionary<int, CardSkinData>();
        /// <summary>
        /// 加载配置路径中的所有卡片和皮肤。
        /// </summary>
        /// <returns></returns>
        public Task loadBuiltinCards(Assembly[] assemblies = default, string[] namespaces = null, bool builtinOnly = false, ILogger logger = null, StageLogger screenLogger = null, bool loadResources = true)
        {
            return load(builtinOnly ? new string[0] : _externalCardPaths, assemblies, namespaces, logger, screenLogger, loadResources);
        }
        /// <summary>
        /// 从给出的路径中加载卡片和皮肤，支持通配符，比如“Cards/*.xls”
        /// </summary>
        /// <param name="excelPaths"></param>
        /// <returns></returns>
        public async Task load(string[] excelPaths, Assembly[] assemblies = default, string[] namespaces = null, ILogger logger = null, StageLogger screenLogger = null, bool loadResources = true)
        {
            //加载内置卡片
            screenLogger?.NextStage("加载内置卡片");
            if (assemblies == default)
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<CardDefine> cardList = new List<CardDefine>(CardHelper.getCardDefines(assemblies, namespaces, logger));
            foreach (CardDefine card in cardList)
            {
                if (defineDic.ContainsKey(card.id))
                {
                    Debug.LogWarning("存在重复ID的卡片" + defineDic[card.id] + "和" + card);
                    continue;
                }
                defineDic.Add(card.id, card);
            }
            cardList.Clear();
            //加载外置卡片
            List<string> pathList = new List<string>();
            if (excelPaths != null)
            {
                foreach (var excelPath in excelPaths)
                {
                    // 判断文件夹
                    if (excelPath.Contains("*") || excelPath.Contains("?"))
                    {
                        if (!PlatformCompability.Current.RequireWebRequest)
                        {
                            var pathes = THHResourceManager.GetDirectoryFiles(excelPath);
                            // 去重
                            foreach (var item in pathes)
                            {
                                if (!pathList.Contains(item)) pathList.Add(item);
                            }
                        }
                        else
                        {
                            // 移动端无法直接读取文件，只能读取文件列表了
                            var regexp = excelPath.Replace("*", ".+").Replace("?", ".+");
                            foreach (var item in excelList?.FileList)
                            {
                                if (Regex.Match(item, regexp).Success)
                                {
                                    if (!pathList.Contains(item)) pathList.Add(item);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!pathList.Contains(excelPath)) pathList.Add(excelPath);
                    }
                }
            }
            screenLogger?.NextStage("加载卡片集", pathList.Count);
            int cardCount = 0;
            foreach (string path in pathList)
            {
                try
                {
                    screenLogger?.NextSub("加载卡片集：" + Path.GetFileNameWithoutExtension(path));

                    CardDefine[] cards = await loadCards(path);
                    foreach (var card in cards)
                    {
                        CardDefine older = defineDic.FirstOrDefault(p => p.Value.id == card.id).Value;
                        if (older != null)
                        {
                            older.merge(card);
                            UberDebug.LogDebugChannel("Load", "数据覆盖：" + card.ToJson());
                        }
                        else
                            cardList.Add(card);
                    }
                    cardCount += cards.Length;
                }
                catch (Exception e)
                {
                    UberDebug.LogError("读取外部卡牌文件" + path + "失败，发生异常：" + e);
                }
            }
            foreach (CardDefine card in cardList)
            {
                if (defineDic.ContainsKey(card.id))
                {
                    Debug.LogWarning("存在重复ID的卡片" + defineDic[card.id] + "和" + card);
                    continue;
                }
                defineDic.Add(card.id, card);
            }
            cardList.Clear();
            //加载内置皮肤
            skinDic.Clear();
            foreach (CardSkinData skin in _skins)
            {
                if (skin.image == null)
                    skin.image = await getDefaultSprite();
                skinDic[skin.id] = skin;
            }
            //加载外置皮肤
            screenLogger?.NextStage("加载外置皮肤", cardCount);
            foreach (string path in pathList)
            {
                try
                {
                    CardSkinData[] skins = await loadSkins(path, logger: screenLogger, loadResources: loadResources);
                    foreach (var skin in skins)
                    {
                        if (skinDic.ContainsKey(skin.id))
                            Debug.LogWarning("存在重复ID的皮肤" + skinDic[skin.id] + "和" + skin);
                        skinDic[skin.id] = skin;
                    }
                }
                catch (Exception e)
                {
                    UberDebug.LogError("读取外部皮肤文件" + path + "失败，发生异常：" + e);
                }
            }
        }
        /// <summary>
        /// 加载文件中的卡片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<CardDefine[]> loadCards(string path, PlatformCompability platform = null)
        {
            platform = platform ?? PlatformCompability.Current;

            DataSet dataset;
            try
            {
                dataset = platform.SupportExcelReading ?
                await getManager<THHResourceManager>().loadExcelAsDataSet(path, platform) :
                await getManager<THHResourceManager>().loadDataSet(path, platform);
            }
            catch (Exception e)
            {
                UberDebug.LogErrorChannel("Card", "读取" + path + "发生异常：" + e);
                return new CardDefine[0];
            }

            if (dataset.Tables.Count < 1) throw new Exception("空数据集");
            var table = dataset.Tables[0];
            List<CardDefine> cards = new List<CardDefine>();

            for (int i = 1; i < table.Rows.Count; i++)
            {
                try
                {
                    var datarow = table.Rows[i];

                    if (datarow.ReadCol("Ignore", false))
                    {
                        int id = datarow.ReadCol<int>("ID");
                        if (defineDic.ContainsKey(id))
                        {
                            Debug.Log("忽略卡牌" + defineDic[id]);
                            defineDic.Remove(id);
                        }
                        continue;
                    }

                    var card = new CardDefine(datarow.ReadCol<int>("ID"), datarow.ReadCol<string>("Type"));
                    card.setProp(nameof(ServantCardDefine.cost), datarow.ReadCol("Cost", 0));
                    card.setProp(DEFINE.ATTACK, datarow.ReadCol("Attack", 0));
                    card.setProp(nameof(ServantCardDefine.life), datarow.ReadCol("Life", 0));
                    card.setProp(DEFINE.TAGS, datarow.ReadCol("Tags", "").Split(','));
                    card.setProp(DEFINE.KEYWORDS, datarow.ReadCol("Keywords", "").Split(','));
                    card.setProp(DEFINE.ISTOKEN, datarow.ReadCol("IsToken", false));
                    card.setProp(nameof(ServantCardDefine.isActive), datarow.ReadCol("IsUnavailable", false));

                    cards.Add(card);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error when loading {path} (row {i}): " + e);
                }
            }

            return cards.ToArray();
        }
        /// <summary>
        /// 加载文件中的皮肤
        /// </summary>
        /// <param name="path"></param>
        /// <param name="platform">当前平台</param>
        /// <param name="logger">提示用Logger</param>
        /// <returns></returns>
        public async Task<CardSkinData[]> loadSkins(string path, PlatformCompability platform = null, StageLogger logger = null, bool loadResources = true)
        {
            platform = platform ?? PlatformCompability.Current;

            DataSet dataset = platform.SupportExcelReading ?
                await getManager<THHResourceManager>().loadExcelAsDataSet(path, platform) :
                await getManager<THHResourceManager>().loadDataSet(path, platform);

            if (dataset.Tables.Count < 1) throw new Exception("空数据集");
            var cardTable = dataset.Tables[0];
            var skinTable = dataset.Tables[1];
            List<CardSkinData> skins = new List<CardSkinData>();

            // Excel的第一行是注释，忽略
            for (int i = 1; i < cardTable.Rows.Count; i++)
            {
                try
                {
                    var datarow = cardTable.Rows[i];

                    if (datarow.ReadCol("Ignore", false))
                        continue;
                    CardSkinData skin = await loadSkinInCardTable(path, datarow, logger, loadResources);
                    int index = skins.FindIndex(s => s.id == skin.id);
                    if (index > 0)
                        skins[index] = skin;
                    else
                        skins.Add(skin);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error when loading {path} (row {i}): " + e);
                }
            }
            for (int i = 1; i < skinTable.Rows.Count; i++)
            {
                try
                {
                    var datarow = skinTable.Rows[i];

                    CardSkinData skin = await loadSkinInSkinTable(path, datarow, loadResources);
                    int index = skins.FindIndex(s => s.id == skin.id);
                    if (index > 0)
                        skins[index] = skin;
                    else
                        skins.Add(skin);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error when loading {path} (row {i}): " + e);
                }
            }
            return skins.ToArray();
        }

        private async Task<CardSkinData> loadSkinInCardTable(string path, DataRow datarow, StageLogger logger = null, bool loadResources = true)
        {
            //加载文字信息
            var skin = new CardSkinData()
            {
                id = datarow.ReadCol<int>("ID"),
                name = datarow.ReadCol<string>("Name"),
                desc = datarow.ReadCol("Desc", "")
            };
            logger?.NextSub("加载卡片资源: " + skin.name);
            //加载图片
            string imagePath = datarow.ReadCol("Image", "");

            if (loadResources)
            {
                try
                {
                    if (imagePath.StartsWith("res:"))
                    {
                        skin.image = Resources.Load<Sprite>(imagePath.Replace("res:", string.Empty));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(imagePath))
                        {
                            skin.image = await getDefaultSprite();
                        }
                        else
                        {
                            Texture2D texture = await getManager<THHResourceManager>().loadTexture(imagePath, Path.GetDirectoryName(path));
                            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
                            skin.image = sprite;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("加载贴图 " + imagePath + " 失败：" + e);
                    skin.image = await getDefaultSprite();
                }
            }
            else
            {
                if (imagePath == null || imagePath.StartsWith("res:"))
                {
                    //什么也不做
                }
                else
                {
                    //标记贴图路径为流文件
                    imagePath = "sa:" + Path.GetDirectoryName(path) + "/" + imagePath;
                }
                skin.imagePath = imagePath;
            }

            return skin;
        }
        private async Task<CardSkinData> loadSkinInSkinTable(string path, DataRow datarow, bool loadResources = true)
        {
            //加载文字信息
            var skin = new CardSkinData()
            {
                id = datarow.ReadCol<int>("ID"),
                name = datarow.ReadCol<string>("Name"),
                desc = datarow.ReadCol("Desc", "")
            };
            //加载图片
            string imagePath = datarow.ReadCol("Image", "");
            if (loadResources)
            {
                try
                {
                    if (imagePath.StartsWith("res:"))
                    {
                        skin.image = Resources.Load<Sprite>(imagePath.Replace("res:", string.Empty));
                    }
                    else
                    {
                        Texture2D texture = await getManager<THHResourceManager>().loadTexture(imagePath, Path.GetDirectoryName(path));
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
                        skin.image = sprite;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("加载贴图 " + imagePath + " 失败：" + e);
                    skin.image = await getDefaultSprite();
                }
            }
            else
            {
                if (imagePath == null || imagePath.StartsWith("res:"))
                {
                    //什么也不做
                }
                else
                {
                    //标记为流文件
                    imagePath = "sa:" + Path.GetDirectoryName(path) + "/" + imagePath;
                }
                skin.imagePath = imagePath;
            }
            //加载音效
            string soundPath = datarow.ReadCol("Sound_onUse", "");
            if (loadResources)
            {
                if (!string.IsNullOrEmpty(soundPath))
                    skin.sound_onUse = await getManager<THHResourceManager>().loadAudio(soundPath);
            }
            else
            {
                skin.soundPath_onUse = soundPath;
            }
            //加载投射物
            string projPath = datarow.ReadCol("Projectile", "");
            if (loadResources)
            {
                if (!string.IsNullOrEmpty(projPath))
                {
                    skin.projectile = await getManager<THHResourceManager>().loadObject<Projectile>(projPath);
                    if (skin.projectile == null)
                        UberDebug.LogWarningChannel("Card", skin + "加载投射物" + projPath + "失败");
                    else
                        UberDebug.LogChannel("Card", skin + "加载投射物" + skin.projectile);
                }
            }
            else
            {
                skin.projectilePath = projPath;
            }
            return skin;
        }

        /// <summary>
        /// 卸载所有加载的资源
        /// </summary>
        public void Unload()
        {

        }
        [SerializeField]
        string[] _externalCardPaths = new string[0];

        ExcelDataSetList excelList => ExcelDataSetList.Instance;

        Dictionary<int, CardDefine> defineDic { get; set; } = new Dictionary<int, CardDefine>();
        /// <summary>
        /// 根据id获取卡片定义
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardDefine getCardDefine(int id)
        {
            if (tryGetCardDefine(id, out CardDefine card))
                return card;
            return defineDic.ContainsKey(id) ? defineDic[id] : null;
        }
        public CardDefine getCardDefine(int cardPoolId, int cardId)
        {
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                return cardPool.cardDefineList.Find(d => d.id == cardId);
            }
            if (defineDic.ContainsKey(cardId))
            {
                return defineDic[cardId];
            }
            return null;
        }
        /// <summary>
        /// 根据条件获取卡片定义
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public CardDefine[] GetCardDefines(Func<CardDefine, bool> filter = null, bool includeExternalCard = false)
        {
            List<CardDefine> cardList = new List<CardDefine>();
            if (includeExternalCard)
            {
                foreach (var cardPool in _cardPoolDict.Values)
                {
                    cardList.AddRange(filter != null ?
                        cardPool.cardDefineList.Where(c => filter.Invoke(c)) :
                        cardPool.cardDefineList);
                }
            }
            cardList.AddRange(filter != null ?
                defineDic.Values.Where(d => filter.Invoke(d)) :
                defineDic.Values);
            return cardList.ToArray();
        }
        public bool isBuiltinCard(CardDefine card)
        {
            return defineDic.ContainsKey(card.id);
        }
        public CardDefine[] getCardDefines(int cardPoolId)
        {
            if (_cardPoolDict.TryGetValue(cardPoolId, out var cardPool))
            {
                //已经加载卡池
                return cardPool.cardDefineList.ToArray();
            }
            else
            {
                return new CardDefine[0];
            }
        }
        public CardDefine[] getCardDefines(Predicate<CardDefine> filter = null)
        {
            List<CardDefine> cardDefineList = new List<CardDefine>();
            //反射卡牌
            cardDefineList.AddRange(filter == null ? defineDic.Values : defineDic.Values.Where(c => filter(c)));
            //卡池卡牌
            foreach (var cardPool in _cardPoolDict.Values)
            {
                cardDefineList.AddRange(filter == null ? cardPool.cardDefineList : cardPool.cardDefineList.Where(c => filter(c)));
            }
            return cardDefineList.ToArray();
        }
        public async Task<CardDefine[]> getOrLoadCardDefines(int cardPoolId)
        {
            if (_cardPoolDict.TryGetValue(cardPoolId, out var cardPool))
            {
                //已经加载卡池
                return cardPool.cardDefineList.ToArray();
            }
            else if (getCardPoolPath(cardPoolId) is string path)
            {
                //未加载卡池但是存在地址
                var result = await loadCardPool(path);
                return result.cardPool.cardDefineList.ToArray();
            }
            else
            {
                return new CardDefine[0];
            }
        }
        public CardSkinData getSkin(TouhouCardEngine.Card card)
        {
            return getSkin(card.define.id);
        }
        public CardSkinData getDefaultSkin(CardDefine cardDefine)
        {
            return getDefaultSkin(cardDefine.id);
        }
        public CardSkinData getDefaultSkin(int id)
        {
            var skinGroup = getAllSkins(id).GroupBy(s => s.skinId).FirstOrDefault();
            if (skinGroup == null)
            {
                //卡牌没有任何皮肤
                throw new InvalidOperationException("卡牌" + id + "没有任何皮肤");
            }
            //获取对应语言的皮肤
            CardSkinData skin = skinGroup.FirstOrDefault(s => s.language == game.langManager.getLanguage());
            //如果没有对应语言，就第一个
            if (skin == null)
                skin = skinGroup.First();
            return skin;
        }
        /// <summary>
        /// 根据id获取卡片皮肤
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardSkinData getSkin(int id)
        {
            if (tryGetCardSkin(id, out CardSkinData skin))
                return skin;
            if (tempSkinDic.ContainsKey(id))
                return tempSkinDic[id];
            if (skinDic.ContainsKey(id))
                return skinDic[id];
            return null;
        }
        public bool tryGetSkin(int id, out CardSkinData skin)
        {
            if (tempSkinDic.ContainsKey(id))
            {
                skin = tempSkinDic[id];
                return true;
            }
            else if (skinDic.ContainsKey(id))
            {
                skin = skinDic[id];
                return true;
            }
            else
            {
                skin = null;
                return false;
            }
        }
        Dictionary<int, CardSkinData> tempSkinDic { get; } = new Dictionary<int, CardSkinData>();
        /// <summary>
        /// 添加临时的卡片皮肤
        /// </summary>
        /// <param name="skin"></param>
        public void AddCardSkinTemp(CardSkinData skin)
        {
            if (skinDic.ContainsKey(skin.id))
            {
                Debug.LogError("已存在相同的皮肤" + skinDic[skin.id]);
                return;
            }
            if (tempSkinDic.ContainsKey(skin.id))
            {
                Debug.LogError("已存在相同的皮肤" + tempSkinDic[skin.id]);
                return;
            }
            tempSkinDic.Add(skin.id, skin);
        }
        public bool isStandardCard(CardDefine c)
        {
            if (c.id == 0)//非法卡片
                return false;
            if (c.GetType().Assembly != typeof(THHGame).Assembly)//非游戏用卡片
                return false;
            if (c is MasterCardDefine)//不是英雄牌
                return false;
            else if (c is SkillCardDefine)//不是技能牌
                return false;
            else if (c is ServantCardDefine servant)
            {
                if (servant.isToken)
                    return false;
            }
            else if (c is SpellCardDefine spell)
            {
                if (spell.isToken)
                    return false;
            }
            else if (c is ItemCardDefine item)
            {
                if (item.isToken)
                {
                    return false;
                }
            }
            else if (c is GeneratedCardDefine generated)
            {
                if (generated.type == DEFINE.MASTER ||
                    generated.type == DEFINE.SKILL)
                    return false;
                if (generated.getProp<bool>(nameof(ServantCardDefine.isToken)))
                    return false;
            }
            return true;
        }
        #region 属性字段
        public MainManager game
        {
            get { return _game; }
        }
        [SerializeField]
        MainManager _game;
        #endregion
    }

    [Serializable]
    public class SkinNotFoundException : Exception
    {
        public SkinNotFoundException() { }
        public SkinNotFoundException(string message) : base(message) { }
        public SkinNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected SkinNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class StageLogger
    {
        int stage = -1;
        int subCount = 0;
        int subStage = 0;
        int[] factors;
        int factorCount = 0;
        int currentFactor = 0;

        public event Action<float, string> OnStageChange;

        public StageLogger(int stages)
        {
            factors = new int[stages];
            factorCount = stages;
            for (int i = 0; i < stages; i++)
            {
                factors[i] = 1;
            }
        }

        public StageLogger(int[] factors)
        {
            this.factors = factors;
            factorCount = 0;
            foreach (var item in factors)
            {
                factorCount += item;
            }
        }

        float progress => (currentFactor + (float)subStage / subCount * factors[stage]) / factorCount;

        public void NextStage(string name, int count = 1)
        {
            if (stage >= 0) currentFactor += factors[stage];
            subCount = count;
            subStage = 0;
            stage++;
            OnStageChange?.Invoke(progress, name);
        }

        public void NextSub(string name, int count = 1)
        {
            subStage++;
            OnStageChange?.Invoke(progress, name);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using IGensoukyo;
using IGensoukyo.Utilities;
using MongoDB.Bson;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UI;
using UnityEngine;
using ILogger = TouhouCardEngine.Shared.ILogger;

namespace Game
{
    public partial class CardManager : Manager
    {
        #region 公有方法

        #region 加载内置卡
        /// <summary>
        /// 加载配置路径中的所有卡片和皮肤。
        /// </summary>
        /// <returns></returns>
        public Task loadBuiltinCards(Assembly[] assemblies = default, string[] namespaces = null, bool builtinOnly = false, ILogger logger = null, StageLogger screenLogger = null, bool loadResources = true)
        {
            return loadBuiltinCards(builtinOnly ? new string[0] : _externalCardPaths, assemblies, namespaces, logger, screenLogger, loadResources);
        }
        /// <summary>
        /// 从给出的路径中加载卡片和皮肤，支持通配符，比如“Cards/*.xls”
        /// </summary>
        /// <param name="excelPaths"></param>
        /// <returns></returns>
        public async Task loadBuiltinCards(string[] excelPaths, Assembly[] assemblies = default, string[] namespaces = null, ILogger logger = null, StageLogger screenLogger = null, bool loadResources = true)
        {
            //加载内置卡片
            screenLogger?.NextStage("加载内置卡片");
            if (assemblies == default)
                assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // 将通过反射获取到的卡牌加入卡牌定义字典。
            var gotCardDefines = CardHelper.getCardDefines(assemblies, namespaces, logger);
            foreach (CardDefine card in gotCardDefines)
            {
                if (builtinDefineDic.ContainsKey(card.id))
                {
                    Debug.LogWarning($"存在重复ID的卡片{builtinDefineDic[card.id]}和{card}");
                    continue;
                }
                builtinDefineDic.Add(card.id, card);
            }

            // 获取外置卡牌xls的路径
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
                                if (!pathList.Contains(item))
                                    pathList.Add(item);
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
                                    if (!pathList.Contains(item))
                                        pathList.Add(item);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!pathList.Contains(excelPath))
                            pathList.Add(excelPath);
                    }
                }
            }

            // 加载xls外置卡牌
            screenLogger?.NextStage("加载卡片集", pathList.Count);
            int cardCount = 0;
            List<CardDefine> cardList = new List<CardDefine>();
            foreach (string path in pathList)
            {
                try
                {
                    screenLogger?.NextSub($"加载卡片集：{Path.GetFileNameWithoutExtension(path)}");

                    CardDefine[] cards = await loadXLSCards(path);
                    foreach (var card in cards)
                    {
                        CardDefine older = builtinDefineDic.FirstOrDefault(p => p.Value.id == card.id).Value;
                        if (older != null)
                        {
                            older.merge(card);
                            UberDebug.LogDebugChannel("Load", $"数据覆盖：{card.ToJson()}");
                        }
                        else
                            cardList.Add(card);
                    }
                    cardCount += cards.Length;
                }
                catch (Exception e)
                {
                    UberDebug.LogError($"读取外部卡牌文件{path}失败，发生异常：{e}");
                }
            }

            // 外置卡牌去重
            foreach (CardDefine card in cardList)
            {
                if (builtinDefineDic.ContainsKey(card.id))
                {
                    Debug.LogWarning($"存在重复ID的卡片{builtinDefineDic[card.id]}和{card}");
                    continue;
                }
                builtinDefineDic.Add(card.id, card);
            }
            cardList.Clear();


            //加载内置皮肤
            builtinSkinDic.Clear();
            foreach (CardSkinData skin in _skins)
            {
                if (skin.image == null)
                    skin.image = getDefaultSprite();
                builtinSkinDic[skin.id] = skin;
            }

            //加载外置皮肤
            screenLogger?.NextStage("加载外置皮肤", cardCount);
            foreach (string path in pathList)
            {
                try
                {
                    CardSkinData[] skins = await loadXLSSkins(path, logger: screenLogger, loadResources: loadResources);
                    foreach (var skin in skins)
                    {
                        if (builtinSkinDic.ContainsKey(skin.id))
                            Debug.LogWarning($"存在重复ID的皮肤{builtinSkinDic[skin.id]}和{skin}");
                        builtinSkinDic[skin.id] = skin;
                    }
                }
                catch (Exception e)
                {
                    UberDebug.LogError($"读取外部皮肤文件{path}失败，发生异常：{e}");
                }
            }
        }
        /// <summary>
        /// 加载xls文件中的卡片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<CardDefine[]> loadXLSCards(string path, PlatformCompability platform = null)
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
                UberDebug.LogErrorChannel("Card", $"读取{path}发生异常：{e}");
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
                        if (builtinDefineDic.ContainsKey(id))
                        {
                            Debug.Log($"忽略卡牌{builtinDefineDic[id]}");
                            builtinDefineDic.Remove(id);
                        }
                        continue;
                    }

                    var card = new CardDefine(datarow.ReadCol<int>("ID"), datarow.ReadCol<string>("Type"));
                    card.setProp(DEFINE.COST, datarow.ReadCol("Cost", 0));
                    card.setProp(DEFINE.ATTACK, datarow.ReadCol("Attack", 0));
                    card.setProp(DEFINE.LIFE, datarow.ReadCol("Life", 0));
                    card.setProp(DEFINE.TAGS, datarow.ReadCol("Tags", "").Split(','));
                    card.setProp(DEFINE.KEYWORDS, datarow.ReadCol("Keywords", "").Split(','));
                    card.setProp(DEFINE.ISTOKEN, datarow.ReadCol("IsToken", false));

                    cards.Add(card);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error when loading {path} (row {i}): {e}");
                }
            }

            return cards.ToArray();
        }
        #endregion

        #region 创建卡牌定义
        public void createCard(ICardPoolReference poolRef, out CardDefine cardDefine, out CardSkinData cardSkin)
        {
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
            {
                throw new KeyNotFoundException($"不存在卡池{poolRef}");
            }
            int cardId = 0;
            for (int i = 1; i < 0xFFFF; i++)
            {
                cardId = i;
                if (cardPool.cardDefineList.Exists(c => c.id == cardId))
                    continue;
                break;
            }
            cardDefine = new CardDefine(cardId, DEFINE.SERVANT);
            cardDefine.cardPoolId = poolRef.CardPoolID;
            cardSkin = createSkin(cardDefine.cardPoolId, cardDefine.id);
        }
        public CardDefine cloneCardDefine(CardDefine define)
        {
            var json = new SerializableCardDefine(define).toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableCardDefine>(json);
            var nodeDefiner = getLocalNodeDefiner();
            var clone = cloneSeri.toCardDefine(nodeDefiner);
            clone.cardPoolId = define.cardPoolId;
            return clone;
        }
        #endregion

        #region 添加卡牌定义
        public void addCard(long cardPoolId, CardDefine cardDefine)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在可编辑的卡池{cardPoolId}");
            }
            cardPool.cardDefineList.Add(cardDefine);
        }
        #endregion

        #region 移除卡牌定义
        public bool removeCard(CardDefine card)
        {
            var poolRef = getCardPoolReferenceOfCard(card);
            if (tryGetCardPool(poolRef, out CardPool cardPool))
            {
                if (cardPool.cardDefineList.Remove(card))
                {
                    cardPool.cardSkinList.RemoveAll(s => s.id == card.id);
                    return true;
                }
            }
            return false;
        }
        public bool removeCard(DefineReference defineRef)
        {
            var cardDefine = getLocalCardDefine(defineRef);
            return removeCard(cardDefine);
        }
        public bool removeCard(long cardPoolId, int defineId)
        {
            var cardDefine = getLocalCardDefine(cardPoolId, defineId);
            return removeCard(cardDefine);
        }
        #endregion

        #region 获取卡牌定义
        /// <summary>
        /// 根据id获取卡片定义
        /// </summary>
        /// <param name="cardPoolRef">卡池引用。</param>
        /// <param name="cardId">卡牌ID。</param>
        /// <returns></returns>
        public CardDefine getCardDefine(ICardPoolReference cardPoolRef, int cardId)
        {
            if (cardPoolRef == null)
                return null;
            // 老版本卡牌定义的兼容。
            if (cardPoolRef.CardPoolID == 0 && cardId < 0)
                cardPoolRef = new CardPoolReference(cardId << 1 >> 17, cardId);

            if (tryGetCardPool(cardPoolRef, out CardPool cardPool)) // 自定义卡池。
            {
                return cardPool.cardDefineList.Find(d => d.id == cardId);
            }
            else if (builtinDefineDic.ContainsKey(cardId)) // 内置卡池。
            {
                return builtinDefineDic[cardId];
            }
            return null;
        }
        public CardDefine getLocalCardDefine(long cardPoolId, int id)
        {
            var poolRef = game.card.getLocalCardPoolReference(cardPoolId);
            return getCardDefine(poolRef, id);
        }
        public CardDefine getLocalCardDefine(DefineReference defRef)
        {
            if (defRef == null)
                return null;
            return getLocalCardDefine(defRef.cardPoolId, defRef.defineId);
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
                builtinDefineDic.Values.Where(d => filter.Invoke(d)) :
                builtinDefineDic.Values);
            return cardList.ToArray();
        }
        public Dictionary<int, CardDefine> getCardDefines(ICardPoolReference cardPoolRef)
        {
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
                return cardPool.cardDefineList.ToDictionary(c => c.id);
            return null;
        }
        public CardDefine[] findCardDefines(Predicate<CardDefine> filter = null)
        {
            List<CardDefine> cardDefineList = new List<CardDefine>();
            //反射卡牌
            cardDefineList.AddRange(filter == null ? builtinDefineDic.Values : builtinDefineDic.Values.Where(c => filter(c)));
            //卡池卡牌
            foreach (var cardPool in _cardPoolDict.Values)
            {
                cardDefineList.AddRange(filter == null ? cardPool.cardDefineList : cardPool.cardDefineList.Where(c => filter(c)));
            }
            return cardDefineList.ToArray();
        }
        public async Task<CardDefine[]> getOrLoadCardDefines(ICardPoolReference cardPoolRef)
        {
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                //已经加载卡池
                return cardPool.cardDefineList.ToArray();
            }
            else if (getCardPoolPath(cardPoolRef) is string path)
            {
                //未加载卡池但是存在地址
                try
                {
                    var result = await loadCardPool(path);
                    return result.cardPool.cardDefineList.ToArray();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return Array.Empty<CardDefine>();
                }
            }
            else
            {
                return new CardDefine[0];
            }
        }
        #endregion

        public bool isBuiltinCard(CardDefine card)
        {
            if (card == null)
                return false;
            return isBuiltinCardPool(card.cardPoolId);
        }
        /// <summary>
        /// 尝试获取翻译过后的卡牌标签名称。
        /// </summary>
        /// <param name="tag">英文标签。</param>
        /// <param name="text">翻译过后的标签。</param>
        /// <returns>是否成功获取。</returns>
        public bool tryGetTranslatedTag(string tag, out string text)
        {
            if (tag != null && _translateTagDic.TryGetValue(tag, out string translatedTag))
            {
                text = game.lang._(translatedTag);
                return true;
            }
            text = null;
            return false;
        }
        public INodeDefiner getLocalNodeDefiner()
        {
            return new THHNodeDefiner(
                (a, n) => getLocalActionDefine(a),
                (poolId, name) => getEventTriggerTime(getLocalCardPoolReference(poolId), name, true));
        }
        public static DefineReference getClassicCardRef(int id)
        {
            return new DefineReference(STANDARD_POOL_ID, id);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected override void onAwake()
        {
            base.onAwake();
            onAwake_Sprite();
            onAwake_CardPool();
            onAwake_Actions();
        }
        protected void Reset()
        {
            _game = TransformHelper.findComponentInScene<MainManager>(gameObject.scene);
        }
        #endregion

        #endregion

        #region 属性字段
        [TranslateMsg("卡牌标签")]
        public const string CARD_TAG_FAIRY = "妖精";
        [TranslateMsg("卡牌标签")]
        public const string CARD_TAG_MECH = "机械";
        [TranslateMsg("卡牌标签")]
        public const string CARD_TAG_DEMON = "恶魔";
        [TranslateMsg("卡牌标签")]
        public const string CARD_TAG_ELEMENTAL = "元素";
        [TranslateMsg("卡牌标签")]
        public const string CARD_TAG_BEAST = "野兽";
        [TranslateMsg("卡牌标签")]
        public const string CARD_TAG_BUG = "昆虫";
        [TranslateMsg("卡牌标签")]
        public const string CARD_TAG_GOD = "神明";
        [TranslateMsg("卡牌标签")]
        public const string CARD_TAG_GHOST = "幽灵";
        public MainManager game
        {
            get { return _game; }
            set { _game = value; }
        }
        private static Dictionary<string, string> _translateTagDic = new Dictionary<string, string>
        {
            { TAG.FAIRY, CARD_TAG_FAIRY },
            { TAG.MACHINE, CARD_TAG_MECH },
            { TAG.DEMON, CARD_TAG_DEMON },
            { TAG.ELEMENT, CARD_TAG_ELEMENTAL },
            { TAG.BEAST, CARD_TAG_BEAST },
            { TAG.BUG, CARD_TAG_BUG },
            { TAG.GOD, CARD_TAG_GOD },
            { TAG.GHOST, CARD_TAG_GHOST },
        };
        private ExcelDataSetList excelList => ExcelDataSetList.Instance;
        private Dictionary<int, CardDefine> builtinDefineDic { get; set; } = new Dictionary<int, CardDefine>();
        [SerializeField]
        private MainManager _game;
        [SerializeField]
        private string[] _externalCardPaths = new string[0];
        #endregion
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

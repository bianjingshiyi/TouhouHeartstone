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
using TouhouCardEngine.Shared;
using TouhouHeartstone.Builtin;
using Controllers;

namespace Game
{
    public partial class CardManager : Manager
    {
        #region 公有方法

        #region 加载
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
                    Debug.LogWarning($"存在重复ID的卡片{defineDic[card.id]}和{card}");
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
                    screenLogger?.NextSub($"加载卡片集：{Path.GetFileNameWithoutExtension(path)}");

                    CardDefine[] cards = await loadCards(path);
                    foreach (var card in cards)
                    {
                        CardDefine older = defineDic.FirstOrDefault(p => p.Value.id == card.id).Value;
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
            foreach (CardDefine card in cardList)
            {
                if (defineDic.ContainsKey(card.id))
                {
                    Debug.LogWarning($"存在重复ID的卡片{defineDic[card.id]}和{card}");
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
                    skin.image = getDefaultSprite();
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
                            Debug.LogWarning($"存在重复ID的皮肤{skinDic[skin.id]}和{skin}");
                        skinDic[skin.id] = skin;
                    }
                }
                catch (Exception e)
                {
                    UberDebug.LogError($"读取外部皮肤文件{path}失败，发生异常：{e}");
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
                        if (defineDic.ContainsKey(id))
                        {
                            Debug.Log($"忽略卡牌{defineDic[id]}");
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
                    Debug.LogWarning($"Error when loading {path} (row {i}): {e}");
                }
            }

            return cards.ToArray();
        }
        #endregion

        #region 创建卡牌定义
        public void createCard(CardPoolInfo cardPoolInfo, out CardDefine cardDefine, out CardSkinData cardSkin)
        {
            if (!tryGetCardPool(cardPoolInfo.ToReference(), out CardPool cardPool))
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolInfo.cardPoolId}");
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
            cardDefine.cardPoolId = cardPoolInfo.cardPoolId;
            cardSkin = new CardSkinData();
            cardSkin.id = cardDefine.id;
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
        public void addCard(CardPoolInfo cardPoolInfo, CardDefine cardDefine)
        {
            addCard(cardPoolInfo.cardPoolId, cardDefine);
        }
        public void addCard(long cardPoolId, CardDefine cardDefine)
        {
            var cardPool = getLocalCardPool(cardPoolId);
            if (cardPool == null)
            {
                throw new KeyNotFoundException($"不存在卡池{cardPoolId}");
            }
            cardPool.cardDefineList.Add(cardDefine);
        }
        #endregion

        #region 移除卡牌定义
        public void removeCard(CardDefine card)
        {
            var cardPool = getCardPoolOfCard(card);
            if (cardPool != null)
            {
                if (cardPool.cardDefineList.Remove(card))
                {
                    cardPool.cardSkinList.RemoveAll(s => s.id == card.id);
                }
            }
        }
        public void removeCard(long cardPoolId, int cardId)
        {
            removeCard(getCardDefine(cardPoolId, cardId));
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
            if (cardPoolRef.CardPoolID == 0 && cardId < 0)
            {
                cardPoolRef = new CardPoolReference(cardId << 1 >> 17, cardId);
            }
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
            {
                return cardPool.cardDefineList.Find(d => d.id == cardId);
            }
            else if (defineDic.ContainsKey(cardId))
            {
                return defineDic[cardId];
            }
            return null;
        }
        /// <summary>
        /// 根据卡池ID和卡牌ID获取卡片定义
        /// </summary>
        /// <param name="cardPoolRef">卡池引用。</param>
        /// <param name="cardId">卡牌ID。</param>
        /// <returns></returns>
        public CardDefine getCardDefine(long cardPoolId, int cardId)
        {
            if (cardPoolId == 0 && cardId < 0)
                cardPoolId = cardId << 1 >> 17;
            if (tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                return cardPool.cardDefineList.Find(d => d.id == cardId);
            }
            else if (defineDic.ContainsKey(cardId))
            {
                return defineDic[cardId];
            }
            return null;
        }
        /// <summary>
        /// 根据卡牌引用获取卡片定义
        /// </summary>
        /// <param name="cardPoolRef">卡池引用。</param>
        /// <param name="cardId">卡牌ID。</param>
        /// <returns></returns>
        public CardDefine getCardDefine(DefineReference defRef)
        {
            return getCardDefine(defRef.cardPoolId, defRef.defineId);
        }
        public CardDefine getLocalCardDefine(DefineReference defRef)
        {
            var poolRef = game.card.getLocalCardPoolReference(defRef.cardPoolId);
            return getCardDefine(poolRef, defRef.defineId);
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

        public Dictionary<ICardPoolReference, Dictionary<int, CardDefine>> getCardDefines(ICardPoolReference[] cardPoolsId)
        {
            Dictionary<ICardPoolReference, Dictionary<int, CardDefine>> cardPoolDict = new Dictionary<ICardPoolReference, Dictionary<int, CardDefine>>();
            for (int i = 0; i < cardPoolsId.Length; i++)
            {
                if (_cardPoolDict.TryGetValue(cardPoolsId[i], out var cardPool))
                {
                    //已经加载卡池
                    cardPoolDict.Add(cardPoolsId[i], cardPool.cardDefineList.ToDictionary(c => c.id));
                }
            }
            return cardPoolDict;
        }
        public Dictionary<int, CardDefine> getCardDefines(ICardPoolReference cardPoolRef)
        {
            if (tryGetCardPool(cardPoolRef, out CardPool cardPool))
                return cardPool.cardDefineList.ToDictionary(c => c.id);
            return null;
        }
        public Dictionary<ICardPoolReference, Dictionary<int, CardDefine>> getCardDefines()
        {
            Dictionary<ICardPoolReference, Dictionary<int, CardDefine>> cardPoolDict = new Dictionary<ICardPoolReference, Dictionary<int, CardDefine>>();
            foreach (var cardPool in _cardPoolDict)
            {
                cardPoolDict.Add(cardPool.Key, cardPool.Value.cardDefineList.ToDictionary(c => c.id));
            }
            cardPoolDict.Add(builtinCardPoolReference, new Dictionary<int, CardDefine>(defineDic));
            return cardPoolDict;
        }
        public CardDefine[] findCardDefines(Predicate<CardDefine> filter = null)
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

        #region 卡牌卡池检测
        public bool isBuiltinCard(CardDefine card)
        {
            if (card == null)
                return false;
            return isBuiltinCardPool(card.cardPoolId);
        }
        public bool isBuiltinCardPool(long cardPoolId)
        {
            return cardPoolId == 0;
        }
        /// <summary>
        /// 卡牌定义是否是自定义。
        /// </summary>
        /// <param name="card">卡牌定义。</param>
        /// <returns></returns>
        public bool isExternalCard(CardDefine card)
        {
            return isExternalCard(card.cardPoolId, card.id);
        }
        /// <summary>
        /// 卡牌引用是否是自定义。
        /// </summary>
        /// <param name="card">卡牌ID。</param>
        /// <returns></returns>
        public bool isExternalCard(DefineReference defRef)
        {
            return isExternalCard(defRef.cardPoolId, defRef.defineId);
        }
        /// <summary>
        /// 卡牌ID是否是自定义。
        /// </summary>
        /// <param name="card">卡牌ID。</param>
        /// <returns></returns>
        public bool isExternalCard(long cardPoolId, int cardId)
        {
            if (cardPoolId == 0 || Array.IndexOf(standardCardPoolsId, cardPoolId) >= 0)
            {
                //0是内置卡池，包含在经典卡池内是经典卡池
                return false;
            }
            return true;
        }
        #endregion

        /// <summary>
        /// 尝试获取翻译过后的卡牌标签名称。
        /// </summary>
        /// <param name="tag">英文标签。</param>
        /// <param name="text">翻译过后的标签。</param>
        /// <returns>是否成功获取。</returns>
        public bool tryGetTranslatedTag(string tag, out string text)
        {
            if (_translateTagDic == null)
            {
                _translateTagDic = new Dictionary<string, string>();
                var T = typeof(TranslateTags);
                FieldInfo[] fields = T.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < fields.Length; i++)
                {
                    _translateTagDic.Add(fields[i].Name, fields[i].GetValue(null).ToString());
                    //Debug.Log("这里是字典结果" + fields[i].Name + fields[i].GetValue(null).ToString());
                }
            }
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
                (a, n) => getActionDefine(a),
                (poolId, name) => getEventTriggerTime(getLocalCardPoolReference(poolId), name, true));
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
        public MainManager game
        {
            get { return _game; }
            set { _game = value; }
        }
        private static Dictionary<string, string> _translateTagDic;
        private ExcelDataSetList excelList => ExcelDataSetList.Instance;
        private Dictionary<int, CardDefine> defineDic { get; set; } = new Dictionary<int, CardDefine>();
        [SerializeField]
        private MainManager _game;
        [SerializeField]
        private string[] _externalCardPaths = new string[0];
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

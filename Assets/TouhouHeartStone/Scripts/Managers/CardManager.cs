using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game
{
    public partial class CardManager : Manager
    {
        #region 公有方法

        #region 创建卡牌定义
        public void createCard(ICardPoolReference poolRef, out CardDefine cardDefine, out CardSkinData cardSkin)
        {
            if (!tryGetCardPool(poolRef, out CardPool cardPool))
            {
                throw new KeyNotFoundException($"不存在卡池{poolRef}");
            }
            var cardId = allocCardDefineID(cardPool);
            cardDefine = new CardDefine(cardId, DEFINE.SERVANT);
            cardDefine.cardPoolId = poolRef.CardPoolID;
            cardSkin = createSkin(cardDefine.cardPoolId, cardDefine.id);
            foreach (var soundName in CardSkinData.servantCardSkinSoundNames)
            {
                cardSkin.addSoundRef(new SkinSoundRef(soundName, null));
            }
        }
        public int allocCardDefineID(CardPool cardPool)
        {
            for (int i = 1; i < 0xFFFF; i++)
            {
                if (!cardPool.cardDefineList.Exists(c => c.id == i))
                    return i;
            }
            return 0;
        }
        public CardDefine cloneCardDefine(CardDefine define)
        {
            var json = new SerializableCardDefine(define).toJson();
            var cloneSeri = JsonHelper.fromJson<SerializableCardDefine>(json);
            var nodeDefiner = getLocalNodeDefiner();
            var clone = cloneSeri.toCardDefine(nodeDefiner);
            clone.cardPoolId = define.cardPoolId;
            foreach (var effect in clone.getGeneratedEffects())
            {
                effect.cardDefineRef = clone.getDefineRef();
                effect.buffDefineRef = null;
            }
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
        public CardDefine[] GetCardDefines(Func<CardDefine, bool> filter = null)
        {
            List<CardDefine> cardList = new List<CardDefine>();
            foreach (var cardPool in _localCardPoolDict.Values)
            {
                cardList.AddRange(filter != null ?
                    cardPool.cardDefineList.Where(c => filter.Invoke(c)) :
                    cardPool.cardDefineList);
            }
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
            //卡池卡牌
            foreach (var cardPool in _localCardPoolDict.Values)
            {
                cardDefineList.AddRange(filter == null ? cardPool.cardDefineList : cardPool.cardDefineList.Where(c => filter(c)));
            }
            foreach (var cardPool in _cachedCardPoolDict.Values)
            {
                cardDefineList.AddRange(filter == null ? cardPool.cardDefineList : cardPool.cardDefineList.Where(c => filter(c)));
            }
            return cardDefineList.ToArray();
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
            if (tag != null && _translateTagDic.TryGetValue(tag, out string translatedTag))
            {
                text = game.lang._(translatedTag);
                return true;
            }
            text = null;
            return false;
        }
        public INodeDefiner getNodeDefiner(IEnumerable<ICardPoolReference> env)
        {
            return new THHNodeDefiner(
                (a, n) => getActionDefineInEnv(a, env),
                (eventRef) => getEventDefineInEnv(eventRef, env, true));
        }
        public INodeDefiner getLocalNodeDefiner()
        {
            return getNodeDefiner(getLocalCardPoolReferences());
        }
        public static DefineReference getClassicCardRef(int id)
        {
            return new DefineReference(STANDARD_POOL_ID, id);
        }
        public CardDefine[] evaluateCardTokens(CardDefine define, IEnumerable<ICardPoolReference> cardPoolSet)
        {
            List<CardDefine> cardDefines = new List<CardDefine>();
            List<ActionGraph> checkedGraphs = new List<ActionGraph>();
            foreach (var effect in define.getGeneratedEffects())
            {
                addEffectDefRef(effect);
            }
            foreach (var propName in define.getPropNames())
            {
                addDefRefByObject(define.getProp<object>(propName));
            }
            return cardDefines.ToArray();

            void addDefRefByObject(object obj)
            {
                if (obj is DefineReference defRef)
                {
                    addDefRef(defRef);
                }
                else if (obj is IList list)
                {
                    foreach (var item in list)
                    {
                        addDefRefByObject(item);
                    }
                }
            }
            void addDefRef(DefineReference defRef)
            {
                if (defRef is BuffReference)
                    return;
                var poolRef = getCardPoolReferenceInEnv(defRef.cardPoolId, cardPoolSet);
                var cardDefine = getCardDefine(poolRef, defRef.defineId);
                if (cardDefine == null || cardDefines.Contains(cardDefine))
                    return;
                if (cardDefine.isMaster() || cardDefine == define)
                    return;
                cardDefines.Add(cardDefine);
            }
            void addEffectDefRef(GeneratedEffect effect)
            {
                if (effect == null)
                    return;
                addGraphDefRef(effect.graph);
                foreach (var prop in effect.propDict)
                {
                    if (prop.Value is DefineReference defRef)
                        addDefRef(defRef);
                }
            }
            void addGraphDefRef(ActionGraph actionGraph)
            {
                if (checkedGraphs.Contains(actionGraph))
                    return;
                checkedGraphs.Add(actionGraph);
                foreach (var node in actionGraph.nodes)
                {
                    if (node is not ActionNode action || action.defineRef == null)
                        continue;
                    foreach (var inputPort in action.inputPorts)
                    {
                        var name = inputPort.name;
                        var paramIndex = inputPort is ValueInput vi ? vi.paramIndex : -1;
                        var inputDefaultValue = action.getInputDefaultValue(name, paramIndex);
                        if (inputDefaultValue == null)
                            continue;
                        var inputValueType = inputPort.define.type.getNodeValueType();
                        if (inputValueType.type == ActionNodeValueType.buffReference)
                        {
                            addBuffDefRefByObject(inputDefaultValue);
                        }
                        else if (inputValueType.type == ActionNodeValueType.cardReference)
                        {
                            addDefRefByObject(inputDefaultValue);
                        } 
                    }
                }
            }
            void addBuffDefRefByObject(object obj)
            {
                if (obj is DefineReference defRef)
                {
                    addBuffDefRef(defRef);
                }
                else if (obj is IList list)
                {
                    foreach (var item in list)
                    {
                        addBuffDefRefByObject(item);
                    }
                }
            }
            void addBuffDefRef(DefineReference buffRef)
            {
                var poolRef = getCardPoolReferenceInEnv(buffRef.cardPoolId, cardPoolSet);
                if (tryGetBuffDefine(poolRef, buffRef.defineId, out var buffDefine))
                {
                    foreach (var effect in buffDefine.effectList)
                    {
                        addEffectDefRef(effect);
                    }
                }
            }
        }
        public async Task<DeckItem[]> orderDeckItemsByLocalPools(IEnumerable<DeckItem> deckItems)
        {
            // 加载未加载的卡池。
            foreach (var deckItem in deckItems)
            {
                var poolRef = getLocalCardPoolReference(deckItem.cardRef.cardPoolId);
                await getOrLoadCardPool(poolRef);
            }
            return deckItems.OrderBy(item =>
                {
                    var cardRef = item.cardRef;
                    var poolRef = getLocalCardPoolReference(cardRef.cardPoolId);
                    return getCardDefine(poolRef, cardRef.defineId) is CardDefine cardDefine ? cardDefine.getCost() : 0;
                })
                .ThenBy(item => item.cardRef.cardPoolId)
                .ThenBy(item => item.cardRef.defineId)
                .ToArray();
        }
        public async Task<DeckItem[]> orderDeckItemsByPoolsInEnv(IEnumerable<DeckItem> deckItems, IEnumerable<ICardPoolReference> env)
        {
            // 加载未加载的卡池。
            foreach (var deckItem in deckItems)
            {
                var poolRef = getCardPoolReferenceInEnv(deckItem.cardRef.cardPoolId, env);
                await getOrLoadCardPool(poolRef);
            }
            return deckItems.OrderBy(item =>
                {
                    var cardRef = item.cardRef;
                    var poolRef = getCardPoolReferenceInEnv(cardRef.cardPoolId, env);
                    return getCardDefine(poolRef, cardRef.defineId) is CardDefine cardDefine ? cardDefine.getCost() : 0;
                })
                .ThenBy(item => item.cardRef.cardPoolId)
                .ThenBy(item => item.cardRef.defineId)
                .ToArray();
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected override void onAwake()
        {
            base.onAwake();
            _persistentDataPath = Application.persistentDataPath;
            onAwake_Sprite();
            onAwake_CardPool();
            onAwake_Actions();
            onAwake_Keywords();
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
        private static Dictionary<string, string> _translateTagDic = new Dictionary<string, string>
        {
            { TAG.FAIRY, StringTable.CARD_TAG_FAIRY },
            { TAG.MACHINE, StringTable.CARD_TAG_MECH },
            { TAG.DEMON, StringTable.CARD_TAG_DEMON },
            { TAG.ELEMENT, StringTable.CARD_TAG_ELEMENTAL },
            { TAG.BEAST, StringTable.CARD_TAG_BEAST },
            { TAG.GOD, StringTable.CARD_TAG_GOD },
            { TAG.GHOST, StringTable.CARD_TAG_GHOST },
        };
        [SerializeField]
        private MainManager _game;
        #endregion
    }
}

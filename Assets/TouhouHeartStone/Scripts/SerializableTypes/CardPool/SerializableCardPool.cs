using System;
using System.Collections.Generic;
using System.Linq;
using Game.Animations;
using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableCardPool
    {
        #region 公有方法
        #region 构造方法
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableCardPool()
        {
        }
        public SerializableCardPool(CardPool cardPool)
        {
            if (cardPool == null)
                throw new ArgumentNullException(nameof(cardPool));
            originLanguage = cardPool.originLanguage;
            cardDefineList = cardPool.cardDefineList.ConvertAll(c => new SerializableCardDefine(c));
            cardDefineList.Sort((a, b) => a.id.CompareTo(b.id));
            cardSkinList = cardPool.cardSkinList.ConvertAll(s => new SerializableCardSkinData(s));
            cardSkinList.Sort((a, b) =>
            {
                int result = a._id.CompareTo(b._id);
                if (result != 0)
                    return result;
                return a._skinId.CompareTo(b._skinId);
            });
            buffDefineList = cardPool.buffDefineList.ConvertAll(b => new SerializableBuffDefine(b));
            buffDefineList.Sort((a, b) => a.id.CompareTo(b.id));
            buffSkinList = cardPool.buffSkinList.ConvertAll(s => new SerializableBuffSkin(s));
            buffSkinList.Sort((a, b) => a.buffId.CompareTo(b.buffId));
            if (cardPool.actionDefineList.Count > 0)
                actionDefineList = cardPool.actionDefineList.ConvertAll(a => new SerializableActionDefine(a));
            else
                actionDefineList = new List<SerializableActionDefine>();
            actionDefineList.Sort((a, b) => a.id.CompareTo(b.id));
            animationDefineList = cardPool.animationDefineList.ConvertAll(a => new SerializableAnimationDefine(a));
            animationDefineList.Sort((a, b) => a.id.CompareTo(b.id));
            ruleLogicList = cardPool.ruleLogicList.ConvertAll(r => SerializeHelper.ToSerializable(r));
            ruleLogicList.Sort((a, b) => a.id.CompareTo(b.id));
            localizedStringList = cardPool.localizedStringList.Count(v => v != null) > 0 ?
                cardPool.localizedStringList.ConvertAll(s => new SerializableLocalizedString(s)) :
                null;
            localizedStringList?.Sort((a, b) => GBStringComparer.Default.Compare(a.value, b.value));
            localizedResourceList = cardPool.localizedStringList.Count(v => v != null) > 0 ?
                cardPool.localizedResourceList.ConvertAll(s => new SerializableLocalizedResource(s)) :
                null;
            localizedResourceList?.Sort((a, b) => GBStringComparer.Default.Compare(a.value, b.value));
        }
        #endregion
        public CardPool toCardPool(long cardPoolId, MainManager mainManager, TypeFinder typeFinder)
        {
            _genActionDefines = new List<GeneratedActionDefine>();
            _loadedEventDefines = new List<EventDefine>();
            _nodeDefiner = new THHNodeDefiner(
                (actRef, name) => getLoadingActionDefine(cardPoolId, mainManager, actRef, name),
                (eventRef) => getLoadingEventDefine(cardPoolId, mainManager, eventRef));
            getGeneratedActionDefines(cardPoolId, mainManager, typeFinder);

            var cardDefines = cardDefineList.ConvertAll(c =>
            {
                var card = c.toCardDefine(_nodeDefiner);
                card.cardPoolId = cardPoolId;
                foreach (var effect in card.getGeneratedEffects())
                {
                    effect.buffDefineRef = null;
                    effect.cardDefineRef = card.getDefineRef();
                }
                return card;
            });
            var buffDefines = buffDefineList.ConvertAll(b =>
            {
                var buff = b.toGeneratedBuffDefine(_nodeDefiner);
                buff.cardPoolId = cardPoolId;
                foreach (var effect in buff.effectList)
                {
                    effect.buffDefineRef = buff.getDefineRef();
                    effect.cardDefineRef = null;
                }
                return buff;
            });
            var animationDefines = animationDefineList?.ConvertAll(b => b.toAnimDefine()) ?? new List<GeneratedAnimationDefine>();

            var localizedStrings = localizedStringList?.ConvertAll(s => s.deserialize()) ?? new List<LocalizedString>();
            var localizedResources = localizedResourceList?.ConvertAll(r => r.deserialize()) ?? new List<LocalizedResource>();
            return new CardPool()
            {
                originLanguage = originLanguage,
                cardDefineList = cardDefines,
                buffDefineList = buffDefines,
                actionDefineList = new List<GeneratedActionDefine>(_genActionDefines),
                animationDefineList = animationDefines,
                ruleLogicList = ruleLogicList?.ConvertAll(r => r.deserialize(_nodeDefiner)) ?? new List<GeneratedRuleLogic>(),
                localizedStringList = localizedStrings,
                localizedResourceList = localizedResources,
                cardSkinList = cardSkinList.ConvertAll(s => s.toSkin()),
                buffSkinList = buffSkinList.ConvertAll(s => s.toSkin()),
                eventDefineList = new List<EventDefine>(_loadedEventDefines)
            };
        }
        private void getGeneratedActionDefines(long cardPoolId, MainManager main, TypeFinder typeFinder)
        {
            var defines = actionDefineList;
            // 铺设节点。
            foreach (var seri in defines)
            {
                var define = seri.toGeneratedActionDefine(typeFinder);
                define.cardPoolId = cardPoolId;
                _genActionDefines.Add(define);
                if (define.type == NodeDefineType.Event)
                {
                    _loadedEventDefines.Add(main.eventManager.generateEventDefineForActionDefine(define));
                }
            }
            // 定义所有节点。
            foreach (var define in _genActionDefines)
            {
                define.graph.DefineNodes(_nodeDefiner);
                define.graph.DefineGeneratedActionDefine(define);
            }
            // 连接所有节点。
            foreach (var define in _genActionDefines)
            {
                var graph = define.graph;
                var nodes = defines.FirstOrDefault(d => d.id == define.defineId).graph.GetConnections(graph);
                graph.AddConnections(nodes);
            }
        }
        private ActionDefine getLoadingActionDefine(long cardPoolId, MainManager mainManager, ActionReference actRef, string name)
        {
            var cardManager = mainManager.card;
            ActionDefine def = null;
            if (actRef != null)
            {
                if (actRef.cardPoolId == cardPoolId)
                {
                    def = _genActionDefines.FirstOrDefault(a => a.defineId == actRef.defineId);
                }
                else
                {
                    def = cardManager.getLocalActionDefine(actRef);
                }
            }
            else
            {
                actRef = cardManager.getBuiltinActionReferenceFromName(name);
                if (actRef != null)
                {
                    def = cardManager.getLocalActionDefine(actRef);
                }
                if (def == null)
                {
                    def = _genActionDefines.FirstOrDefault(a => a.editorName == name);
                }
            }
            return def;
        }
        private EventDefine getLoadingEventDefine(long curCardPoolId, MainManager mainManager, EventReference eventRef)
        {
            var cardManager = mainManager.card;
            if (eventRef.cardPoolId == curCardPoolId)
            {
                return _loadedEventDefines.FindEventDefine(eventRef);
            }
            else
            {
                return cardManager.getLocalEventDefine(eventRef);
            }
        }

        #endregion
        #region 属性字段
        [BsonIgnore]
        private INodeDefiner _nodeDefiner;
        [BsonIgnore]
        private List<GeneratedActionDefine> _genActionDefines;
        [BsonIgnore]
        private List<EventDefine> _loadedEventDefines;

        [Key(0)]
        public List<SerializableCardDefine> cardDefineList = new List<SerializableCardDefine>();
        [Key(1)]
        public List<SerializableCardSkinData> cardSkinList = new List<SerializableCardSkinData>();
        [Key(2)]
        public List<SerializableBuffDefine> buffDefineList = new List<SerializableBuffDefine>();
        [Key(3)]
        public List<SerializableBuffSkin> buffSkinList = new List<SerializableBuffSkin>();
        [Key(4)]
        public List<SerializableActionDefine> actionDefineList;
        [Key(5)]
        public List<SerializableAnimationDefine> animationDefineList;
        [Key(6)]
        public List<SerializableGeneratedRuleLogic> ruleLogicList;
        [Key(7)]
        public string originLanguage;
        [Key(8)]
        public List<SerializableLocalizedString> localizedStringList;
        [Key(9)]
        public List<SerializableLocalizedResource> localizedResourceList;
        [IgnoreMember]
        [Obsolete]
        public List<SerializableActionNodeGraphInfo> actionNodeGraphInfoList = new List<SerializableActionNodeGraphInfo>();
        #endregion
    }
}

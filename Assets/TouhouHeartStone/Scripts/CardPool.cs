using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Game.Animations;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game
{
    public class CardPool
    {
        public List<CardDefine> cardDefineList = new List<CardDefine>();
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<GeneratedBuffDefine> buffDefineList = new List<GeneratedBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<GeneratedActionDefine> actionDefineList = new List<GeneratedActionDefine>();
        public List<GeneratedAnimationDefine> animationDefineList = new List<GeneratedAnimationDefine>();
        public List<EventDefine> eventDefineList = new List<EventDefine>();
        public List<GeneratedRuleLogic> ruleLogicList = new List<GeneratedRuleLogic>();
    }
    [Serializable]
    public class SerializableCardPool
    {
        #region 公有方法
        #region 构造方法
        public SerializableCardPool(CardPool cardPool)
        {
            if (cardPool == null)
                throw new ArgumentNullException(nameof(cardPool));
            cardDefineList = cardPool.cardDefineList.ConvertAll(c => new SerializableCardDefine(c));
            cardSkinList = cardPool.cardSkinList.ConvertAll(s => new SerializableCardSkinData(s));
            buffDefineList = cardPool.buffDefineList.ConvertAll(b => new SerializableBuffDefine(b));
            buffSkinList = cardPool.buffSkinList.ConvertAll(s => new SerializableBuffSkin(s));
            if (cardPool.actionDefineList.Count > 0)
                actionDefineList = cardPool.actionDefineList.ConvertAll(a => new SerializableActionDefine(a));
            else
                actionDefineList = new List<SerializableActionDefine>();
            animationDefineList = cardPool.animationDefineList.ConvertAll(a => new SerializableAnimationDefine(a));
            ruleLogicList = cardPool.ruleLogicList.ConvertAll(r => r.serialize());
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

            var cardDefines = cardDefineList.ConvertAll(c => {
                var card = c.toCardDefine(_nodeDefiner);
                card.cardPoolId = cardPoolId;
                foreach (var effect in card.getGeneratedEffects())
                {
                    effect.buffDefineRef = null;
                    effect.cardDefineRef = card.getDefineRef();
                }
                return card;
            });
            var buffDefines = buffDefineList.ConvertAll(b => {
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

            return new CardPool()
            {
                cardDefineList = cardDefines,
                buffDefineList = buffDefines,
                actionDefineList = new List<GeneratedActionDefine>(_genActionDefines),
                animationDefineList = animationDefines,
                ruleLogicList = ruleLogicList?.ConvertAll(r => r.deserialize(_nodeDefiner)) ?? new List<GeneratedRuleLogic>(),
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

        public List<SerializableCardDefine> cardDefineList = new List<SerializableCardDefine>();
        public List<SerializableCardSkinData> cardSkinList = new List<SerializableCardSkinData>();
        public List<SerializableBuffDefine> buffDefineList = new List<SerializableBuffDefine>();
        public List<SerializableBuffSkin> buffSkinList = new List<SerializableBuffSkin>();
        public List<SerializableActionDefine> actionDefineList;
        public List<SerializableAnimationDefine> animationDefineList;
        public List<SerializableGeneratedRuleLogic> ruleLogicList;
        [Obsolete]
        public List<SerializableActionNodeGraphInfo> actionNodeGraphInfoList = new List<SerializableActionNodeGraphInfo>();
        #endregion
    }
}

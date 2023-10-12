using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Game.Animations;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
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
            cardSkinList = cardPool.cardSkinList;
            buffDefineList = cardPool.buffDefineList.ConvertAll(b => new SerializableBuffDefine(b));
            buffSkinList = cardPool.buffSkinList;
            if (cardPool.actionDefineList.Count > 0)
                actionDefineList = cardPool.actionDefineList.ConvertAll(a => new SerializableActionDefine(a));
            else
                actionDefineList = new List<SerializableActionDefine>();
            animationDefineList = cardPool.animationDefineList.ConvertAll(a => new SerializableAnimationDefine(a));
        }
        #endregion
        public CardPool toCardPool(long cardPoolId, MainManager mainManager, TypeFinder typeFinder)
        {
            _genActionDefines = new List<GeneratedActionDefine>();
            _loadedEventDefines = new List<EventDefine>();
            _nodeDefiner = new THHNodeDefiner(
                (actRef, name) => getLoadingActionDefine(cardPoolId, mainManager, actRef, name),
                (poolId, eventName) => getLoadingEventTriggerTime(cardPoolId, mainManager, poolId, eventName));
            getGeneratedActionDefines(cardPoolId, typeFinder);

            var cardDefines = cardDefineList.ConvertAll(c => c.toCardDefine(_nodeDefiner));
            var buffDefines = buffDefineList.ConvertAll(b => b.toGeneratedBuffDefine(_nodeDefiner));
            var animationDefines = animationDefineList?.ConvertAll(b => b.toAnimDefine()) ?? new List<GeneratedAnimationDefine>();

            return new CardPool()
            {
                cardDefineList = cardDefines,
                buffDefineList = buffDefines,
                actionDefineList = new List<GeneratedActionDefine>(_genActionDefines),
                animationDefineList = animationDefines,
                cardSkinList = cardSkinList,
                buffSkinList = buffSkinList,
                eventDefineList = new List<EventDefine>(_loadedEventDefines)
            };
        }
        private void getGeneratedActionDefines(long cardPoolId, TypeFinder typeFinder)
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
                    _loadedEventDefines.Add(EventManager.generateEventDefineForActionDefine(define));
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
                var nodes = defines.FirstOrDefault(d => d.id == define.id).graph.GetConnections(graph);
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
                    def = cardManager.getActionDefine(actRef);
                }
            }
            else
            {
                actRef = cardManager.getBuiltinActionReferenceFromName(name);
                if (actRef != null)
                {
                    def = cardManager.getActionDefine(actRef);
                }
                if (def == null)
                {
                    def = _genActionDefines.FirstOrDefault(a => a.editorName == name);
                }
            }
            return def;
        }
        private EventTriggerTime getLoadingEventTriggerTime(long curCardPoolId, MainManager mainManager, long cardPoolId, string name)
        {
            var cardManager = mainManager.card;
            if (cardPoolId == curCardPoolId)
            {
                return _loadedEventDefines.getEventTriggerTimeByName(name);
            }
            else
            {
                return cardManager.getLocalEventTriggerTime(cardPoolId, name);
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
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<SerializableBuffDefine> buffDefineList = new List<SerializableBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<SerializableActionDefine> actionDefineList;
        public List<SerializableAnimationDefine> animationDefineList;
        [Obsolete]
        public List<SerializableActionNodeGraphInfo> actionNodeGraphInfoList = new List<SerializableActionNodeGraphInfo>();
        #endregion
    }
}

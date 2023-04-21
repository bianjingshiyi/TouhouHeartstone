using System;
using UI;
using Controllers;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
using TouhouHeartstone;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class CardPool
    {
        public List<CardDefine> cardDefineList = new List<CardDefine>();
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<GeneratedBuffDefine> buffDefineList = new List<GeneratedBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<GeneratedActionDefine> actionDefineList = new List<GeneratedActionDefine>();
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
        }
        #endregion
        public CardPool toCardPool(ActionDefineFinder defineFinder, EventTypeInfoFinder eventFinder, TypeFinder typeFinder)
        {
            _genActionDefines = new List<GeneratedActionDefine>();
            _defineFinder = defineFinder;
            _nodeDefiner = new THHNodeDefiner(getActionDefine, eventFinder);
            getGeneratedActionDefines(typeFinder);

            var cardDefines = cardDefineList.ConvertAll(c => c.toCardDefine(_nodeDefiner));
            var buffDefines = buffDefineList.ConvertAll(b => b.toGeneratedBuffDefine(_nodeDefiner));

            return new CardPool()
            {
                cardDefineList = cardDefines,
                buffDefineList = buffDefines,
                actionDefineList = new List<GeneratedActionDefine>(_genActionDefines),
                cardSkinList = cardSkinList,
                buffSkinList = buffSkinList,
            };
        }
        private void getGeneratedActionDefines(TypeFinder typeFinder)
        {
            var defines = actionDefineList;
            // 铺设节点。
            foreach (var seri in defines)
            {
                var define = seri.toGeneratedActionDefine(typeFinder);
                _genActionDefines.Add(define);
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
        private ActionDefine getActionDefine(string name)
        {
            return _defineFinder?.Invoke(name) ?? _genActionDefines.FirstOrDefault(a => a.defineName == name || (a.obsoleteNames != null && a.obsoleteNames.Contains(name)));
        }

        #endregion
        #region 属性字段
        [BsonIgnore]
        private INodeDefiner _nodeDefiner;
        [BsonIgnore]
        private ActionDefineFinder _defineFinder;
        [BsonIgnore]
        private List<GeneratedActionDefine> _genActionDefines;

        public List<SerializableCardDefine> cardDefineList = new List<SerializableCardDefine>();
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<SerializableBuffDefine> buffDefineList = new List<SerializableBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<SerializableActionDefine> actionDefineList;
        [Obsolete]
        public List<SerializableActionNodeGraphInfo> actionNodeGraphInfoList = new List<SerializableActionNodeGraphInfo>();
        #endregion
    }
}

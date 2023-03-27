using System;
using UI;
using Controllers;
using TouhouCardEngine;
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
            var genActionDefines = SerializableActionDefine.toGeneratedActionDefines(actionDefineList, defineFinder, eventFinder, typeFinder);

            var cardDefines = cardDefineList.ConvertAll(c => c.toCardDefine(getActionDefine, eventFinder, typeFinder));
            var buffDefines = buffDefineList.ConvertAll(b => b.toGeneratedBuffDefine(getActionDefine, eventFinder, typeFinder));

            return new CardPool()
            {
                cardDefineList = cardDefines,
                buffDefineList = buffDefines,
                actionDefineList = new List<GeneratedActionDefine>(genActionDefines),
                cardSkinList = cardSkinList,
                buffSkinList = buffSkinList,
            };

            ActionDefine getActionDefine(string name)
            {
                return defineFinder?.Invoke(name) ?? genActionDefines.FirstOrDefault(a => a.defineName == name || (a.obsoleteNames != null && a.obsoleteNames.Contains(name)));
            }
        }
        #endregion
        #region 属性字段
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

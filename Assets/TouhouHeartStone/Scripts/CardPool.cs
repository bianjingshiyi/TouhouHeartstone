using System;
using UI;
using Controllers;
using TouhouCardEngine;
using System.Collections.Generic;
namespace Game
{
    [Serializable]
    public class CardPool
    {
        public List<CardDefine> cardDefineList = new List<CardDefine>();
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<ActionNodeGraphInfo> actionNodeGraphInfoList = new List<ActionNodeGraphInfo>();
        public List<GeneratedBuffDefine> buffDefineList = new List<GeneratedBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<SerializableActionDefine> actionDefineList = new List<SerializableActionDefine>();
        public List<GeneratedActionDefine> generatedActionDefineList = new List<GeneratedActionDefine>();
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
            actionNodeGraphInfoList = cardPool.actionNodeGraphInfoList;
            buffDefineList = cardPool.buffDefineList.ConvertAll(b => new SerializableBuffDefine(b));
            buffSkinList = cardPool.buffSkinList;
            if (cardPool.actionDefineList.Count > 0)
                actionDefineList = cardPool.actionDefineList;
            else if (cardPool.generatedActionDefineList.Count > 0)
                actionDefineList = cardPool.generatedActionDefineList.ConvertAll(a => new SerializableActionDefine(a));
            else
                actionDefineList = new List<SerializableActionDefine>();
        }
        #endregion
        public CardPool toCardPool(Func<string, Type> typeFinder)
        {
            return new CardPool()
            {
                cardDefineList = cardDefineList.ConvertAll(c => c.toCardDefine(typeFinder)),
                cardSkinList = cardSkinList,
                actionNodeGraphInfoList = actionNodeGraphInfoList,
                buffDefineList = buffDefineList.ConvertAll(b => b.toGeneratedBuffDefine(typeFinder)),
                buffSkinList = buffSkinList,
                generatedActionDefineList = actionDefineList.ConvertAll(a => a.toGeneratedActionDefine(typeFinder))
            };
        }
        #endregion
        #region 属性字段
        public List<SerializableCardDefine> cardDefineList = new List<SerializableCardDefine>();
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<ActionNodeGraphInfo> actionNodeGraphInfoList = new List<ActionNodeGraphInfo>();
        public List<SerializableBuffDefine> buffDefineList = new List<SerializableBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<SerializableActionDefine> actionDefineList;
        #endregion
    }
}

using MessagePack;
using System;
using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableCardDefine
    {
        #region 公有方法
        #region 构造方法
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableCardDefine()
        {
        }
        public SerializableCardDefine(CardDefine cardDefine)
        {
            id = cardDefine.id;
            type = cardDefine.type;
            foreach (string propName in cardDefine.getPropNames())
            {
                var prop = cardDefine.getProp<object>(propName);
                propDict.Add(propName, ObjectProxy.TryProxy(prop));
            }
            effects.AddRange(Array.ConvertAll(cardDefine.getGeneratedEffects(), e => SerializeHelper.ToSerializable(e)));
        }
        #endregion
        public CardDefine toCardDefine(INodeDefiner definer)
        {
            var newPropDict = new Dictionary<string, object>();
            foreach (var pair in propDict)
            {
                var prop = pair.Value;
                newPropDict.Add(pair.Key, ObjectProxy.TryConvertBack(prop));
            }
            CardDefine cardDefine = new CardDefine(id, type, newPropDict);
            GeneratedEffect[] effectsList = new GeneratedEffect[effects.Count];
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null)
                    continue;
                try
                {
                    effectsList[i] = effects[i].Deserialize(definer);
                }
                catch (Exception e)
                {
                    throw new FormatException($"反序列化卡牌定义{id}的效果{i}失败", e);
                }
            }
            cardDefine.setEffects(effectsList);
            return cardDefine;
        }
        #endregion
        #region 属性字段
        [Key(0)]
        public int id;
        [Key(1)]
        public string type;
        [Key(2)]
        public Dictionary<string, object> propDict = new Dictionary<string, object>();
        [Key(3)]
        public List<SerializableEffect> effects = new List<SerializableEffect>();
        [IgnoreMember]
        [Obsolete]
        public List<SerializableGeneratedEffect> effectList = new List<SerializableGeneratedEffect>();
        #endregion
    }
}

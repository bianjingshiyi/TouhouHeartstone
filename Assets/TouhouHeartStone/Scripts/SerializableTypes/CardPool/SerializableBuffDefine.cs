using MessagePack;
using System;
using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableBuffDefine
    {
        #region 公有方法
        #region 构造方法
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableBuffDefine()
        {
        }
        public SerializableBuffDefine(GeneratedBuffDefine buffDefine)
        {
            if (buffDefine == null)
                throw new ArgumentNullException(nameof(buffDefine));
            id = buffDefine.id;
            propModifierList = buffDefine.propModifierList != null
                ? buffDefine.propModifierList.ConvertAll(p => SerializeHelper.ToSerializable(p))
                : new List<SerializablePropModifier>();
            existLimitList = buffDefine.existLimitList != null
                ? buffDefine.existLimitList.ConvertAll(p => new SerializableBuffExistLimitDefine(p))
                : new List<SerializableBuffExistLimitDefine>();
            effects = buffDefine.effectList != null ?
                buffDefine.effectList.ConvertAll(e => SerializeHelper.ToSerializable(e)) :
                new List<SerializableEffect>();
            props = new Dictionary<string, object>();
            foreach (var propName in buffDefine.getPropNames())
            {
                props.Add(propName, buffDefine.getProp(propName));
            }
        }
        #endregion
        public GeneratedBuffDefine toGeneratedBuffDefine(INodeDefiner definer)
        {
            GeneratedBuffDefine generatedBuffDefine = new GeneratedBuffDefine(id, propModifierList.ConvertAll(p => p.deserialize()) ?? new List<PropModifier>());
            generatedBuffDefine.existLimitList = existLimitList?.ConvertAll(e => e.toDefine()) ?? new List<BuffExistLimitDefine>();
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null)
                    continue;
                try
                {
                    generatedBuffDefine.effectList.Add(effects[i].Deserialize(definer));
                }
                catch (Exception e)
                {
                    throw new FormatException($"反序列化增益定义{id}的效果{i}失败", e);
                }
            }
            if (props != null)
            {
                foreach (var pair in props)
                {
                    generatedBuffDefine.setProp(pair.Key, pair.Value);
                }
            }
            return generatedBuffDefine;
        }
        #endregion
        #region 属性字段
        [Key(0)]
        public int id;
        [Key(1)]
        public List<SerializablePropModifier> propModifierList;
        [Key(2)]
        public List<SerializableBuffExistLimitDefine> existLimitList;
        [Key(3)]
        public List<SerializableEffect> effects;
        [Key(4)]
        public Dictionary<string, object> props;
        [IgnoreMember]
        [Obsolete]
        public List<SerializableGeneratedEffect> effectList;
        #endregion
    }
}

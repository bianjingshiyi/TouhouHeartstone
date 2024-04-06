using MessagePack;
using System;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializablePortDefine
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializablePortDefine()
        {
        }
        public SerializablePortDefine(PortDefine portDefine)
        {
            typeName = portDefine.type.Name;
            name = portDefine.name;
            displayName = portDefine.displayName;
            isParams = portDefine.isParams;
        }
        public PortDefine ToPortDefine(TypeFinder typeFinder = null)
        {
            var type = !string.IsNullOrEmpty(typeName) && typeFinder != null ? typeFinder(typeName) : null;
            return new PortDefine(type, name, displayName, isParams);
        }
        [Key(0)]
        public string typeName;
        [Key(1)]
        public string name;
        [Key(2)]
        public string displayName;
        [Key(3)]
        public bool isParams;
    }
}

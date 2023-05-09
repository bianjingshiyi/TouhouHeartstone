using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Controllers
{
    public static class JsonHelper
    {
        public static void init()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(DefineReference)))
                BsonClassMap.RegisterClassMap<DefineReference>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(CardReference)))
                BsonClassMap.RegisterClassMap<CardReference>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(ObjectProxy)))
                BsonClassMap.RegisterClassMap<ObjectProxy>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(ActionValueRef)))
                BsonClassMap.RegisterClassMap<ActionValueRef>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableActionValueRef)))
                BsonClassMap.RegisterClassMap<SerializableActionValueRef>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableFunctionNode)))
                BsonClassMap.RegisterClassMap<SerializableFunctionNode>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(GeneratedEffect)))
                BsonClassMap.RegisterClassMap<GeneratedEffect>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(GeneratedAura)))
                BsonClassMap.RegisterClassMap<GeneratedAura>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(CompareOperator)))
                BsonClassMap.RegisterClassMap<CompareOperator>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(IntegerOperator)))
                BsonClassMap.RegisterClassMap<IntegerOperator>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(LogicOperator)))
                BsonClassMap.RegisterClassMap<LogicOperator>();

            // Nodes.
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableActionNode))) BsonClassMap.RegisterClassMap<SerializableActionNode>();

            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableActiveEntryNode))) BsonClassMap.RegisterClassMap<SerializableActiveEntryNode>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializablePassiveEntryNode))) BsonClassMap.RegisterClassMap<SerializablePassiveEntryNode>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableEventEntryNode))) BsonClassMap.RegisterClassMap<SerializableEventEntryNode>();

            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableGeneratedEntryNode))) BsonClassMap.RegisterClassMap<SerializableGeneratedEntryNode>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableGeneratedReturnNode))) BsonClassMap.RegisterClassMap<SerializableGeneratedReturnNode>();

            // Effects.
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableActiveEffect))) BsonClassMap.RegisterClassMap<SerializableActiveEffect>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializablePassiveEffect))) BsonClassMap.RegisterClassMap<SerializablePassiveEffect>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableEventEffect))) BsonClassMap.RegisterClassMap<SerializableEventEffect>();
            if (!BsonClassMap.IsClassMapRegistered(typeof(SerializableAuraEffect))) BsonClassMap.RegisterClassMap<SerializableAuraEffect>();
            isInited = true;
        }
        public static string toJson(this object obj, bool readFriendly = false)
        {
            ConventionRegistry.Register(nameof(CustomClassMapConvention), conventions, t => true);
            JsonWriterSettings setting = readFriendly ? readFriendlyWriterSettings : null;
            string json = obj.ToJson(setting);
            ConventionRegistry.Remove(nameof(CustomClassMapConvention));
            return json;
        }
        public static T fromJson<T>(string json)
        {
            ConventionRegistry.Register(nameof(CustomClassMapConvention), conventions, t => true);
            T obj = BsonSerializer.Deserialize<T>(json);
            ConventionRegistry.Remove(nameof(CustomClassMapConvention));
            return obj;
        }
        static ConventionPack conventions = new ConventionPack()
        {
            new CustomClassMapConvention()
        };
        public static bool isInited { get; private set; } = false;
        public static JsonWriterSettings readFriendlyWriterSettings = new JsonWriterSettings()
        {
            Indent = true
        };
    }
    /// <summary>
    /// 自定义类映射规则
    /// 只映射字段，忽略属性
    /// </summary>
    public class CustomClassMapConvention : IClassMapConvention
    {
        public string Name { get; } = "FieldOnlyClassMapConvention";
        public void Apply(BsonClassMap classMap)
        {
            //递归父类
            while (classMap != null)
            {
                BsonMemberMap[] memberMaps = classMap.DeclaredMemberMaps.ToArray();
                foreach (BsonMemberMap memberMap in memberMaps)
                {
                    //去掉所有属性
                    if (memberMap.MemberInfo.MemberType == MemberTypes.Property)
                        classMap.UnmapMember(memberMap.MemberInfo);
                }
                foreach (var fieldInfo in classMap.ClassType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f.GetCustomAttribute<BsonIgnoreAttribute>() == null && f.GetCustomAttribute<NonSerializedAttribute>() == null))
                {
                    //忽略所有带有忽略特性的字段
                    BsonMemberMap memberMap = classMap.MapMember(fieldInfo);
                    //如果为null或者默认值则忽略
                    if (fieldInfo.FieldType.IsValueType)
                    {
                        //值类型的默认值可以直接创建
                        memberMap.SetDefaultValue(Activator.CreateInstance(fieldInfo.FieldType));
                        memberMap.SetIgnoreIfDefault(true);
                    }
                    else
                    {
                        //引用类型的默认值为null
                        memberMap.SetIgnoreIfNull(true);
                    }
                }
                classMap = classMap.BaseClassMap;
            }
        }
    }
}

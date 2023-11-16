using Game;
using Game.PVE;
using Game.Replays;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using NitoriNetwork.Common;
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
            registerClass<ExecuteResult<string>>();
            registerClass<ExecuteResult<LobbyRoomData>>();
            registerClass<ExecuteResult<LobbyRoomData[]>>();
            registerClass<ExecuteResult<DeckDataItem[]>>(); 
            registerClass<ExecuteResult<ClientUpdateInfo>>();
            registerClass<ExecuteResult<PublicBasicUserInfo>>();
            registerClass<DefineReference>();
            registerClass<CardReference>();
            registerClass<EffectTag>();
            registerClass<EventArgReference>();
            registerClass<EventReferenceProxy>();
            registerClass<EventVarName>();
            registerClass<ObjectProxy>();
            registerClass<ActionValueRef>();
            registerClass<SerializableActionValueRef>();
            registerClass<SerializableFunctionNode>();
            registerClass<GeneratedEffect>();
            registerClass<GeneratedAura>();
            registerClass<CompareOperator>();
            registerClass<IntegerOperator>();
            registerClass<LogicOperator>();

            // Nodes.
            registerClass<SerializableActionNode>();

            registerClass<SerializableActiveEntryNode>();
            registerClass<SerializablePassiveEntryNode>();
            registerClass<SerializableEventEntryNode>();
            registerClass<SerializableAuraEntryNode>();

            registerClass<SerializableGeneratedEntryNode>();
            registerClass<SerializableGeneratedReturnNode>();

            // Effects.
            registerClass<SerializableActiveEffect>();
            registerClass<SerializablePassiveEffect>();
            registerClass<SerializableEventEffect>();
            registerClass<SerializableAuraEffect>();

            // Replays.
            registerClass<SerializableReplayMeta>();
            registerClass<SerializableReplayData>();
            registerClass<SerializableReplayPlayerData>();
            registerClass<SerializableHearthstoneOption>();

            // Responses.
            registerClass<Response>();
            registerClass<AttackResponse>();
            registerClass<DiscoverResponse>();
            registerClass<InitReplaceResponse>();
            registerClass<SurrenderResponse>();
            registerClass<TurnEndResponse>();
            registerClass<UseResponse>();

            // PVE.
            registerClass<SerializableSaveData>();
            registerClass<SerializableCardGem>();
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
        private static void registerClass<T>()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T))) BsonClassMap.RegisterClassMap<T>();
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

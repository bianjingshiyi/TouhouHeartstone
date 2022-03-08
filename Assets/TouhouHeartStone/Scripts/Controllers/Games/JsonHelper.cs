using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;

namespace Controllers
{
    public static class JsonHelper
    {
        public static void init()
        {
            ConventionRegistry.Register(nameof(CustomClassMapConvention), conventions, t => true);
            BsonClassMap.RegisterClassMap<CardReference>();
            isInited = true;
        }
        public static string toJson(this object obj)
        {
            return obj.ToJson();
        }
        public static T fromJson<T>(string json)
        {
            return BsonSerializer.Deserialize<T>(json);
        }
        static ConventionPack conventions = new ConventionPack()
        {
            new CustomClassMapConvention()
        };
        public static bool isInited { get; private set; } = false;
    }
    /// <summary>
    /// 自定义类映射规则
    /// 只映射字段，忽略属性
    /// 
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

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
        public static string toJson(this object obj)
        {
            ConventionRegistry.Register(nameof(CustomClassMapConvention), conventions, t => true);
            string json = obj.ToJson();
            ConventionRegistry.Remove(nameof(CustomClassMapConvention));
            return json;
        }
        public static T fromJson<T>(string json)
        {
            return BsonSerializer.Deserialize<T>(json);
        }
        static ConventionPack conventions = new ConventionPack()
        {
            new CustomClassMapConvention()
        };
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
                    memberMap.SetIgnoreIfDefault(true);
                    memberMap.SetIgnoreIfNull(true);
                }
                classMap = classMap.BaseClassMap;
            }
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;
using System.Collections.Generic;
using MongoDB.Bson;
using ILogger = TouhouCardEngine.Shared.ILogger;
namespace TouhouHeartstone
{
    public class CardHelper
    {
        public static CardDefine[] getCardDefines(ILogger logger)
        {
            return getCardDefines(AppDomain.CurrentDomain.GetAssemblies(), null, logger);
        }
        public static CardDefine[] getCardDefines(Assembly[] assemblies, string[] namespaces, ILogger logger)
        {
            List<CardDefine> cardList = new List<CardDefine>();
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(CardDefine)) &&
                    !t.IsAbstract &&
                    t != typeof(GeneratedCardDefine) && 
                    t.GetCustomAttribute<IgnoreCardDefineAttribute>() == null))
                {
                    if (namespaces != null && !namespaces.Contains(type.Namespace))
                    {
                        UberDebug.LogChannel("Card", $"{type.Namespace}不包含在[{string.Join(",", namespaces)}]中，被忽略");
                        continue;
                    }
                    ConstructorInfo constructor = type.GetConstructor(new Type[0]);
                    if (constructor != null)
                    {
                        if (constructor.Invoke(new object[0]) is CardDefine define)
                        {
                            try
                            {
                                cardList.Add(define);
                            }
                            catch (Exception e)
                            {
                                if (e.InnerException is NotImplementedException)
                                {
                                    if (logger == null)
                                        UberDebug.LogWarningChannel("Load", $"忽略尚未完成的卡片{define}");
                                    else
                                        logger.logWarn("Load", $"忽略尚未完成的卡片{define}");
                                }
                                else
                                    throw e;
                            }
                        }
                    }
                }
            }
            //把内置数据放入卡牌定义的动态词典中
            foreach (var define in cardList)
            {
                if (define is THHCardDefine d)
                {
                    d.setProp(DEFINE.COST, d.cost);
                    d.setProp(DEFINE.LIFE, d.life);
                    d.setProp(DEFINE.ATTACK, d.attack);
                    d.setProp(DEFINE.TAGS, d.tags);
                    d.setProp(DEFINE.KEYWORDS, d.keywords);
                    d.setProp(DEFINE.ISTOKEN, d.isToken);
                }
            }
            return cardList.ToArray();
        }
    }
}
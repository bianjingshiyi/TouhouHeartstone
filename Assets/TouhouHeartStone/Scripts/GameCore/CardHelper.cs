using System;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;
using System.Collections.Generic;
using MongoDB.Bson;
using ILogger = TouhouCardEngine.Interfaces.ILogger;
namespace TouhouHeartstone
{
    public class CardHelper
    {
        public static CardDefine[] getCardDefines(ILogger logger)
        {
            return getCardDefines(AppDomain.CurrentDomain.GetAssemblies(), logger);
        }
        public static CardDefine[] getCardDefines(Assembly[] assemblies, ILogger logger)
        {
            List<CardDefine> cardList = new List<CardDefine>();
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(CardDefine)) && !t.IsAbstract && t != typeof(GeneratedCardDefine)))
                {
                    ConstructorInfo constructor = type.GetConstructor(new Type[0]);
                    if (constructor != null)
                    {
                        if (constructor.Invoke(new object[0]) is CardDefine define)
                        {
                            try
                            {
                                if (logger == null)
                                    UberDebug.LogChannel("Load", "加载内置卡片" + define.ToJson());
                                else
                                    logger.log("Load", "加载内置卡片" + define.ToJson());
                                cardList.Add(define);
                            }
                            catch (Exception e)
                            {
                                if (e.InnerException is NotImplementedException)
                                {
                                    if (logger == null)
                                        UberDebug.LogWarningChannel("Load", "忽略尚未完成的卡片" + define);
                                    else
                                        logger.log("Load", "忽略尚未完成的卡片" + define);
                                }
                                else
                                    throw e;
                            }
                        }
                    }
                }
            }
            return cardList.ToArray();
        }
    }
}
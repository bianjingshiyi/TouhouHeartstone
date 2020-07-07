using System;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;
using System.Collections.Generic;
using ExcelLibrary.SpreadSheet;
using UnityEngine;
using MongoDB.Bson;
namespace TouhouHeartstone
{
    public class CardHelper
    {
        public static CardDefine[] getCardDefines()
        {
            return getCardDefines(AppDomain.CurrentDomain.GetAssemblies());
        }
        public static CardDefine[] getCardDefines(Assembly[] assemblies)
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
                                UberDebug.LogChannel("Load", "加载内置卡片" + define.ToJson());
                                cardList.Add(define);
                            }
                            catch (Exception e)
                            {
                                if (e.InnerException is NotImplementedException)
                                {
                                    UberDebug.LogWarningChannel("Load", "忽略尚未完成的卡片" + define);
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
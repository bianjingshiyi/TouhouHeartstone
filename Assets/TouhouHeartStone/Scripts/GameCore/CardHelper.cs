using System;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;
using System.Collections.Generic;
using ExcelLibrary.SpreadSheet;
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
                            cardList.Add(define);
                    }
                }
            }
            return cardList.ToArray();
        }
    }
}
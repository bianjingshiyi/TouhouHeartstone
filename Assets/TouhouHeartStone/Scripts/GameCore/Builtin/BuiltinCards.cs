using System;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;
using System.Collections.Generic;

namespace TouhouHeartstone.Builtin
{
    public class BuiltinCards
    {
        public static CardDefine[] getCardDefines()
        {
            List<CardDefine> cardList = new List<CardDefine>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(CardDefine)) && !t.IsAbstract))
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
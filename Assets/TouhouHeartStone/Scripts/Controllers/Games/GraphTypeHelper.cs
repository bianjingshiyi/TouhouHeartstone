using System;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;

namespace Game
{
    public static class GraphTypeHelper
    {
        public static void init()
        {
            Flow.addConversion(new CardDefineConversion());
            Flow.addConversion(new NodeConstProxyConversion());
            Flow.addConversion(new CollectionConversion());
        }
    }

    public class CardDefineConversion : ParamConversion
    {
        public override bool canConvert(Type from, Type to)
        {
            return typeof(DefineReference).IsAssignableFrom(from) && typeof(CardDefine).IsAssignableFrom(to);
        }
        public override object convert(Flow flow, object input, Type to)
        {
            if (input is DefineReference defRef)
            {
                if (flow.env.game is CardEngine game)
                {
                    return game.getDefine(defRef);
                }
            }
            return null;
        }
    }

    public class NodeConstProxyConversion : ParamConversion
    {
        public override bool canConvert(Type from, Type to)
        {
            Type fromUnpacked = from;
            Type toUnpacked = to;
            if (IsSubclassOfRawGeneric(typeof(NodeConstProxy<>), from, out var fromGenArgs))
            {
                fromUnpacked = fromGenArgs[0];
            }
            if (IsSubclassOfRawGeneric(typeof(NodeConstProxy<>), to, out var toGenArgs))
            {
                toUnpacked = toGenArgs[0];
            }
            return toUnpacked.IsAssignableFrom(fromUnpacked);
        }
        public override object convert(Flow flow, object input, Type to)
        {
            if (input != null)
            {
                var constructor = to.GetConstructor(new Type[] { input.GetType() });
                if (constructor != null)
                {
                    return constructor.Invoke(new object[1] { input });
                }
            }
            return input;
        }
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck, out Type[] genericArgs)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    genericArgs = toCheck.GetGenericArguments();
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            genericArgs = null;
            return false;
        }
    }
    public class CollectionConversion : ParamConversion
    {
        public override bool canConvert(Type from, Type to)
        {
            if (typeof(Array).IsAssignableFrom(to))
            {
                return typeof(PileNameCollection).IsAssignableFrom(from) ||
                    typeof(KeywordCollection).IsAssignableFrom(from) ||
                    typeof(CardReferenceCollection).IsAssignableFrom(from);
            }
            else if (typeof(string).IsAssignableFrom(to))
            {
                return typeof(PileNameCollection).IsAssignableFrom(from) ||
                    typeof(KeywordCollection).IsAssignableFrom(from);
            }
            else if (typeof(DefineReference).IsAssignableFrom(to) || typeof(CardDefine).IsAssignableFrom(to))
            {
                return typeof(CardReferenceCollection).IsAssignableFrom(from);
            }
            return false;
        }
        public override object convert(Flow flow, object input, Type to)
        {
            if (typeof(Array).IsAssignableFrom(to))
            {
                switch (input)
                {
                    case PileNameCollection pileNames:
                        return pileNames.ToArray();
                    case KeywordCollection keywords:
                        return keywords.ToArray();
                    case CardReferenceCollection cardReferences:
                        return cardReferences.ToArray();
                }
            }
            else if (typeof(string).IsAssignableFrom(to))
            {
                switch (input)
                {
                    case PileNameCollection pileNames:
                        return pileNames.FirstOrDefault();
                    case KeywordCollection keywords:
                        return keywords.FirstOrDefault();
                }
            }
            else if (typeof(DefineReference).IsAssignableFrom(to))
            {
                switch (input)
                {
                    case CardReferenceCollection cardReferences:
                        return cardReferences.FirstOrDefault();
                }
            }
            else if (typeof(CardDefine).IsAssignableFrom(to))
            {
                switch (input)
                {
                    case CardReferenceCollection cardReferences:
                        if (flow.env.game is CardEngine game)
                        {
                            return game.getDefine(cardReferences.FirstOrDefault());
                        }
                        return null;
                }
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    public static class GraphTypeHelper
    {
        public static void init()
        {
            Flow.addConversion(new CardDefineConversion());
            Flow.addConversion(new BuffDefineConversion());
            Flow.addConversion(new EventReferenceConversion());
            Flow.addConversion(new NodeConstProxyConversion());
            Flow.addConversion(new CollectionConversion());
        }
        public readonly static IEqualityComparer<object> graphEqualityComparer = new GraphEqualityComparer();
    }
    public class GraphEqualityComparer : IEqualityComparer<object>
    {
        public GraphEqualityComparer(Func<object, object> selector = null)
        {
            _selector = selector;
        }
        public new bool Equals(object x, object y)
        {
            if (_selector != null)
            {
                x = _selector(x);
                y = _selector(y);
            }
            if (x == null)
                return y == null;
            if (GetSpecific2Types(x, y, out BuffDefine buffDefine, out BuffReference buffRef))
            {
                return buffDefine.getDefineRef() == buffRef;
            }
            else if (GetSpecific2Types(x, y, out CardDefine cardDefine, out DefineReference defineRef))
            {
                return cardDefine.getDefineRef() == defineRef;
            }
            return x.Equals(y);
        }

        public int GetHashCode(object obj)
        {
            if (_selector != null)
                obj = _selector(obj);
            if (obj is CardDefine cardDefine)
            {
                return cardDefine.getDefineRef().GetHashCode();
            }
            else if (obj is BuffDefine buffDefine)
            {
                return buffDefine.getDefineRef().GetHashCode();
            }
            return obj.GetHashCode();
        }
        private bool GetSpecific2Types<T1, T2>(object x, object y, out T1 t1, out T2 t2)
        {
            if (x is T1 t1x && y is T2 t2y)
            {
                t1 = t1x;
                t2 = t2y;
                return true;
            }

            if (y is T1 t1y && x is T2 t2x)
            {
                t1 = t1y;
                t2 = t2x;
                return true;
            }

            t1 = default;
            t2 = default;
            return false;
        }
        private readonly Func<object, object> _selector;
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
    public class BuffDefineConversion : ParamConversion
    {
        public override bool canConvert(Type from, Type to)
        {
            return typeof(DefineReference).IsAssignableFrom(from) && typeof(BuffDefine).IsAssignableFrom(to);
        }
        public override object convert(Flow flow, object input, Type to)
        {
            if (input is DefineReference defRef)
            {
                if (flow.env.game is CardEngine game)
                {
                    return game.getBuffDefine(defRef);
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
            if (from.HasInterfaceOfRawGeneric(typeof(INodeConstProxy<>), out var fromGenArgs))
            {
                fromUnpacked = fromGenArgs[0];
            }
            if (to.HasInterfaceOfRawGeneric(typeof(INodeConstProxy<>), out var toGenArgs))
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
    }
    public class EventReferenceConversion : ParamConversion
    {
        public override bool canConvert(Type from, Type to)
        {
            return typeof(EventReference).IsAssignableFrom(to) && (typeof(string).IsAssignableFrom(from) || typeof(EventArgReference).IsAssignableFrom(from));
        }
        public override object convert(Flow flow, object input, Type to)
        {
            if (input is string str)
            {
                return new EventReference(0, str);
            }
            else if (input is EventArgReference eventArgRef)
            {
                return new EventReference(0, eventArgRef.value);
            }
            return input;
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
                    typeof(CardTagCollection).IsAssignableFrom(from) ||
                    typeof(CardReferenceCollection).IsAssignableFrom(from);
            }
            else if (typeof(string).IsAssignableFrom(to))
            {
                return typeof(PileNameCollection).IsAssignableFrom(from) ||
                    typeof(KeywordCollection).IsAssignableFrom(from) ||
                    typeof(CardTagCollection).IsAssignableFrom(from);
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
                    case CardTagCollection cardTags:
                        return cardTags.ToArray();
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
                    case CardTagCollection cardTags:
                        return cardTags.FirstOrDefault();
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

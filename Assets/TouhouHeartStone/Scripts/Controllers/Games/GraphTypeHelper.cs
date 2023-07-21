using System;
using System.Linq;
using TouhouCardEngine;

namespace Game
{
    public static class GraphTypeHelper
    {
        public static void init()
        {
            Flow.addConversion(new CardDefineConversion());
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

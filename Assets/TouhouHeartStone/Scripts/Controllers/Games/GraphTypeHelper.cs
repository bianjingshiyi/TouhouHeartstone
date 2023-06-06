using System;
using TouhouCardEngine;

namespace Game
{
    public static class GraphTypeHelper
    {
        public static void init()
        {
            Flow.addConversion(new CardDefineConversion());
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
            return false;
        }
    }
}

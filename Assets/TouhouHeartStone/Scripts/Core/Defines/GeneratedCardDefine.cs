using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    public class GeneratedCardDefine : CardDefine
    {
        Dictionary<string, object> dicProp { get; } = new Dictionary<string, object>();
        public void setProp<T>(string propName, T value)
        {
            dicProp[propName] = value;
        }
        public override T getProp<T>(string propName)
        {
            if (dicProp.ContainsKey(propName) && dicProp[propName] is T)
                return (T)dicProp[propName];
            else
                return default;
        }
        public override int id
        {
            get { return getProp<int>("id"); }
        }
        public override CardDefineType type
        {
            get { return (CardDefineType)getProp<int>("type"); }
        }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            string script = getProp<string>("condition");
            if (string.IsNullOrEmpty(script))
                return null;
            return engine.runFunc<string>(script, new EffectGlobals() { engine = engine, player = player, card = card, targetCards = null });
        }
        public override Effect[] effects
        {
            get { return getProp<Effect[]>("effects"); }
        }
    }
}
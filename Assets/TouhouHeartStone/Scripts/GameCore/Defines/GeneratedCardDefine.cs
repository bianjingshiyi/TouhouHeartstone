using System;
using System.Collections.Generic;

using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone
{
    public class GeneratedCardDefine : CardDefine
    {
        public override int id { get; set; } = 0;
        Dictionary<string, object> dicProp { get; } = new Dictionary<string, object>();
        public override void setProp<T>(string propName, T value)
        {
            if (propName == nameof(CardDefine.id))
                id = (int)(object)value;
            dicProp[propName] = value;
        }
        public override T getProp<T>(string propName)
        {
            if (dicProp.ContainsKey(propName) && dicProp[propName] is T)
                return (T)dicProp[propName];
            else
                return default;
        }
        public override CardDefineType type { get; set; }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            string script = getProp<string>("condition");
            if (string.IsNullOrEmpty(script))
                return null;
            return engine.runFunc<string>(script, new EffectGlobals() { engine = engine, player = player, card = card, targetCards = null });
        }
        public override IEffect[] effects
        {
            get { return getProp<Effect[]>(nameof(CardDefine.effects)); }
            set { setProp(nameof(CardDefine.effects), value); }
        }
    }
}
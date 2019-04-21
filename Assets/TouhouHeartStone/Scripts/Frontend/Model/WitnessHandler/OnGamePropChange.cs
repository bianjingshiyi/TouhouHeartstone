using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnGamePropChange : WitnessHandler
    {
        public override string Name => "onGamePropChange";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var propName = witness.getVar<string>("propName");
            var changeType = witness.getVar<PropertyChangeType>("changeType");
            var val = witness.getVar<object>("value");

            DebugUtils.LogNoImpl($"将{propName}设置为{val}的方法未定义.");
            callback?.Invoke(this, null);
            return false;
        }
    }
}

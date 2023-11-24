using System.Collections;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = SetEventVarEventArg;
    class SetEventVarAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var eventArg = record.getVar<IEventArg>(Arg.VAR_EVENT);
            var varName = record.getVar<string>(Arg.VAR_VAR_NAME);
            var after = record.getVar<object>(Arg.VAR_AFTER_VALUE);

            var anim = table.getAnimOfEvent(eventArg);
            if (anim != null)
            {
                while (anim.isVarChanged())
                {
                    yield return null;
                }
                anim.setVar(varName, after);
            }
            yield break;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            return true;
        }
    }
}

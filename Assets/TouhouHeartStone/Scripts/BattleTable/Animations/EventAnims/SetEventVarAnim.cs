using System.Collections;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = SetEventVarEventDefine;
    class SetEventVarAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var eventArg = base.eventArg.getVar<IEventArg>(Arg.VAR_EVENT);
            var varName = base.eventArg.getVar<string>(Arg.VAR_VAR_NAME);
            var after = base.eventArg.getVar<object>(Arg.VAR_AFTER_VALUE);

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

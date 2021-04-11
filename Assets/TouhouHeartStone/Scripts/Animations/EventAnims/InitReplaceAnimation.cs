using TouhouHeartstone;
using UI;
namespace Game
{
    class InitReplaceAnimation : EventAnimation<HearthStoneRule.InitReplaceEventArg>
    {
        public override bool update(TableManager table, HearthStoneRule.InitReplaceEventArg eventArg, float time)
        {
            table.ui.InitReplaceDialog.hide();
            return true;
        }
    }
}

using TouhouHeartstone;
using UI;
namespace Game
{
    class InitReplaceAnimation : EventAnimation<THHPlayer.InitReplaceEventArg>
    {
        public override bool update(TableManager table, THHPlayer.InitReplaceEventArg eventArg)
        {
            table.ui.InitReplaceDialog.hide();
            return true;
        }
    }
}

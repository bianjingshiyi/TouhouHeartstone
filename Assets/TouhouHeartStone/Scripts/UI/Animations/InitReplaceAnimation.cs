using TouhouHeartstone;
namespace UI
{
    class InitReplaceAnimation : Animation<THHPlayer.InitReplaceEventArg>
    {
        public InitReplaceAnimation(THHPlayer.InitReplaceEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            return true;
        }
    }
}

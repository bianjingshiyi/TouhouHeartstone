using TouhouHeartstone;
namespace UI
{
    class InitAnimation : Animation<THHGame.InitEventArg>
    {
        public InitAnimation(THHGame.InitEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            return true;
        }
    }
}

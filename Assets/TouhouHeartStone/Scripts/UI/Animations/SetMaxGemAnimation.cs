using TouhouHeartstone;
namespace UI
{
    class SetMaxGemAnimation : Animation<THHPlayer.SetMaxGemEventArg>
    {
        public SetMaxGemAnimation(THHPlayer.SetMaxGemEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            return true;
        }
    }
}

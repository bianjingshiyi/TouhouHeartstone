using TouhouHeartstone;
namespace UI
{
    class SetGemAnimation : Animation<THHPlayer.SetGemEventArg>
    {
        public SetGemAnimation(THHPlayer.SetGemEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            if (eventArg.player == table.player)
            {
                table.SelfGem.Text.text = eventArg.value.ToString();
            }
            else
            {
                table.EnemyGem.Text.text = eventArg.value.ToString();
            }
            return true;
        }
    }
}

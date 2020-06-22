using TouhouHeartstone;
using UI;
namespace Game
{
    class GemChangeAnimation : EventAnimation<THHPlayer.SetGemEventArg>
    {
        public override bool update(TableManager table, THHPlayer.SetGemEventArg eventArg)
        {
            if (eventArg.player == table.player)
                table.ui.SelfGem.Text.text = eventArg.player.gem.ToString();
            else
                table.ui.EnemyGem.Text.text = eventArg.player.gem.ToString();
            return true;
        }
    }
}

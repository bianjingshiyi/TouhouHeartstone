using TouhouHeartstone;
using UI;
namespace Game
{
    class GemChangeAnimation : EventAnimation<THHPlayer.SetGemEventArg>
    {
        public override bool update(TableManager table, THHPlayer.SetGemEventArg eventArg, float time)
        {
            if (eventArg.player == table.player)
                table.ui.SelfGem.Text.text = eventArg.player.getGem().ToString();
            else
                table.ui.EnemyGem.Text.text = eventArg.player.getGem().ToString();
            table.setSkill(table.ui.SelfSkill, table.player.getSkill());
            return true;
        }
    }
}

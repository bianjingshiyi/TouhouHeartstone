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
            table.SelfMaster.update(table, table.player, table.player.master, table.getSkin(table.player.master));
            THHPlayer opponent = table.game.getOpponent(table.player);
            table.EnemyMaster.update(table, opponent, opponent.master, table.getSkin(opponent.master));
            return true;
        }
    }
}

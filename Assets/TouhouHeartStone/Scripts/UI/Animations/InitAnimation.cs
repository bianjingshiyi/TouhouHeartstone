using TouhouHeartstone;
using BJSYGameCore;
using UI;
namespace Game
{
    class InitAnimation : UIAnimation<THHGame.InitEventArg>
    {
        public InitAnimation(THHGame.InitEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            TableManager manager = table.findInstance<TableManager>();
            table.SelfMaster.update(table, manager.player, manager.player.master, manager.getManager<CardManager>().getSkin(manager.player.master));
            table.SelfSkill.update(table, manager.player, manager.player, manager.player.skill, manager.getManager<CardManager>().getSkin(manager.player.skill));
            THHPlayer opponent = manager.game.getOpponent(manager.player);
            table.EnemyMaster.update(table, opponent, opponent.master, manager.getManager<CardManager>().getSkin(opponent.master));
            table.EnemySkill.update(table, manager.player, opponent, opponent.skill, manager.getManager<CardManager>().getSkin(opponent.skill));
            return true;
        }
    }
}

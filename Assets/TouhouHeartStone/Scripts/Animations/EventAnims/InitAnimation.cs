using Game;
using TouhouHeartstone;
using System;
using UI;
namespace Game
{
    class InitAnimation : EventAnimation<THHGame.InitEventArg>
    {
        public override bool update(TableManager table, THHGame.InitEventArg eventArg)
        {
            table.setMaster(table.ui.SelfMaster, table.player.master);
            table.setSkill(table.ui.SelfSkill, table.player.skill);
            THHPlayer opponent = table.game.getOpponent(table.player);
            table.setMaster(table.ui.EnemyMaster, opponent.master);
            table.setSkill(table.ui.EnemySkill, opponent.skill);
            return true;
        }
    }
}

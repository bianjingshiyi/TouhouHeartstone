using Game;
using TouhouHeartstone;
using System;
using UI;
namespace Game
{
    class InitAnimation : EventAnimation<THHGame.InitEventArg>
    {
        public override bool update(TableManager table, THHGame.InitEventArg eventArg, float time)
        {
            table.setMaster(table.ui.SelfMaster, table.player.getMaster());
            table.setSkill(table.ui.SelfSkill, table.player.getSkill());
            THHPlayer opponent = table.game.getOpponent(table.player);
            table.setMaster(table.ui.EnemyMaster, opponent.getMaster());
            table.setSkill(table.ui.EnemySkill, opponent.getSkill());
            return true;
        }
    }
}

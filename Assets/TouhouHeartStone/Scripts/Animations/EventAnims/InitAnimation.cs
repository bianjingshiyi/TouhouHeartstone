using Game;
using TouhouHeartstone;
using System;
using UI;
namespace Game
{
    class InitAnimation : EventAnimation<HearthStoneRule.InitEventArg>
    {
        public override bool update(TableManager table, HearthStoneRule.InitEventArg eventArg, float time)
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

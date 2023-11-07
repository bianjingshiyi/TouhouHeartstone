using System.Collections;

namespace Game.BattleTable
{
    class ClearDeadAnchorAnimation : TableAnimation
    {
        public override IEnumerator update(BattleTableManager table)
        {
            table.clearDeadCardAnchors();
            yield break;
        }
    }
}

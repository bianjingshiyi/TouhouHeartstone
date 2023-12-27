using System.Collections;
using TouhouCardEngine;
using UnityEngine;

namespace Game.BattleTable
{
    class MoveToNullAnim : CardMoveAnim
    {
        public MoveToNullAnim(CardDescriptor cardDesc, Vector3 startTablePos, Pile fromPile, Pile toPile, bool startVisible) : base(cardDesc, startTablePos, fromPile, toPile, startVisible)
        {
        }
        public override IEnumerator startUpdate(BattleTableManager table, float deltaTime)
        {
            yield break;
        }
        public override IEnumerator logicUpdate(BattleTableManager table, float deltaTime)
        {
            removeCard(table);
            yield break;
        }
        public override Vector3 getCurrentPosition(BattleTableManager table)
        {
            return table.WorldToTablePosition(table.table.CenterDisplayTransform.position);
        }
    }
}

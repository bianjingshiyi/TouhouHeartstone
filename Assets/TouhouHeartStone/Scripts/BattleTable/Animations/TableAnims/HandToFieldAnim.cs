using System.Collections;
using TouhouCardEngine;
using UnityEngine;

namespace Game.BattleTable
{
    class HandToFieldAnim : TableAnimation
    {
        public HandToFieldAnim(CardDescriptor servantDesc, Vector3 startPosition, Pile fromPile, Pile toPile, int index)
        {
            _cardDesc = servantDesc;
            _anim = new MoveToFieldAnim(servantDesc, startPosition, fromPile, toPile, index, -1);
        }
        public override IEnumerator update(BattleTableManager table)
        {
            table.removeHand(_cardDesc.id);
            while (!_anim.update(table, _deltaTime))
            {
                yield return null;
            }
        }
        public void forceFinish(BattleTableManager table)
        {
            _anim.forceFinish(table);
            finish(table);
        }
        CardDescriptor _cardDesc;
        MoveToFieldAnim _anim;
    }
}

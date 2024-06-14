using System.Collections;
using TouhouCardEngine;
using UnityEngine;

namespace Game.BattleTable
{
    class HandToFieldAnim : TableAnimation
    {
        public HandToFieldAnim(CardDescriptor servantDesc, Vector3 startPosition, Pile fromPile, Pile toPile, int index, bool startVisible)
        {
            _cardDesc = servantDesc;
            _anim = new MoveToFieldAnim(servantDesc, startPosition, fromPile, toPile, index, -1, startVisible, false);
        }
        public override IEnumerator update(BattleTableManager table)
        {
            if (table.tryGetHand(_cardDesc.id, out var hand))
            {
                _anim.startPosition = table.WorldToTablePosition(hand.transform.position);
            }
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

using System.Collections;
using TouhouCardEngine;
using UnityEngine;

namespace Game.BattleTable
{
    class MoveToItemAnim : CardMoveAnim
    {
        public MoveToItemAnim(CardDescriptor cardDesc, Vector3 startTablePos, Pile fromPile, Pile toPile) : base(cardDesc, startTablePos, fromPile, toPile)
        {
        }
        public override IEnumerator startUpdate(BattleTableManager table, float deltaTime)
        {
            yield break;
        }
        public override IEnumerator logicUpdate(BattleTableManager table, float deltaTime)
        {
            // 创建道具。
            var item = table.getItem(_to);
            item.setExist(true);
            item.setDeath(false);
            table.setItem(item, _cardDesc);

            // 播放入场动画。
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, item.onEquip, item.animator, _deltaTime))
            {
                yield return null;
            }
        }
        public override Vector3 getCurrentPosition(BattleTableManager table)
        {
            if (table.tryGetItem(_cardDesc.id, out var item))
                return table.WorldToTablePosition(item.transform.position);
            var transform = table.getCardTransform(_cardDesc.id);
            if (transform)
                return table.WorldToTablePosition(transform.position);
            return _startTablePos;
        }
    }
}

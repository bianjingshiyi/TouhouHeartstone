using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    class MoveToHeadAnim : CardMoveAnim
    {
        public MoveToHeadAnim(CardDescriptor cardDesc, Vector3 startTablePos, Pile fromPile, Pile toPile, int position) : base(cardDesc, startTablePos, fromPile, toPile)
        {
        }
        public override IEnumerator startUpdate(BattleTableManager table, float deltaTime)
        {
            yield break;
        }
        public override IEnumerator logicUpdate(BattleTableManager table, float deltaTime)
        {
            // 创建延迟区物件。
            if (!table.tryGetSecret(_cardDesc.id, out var secret))
                secret = table.createSecret(_cardDesc);
            table.setSecret(secret, _cardDesc);

            if (_from == _to)
            {
                var sourceHead = secret.headSlots;
                var targetHead = table.getHeadSlots(_to);
                if (sourceHead == targetHead)
                {
                    if (sourceHead.removeItem(secret))
                    {
                        targetHead.insertItem(_position, secret);
                    }
                }
            }

            // 播放入场动画。
            SimpleAnim enterAnim = _cardDesc.define.hasKeyword(KEYWORD.MISSION) ? secret.onMissionEnter : secret.onSecretEnter;
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, enterAnim, secret.animator, _deltaTime))
                yield return null;
        }
        public override Vector3 getCurrentPosition(BattleTableManager table)
        {
            if (table.tryGetSecret(_cardDesc.id, out var secret))
                return table.WorldToTablePosition(secret.transform.position);
            var transform = table.getCardTransform(_cardDesc.id);
            if (transform)
                return table.WorldToTablePosition(transform.position);
            return _startTablePos;
        }
        private int _position;
    }
}

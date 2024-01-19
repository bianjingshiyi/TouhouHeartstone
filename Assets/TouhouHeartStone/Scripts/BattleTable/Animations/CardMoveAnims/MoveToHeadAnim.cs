using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    class MoveToHeadAnim : CardMoveAnim
    {
        public MoveToHeadAnim(CardDescriptor cardDesc, Vector3 startTablePos, Pile fromPile, Pile toPile, int position, bool startVisible) : base(cardDesc, startTablePos, fromPile, toPile, startVisible)
        {
            _position = position;
        }
        public override IEnumerator startUpdate(BattleTableManager table, float deltaTime)
        {
            yield break;
        }
        public override IEnumerator logicUpdate(BattleTableManager table, float deltaTime)
        {
            Secret secret;
            // 夺取陷阱，将原先的卡牌移除。
            if (_from != _to)
            {
                removeCard(table);
                secret = table.createSecret(_cardDesc);
            }
            else // 同牌堆间移动，调整位置。
            {
                // 创建延迟区物件。
                if (!table.tryGetSecret(_cardDesc.id, out secret))
                {
                    secret = table.createSecret(_cardDesc);
                }
                else
                {
                    var head = secret.headSlots;
                    if (head.removeItem(secret))
                    {
                        head.insertItem(_position, secret);
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
            return base.getCurrentPosition(table);
        }
        private int _position;
    }
}

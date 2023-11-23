using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    class MoveToHeadAnim : CardMoveAnim
    {
        public MoveToHeadAnim(CardDescriptor cardDesc, Vector3 startTablePos, Pile fromPile, Pile toPile) : base(cardDesc, startTablePos, fromPile, toPile)
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

            // 播放入场动画。
            SimpleAnim enterAnim = _cardDesc.define.hasKeyword(KEYWORD.MISSION) ? secret.onMissionEnter : secret.onSecretEnter;
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, enterAnim, secret.animator, _deltaTime))
                yield return null;
        }
        public override Vector3 getCurrentPosition(BattleTableManager table)
        {
            return _startTablePos;
        }
    }
}

using System;
using System.Collections;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    [Serializable]
    class HandToFieldAnim : TableAnimation
    {
        [SerializeField]
        Vector3 _startPosition;
        [SerializeField]
        int _position;
        [SerializeField]
        private Transform _target;
        CardDescriptor _servantDesc;
        public HandToFieldAnim(Vector3 startPosition, CardDescriptor servantDesc, int index)
        {
            _startPosition = startPosition;
            _position = index;
            _servantDesc = servantDesc;

        }
        public override IEnumerator update(BattleTableManager table)
        {
            var card = table.createCard(_servantDesc, _startPosition);
            var field = _servantDesc.getOwner() == table.player ? table.table.SelfField : table.table.EnemyField;
            // 手牌移动到目标位置。
            var placeHolder = field.addPlaceHolder(_position);
            AnimAnim anim = null;
            card.positionLerp.setStart(_startPosition);
            card.positionLerp.setTarget(placeHolder);

            while (!SimpleAnimHelper.update(table, ref anim, card.onSummon, card.animator, _deltaTime))
            {
                yield return null;
            }

            // 移除手牌。
            card.destroy();

            // 召唤仆从。
            field.removePlaceHolder(placeHolder);
            var servant = table.createServant(_servantDesc, _position);
            while (!SimpleAnimHelper.update(table, ref anim, servant.onSummon, servant.animator, _deltaTime))
            {
                servant.animator.Update(0);
                yield return null;
            }
            table.game.logger.log("Animation", table.getCard(servant) + "移动动画结束");
            yield break;
        }
    }
}

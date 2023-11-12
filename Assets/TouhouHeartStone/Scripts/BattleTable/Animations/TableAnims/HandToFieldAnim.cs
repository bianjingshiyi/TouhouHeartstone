using System;
using System.Collections;
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
        private TableCard _tableCard;
        public HandToFieldAnim(Vector3 startPosition, CardDescriptor servantDesc, int index)
        {
            _startPosition = startPosition;
            _position = index;
            _servantDesc = servantDesc;

        }
        public override IEnumerator update(BattleTableManager table)
        {
            table.removeHand(_servantDesc.id);
            _tableCard = table.createMovingCard(_servantDesc, _startPosition);
            var field = _servantDesc.getOwner() == table.player ? table.table.SelfField : table.table.EnemyField;
            // 手牌移动到目标位置。
            var servant = table.createServant(_servantDesc, _position);
            servant.gameObject.SetActive(false);
            AnimAnim anim = null;
            _tableCard.positionLerp.setStart(table.TableToWorldPosition(_startPosition));
            _tableCard.positionLerp.setTarget(servant.transform);

            while (_tableCard && !SimpleAnimHelper.update(table, ref anim, _tableCard.onSummon, _tableCard.animator, _deltaTime))
            {
                yield return null;
            }

            // 移除手牌。
            table.removeMovingCard(_servantDesc);
            _tableCard = null;

            // 召唤仆从。
            servant.gameObject.SetActive(true);
            servant.setHidden(false);
            while (!SimpleAnimHelper.update(table, ref anim, servant.onSummon, servant.animator, _deltaTime))
            {
                servant.animator.Update(0);
                yield return null;
            }
            table.game.logger.logTrace("Animation", $"{table.getCard(servant)}移动动画结束");
            yield break;
        }

        public void forceFinish(BattleTableManager table)
        {
            if (_tableCard)
            {
                table.removeMovingCard(_servantDesc);
                _tableCard = null;
            }
            if (table.tryGetServant(_servantDesc.id, out var servant))
            {
                servant.gameObject.SetActive(true);
                servant.setHidden(false);
            }
            else
            {
                table.createServant(_servantDesc, _position);
            }
            finish();
        }
    }
}

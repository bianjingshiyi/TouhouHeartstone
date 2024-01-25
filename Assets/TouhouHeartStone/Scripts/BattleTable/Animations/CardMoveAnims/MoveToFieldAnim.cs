using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace Game.BattleTable
{
    class MoveToFieldAnim : CardMoveAnim
    {
        public MoveToFieldAnim(CardDescriptor cardDesc, Vector3 startPos, Pile fromPile, Pile toPile, int index, float cardHeight, bool startVisible) : base(cardDesc, startPos, fromPile, toPile, startVisible)
        {
            _position = index;
            _cardHeight = cardHeight;
        }
        public override IEnumerator startUpdate(BattleTableManager table, float deltaTime)
        {
            // 1. 如果Servant已经存在，什么也不播放。
            if (table.tryGetServant(_cardDesc.id, out _))
                yield break;

            // 2. 移除源牌堆的卡牌。
            removeCard(table);

            // 3. 创建隐藏的Servant。
            var servant = table.createServant(_cardDesc, _to?.owner, _position);
            servant.gameObject.SetActive(false);
            servant.setHidden(true);

            // 4. 如果源牌堆是手牌、牌库或者墓地，创建MovingCard，将MovingCard移动到Servant位置。
            if (!shouldMoveCreateMovingCardFromPile(_from))
                yield break;
            _tableCard = table.createMovingCard(_cardDesc, _startTablePos, _cardHeight);

            updateStartFaceup(table, _tableCard);

            var field = _cardDesc.getOwner() == table.player ? table.table.SelfField : table.table.EnemyField;
            _tableCard.positionLerp.setStartPosition(table.TableToWorldPosition(_startTablePos));
            _tableCard.positionLerp.setTargetTransform(servant.transform);

            AnimAnim anim = null;
            while (_tableCard && !SimpleAnimHelper.update(table, ref anim, _tableCard.onSummon, _tableCard.animator, deltaTime))
            {
                yield return null;
            }
        }
        public override IEnumerator logicUpdate(BattleTableManager table, float deltaTime)
        {
            // 1. 移除MovingCard。
            AnimAnim movingAnim = null;
            while (_tableCard && !SimpleAnimHelper.update(table, ref movingAnim, _tableCard.onDisappear, _tableCard.animator, deltaTime))
            {
                yield return null;
            }
            table.removeMovingCard(_cardDesc);

            if (!table.tryGetServant(_cardDesc.id, out Servant servant))
                servant = table.createServant(_cardDesc, _position);

            // 2. 如果Servant被隐藏，使Servant显示。
            if (!servant.gameObject.activeSelf)
            {
                servant.gameObject.SetActive(true);
                servant.setHidden(false);
                table.setServant(servant, _cardDesc);
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, servant.onSummon, servant.animator, deltaTime))
                {
                    servant.animator.Update(0);
                    yield return null;
                }
            }
            var sourceField = servant.field;
            var targetField = table.getField(_to);
            if (sourceField != null)
            {
                Vector3 transPos = servant.transform.position;
                if (sourceField.removeServant(servant, false))
                {
                    targetField.insertServant(_position, servant);
                    servant.transform.position = transPos;
                }
            }
            table.game.logger.logTrace("Animation", $"{table.getCard(servant)}移动动画结束");
        }
        public void forceFinish(BattleTableManager table)
        {
            if (_tableCard)
            {
                table.removeMovingCard(_cardDesc);
                _tableCard = null;
            }
            if (table.tryGetServant(_cardDesc.id, out var servant))
            {
                servant.gameObject.SetActive(true);
                servant.setHidden(false);
            }
            else
            {
                table.createServant(_cardDesc, _position);
            }
            finish(table);
        }
        public override Vector3 getCurrentPosition(BattleTableManager table)
        {
            if (_tableCard)
                return table.WorldToTablePosition(_tableCard.transform.position);
            return base.getCurrentPosition(table);
        }
        /// <summary>
        /// 当卡牌从某牌堆移动到战场时，是否应该创建MovingCard？
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        private static bool shouldMoveCreateMovingCardFromPile(Pile pile)
        {
            switch (pile?.name)
            {
                case PILE.GRAVE:
                case PILE.DECK:
                case PILE.HAND:
                    return true;
            }
            return false;
        }

        private int _position;
        private float _cardHeight;
        private TableCard _tableCard;
    }
}

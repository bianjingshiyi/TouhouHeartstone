using System.Collections;
using TouhouCardEngine;
using UnityEngine;

namespace Game.BattleTable
{
    class MoveToDeckAnim : CardMoveAnim
    {
        public MoveToDeckAnim(CardDescriptor cardDesc, Vector3 startPos, Pile fromPile, Pile toPile, float cardHeight, bool startVisible) : base(cardDesc, startPos, fromPile, toPile, startVisible)
        {
            _cardHeight = cardHeight;
        }
        public override IEnumerator startUpdate(BattleTableManager table, float deltaTime)
        {
            // 1. 移除源牌堆的卡牌。
            removeCard(table);

            // 2. 如果是同一个牌堆内的移动，什么也不播放。
            if (_from == _to)
                yield break;

            // 3. 从源牌堆的位置创建MovingCard飞到中间位置。
            var worldPos = table.TableToWorldPosition(_startTablePos);
            var targetDeck = table.getDeckUI(_to);
            var targetWorldPos = targetDeck.HoverTransform.position;
            var targetHeight = targetDeck.getCardHeight();

            //创建卡牌
            if (!table.tryGetMovingCard(_cardDesc.id, out _cardUI))
                _cardUI = table.createMovingCard(_cardDesc, _startTablePos, _cardHeight);
            updateStartFaceup(table, _cardUI);

            var targetPosition = getMiddleTargetPosition(table, worldPos, targetWorldPos);

            // 播放音效
            table.sounds.playSE(table.table.playCardSound);

            //播放动画
            var targetLocalScale = Vector3.one * (targetHeight / _cardUI.height);
            var anim = new CardTransAnim(_cardUI, targetPosition, targetLocalScale, table.toDeckCurveStart);

            while (!anim.update(table, deltaTime))//没放完动画就让它接着放
                yield return null;
        }
        public override IEnumerator logicUpdate(BattleTableManager table, float deltaTime)
        {
            // 1. 如果是同一个牌堆内的移动，什么也不播放。
            if (_from == _to)
                yield break;

            // 2. 使用MovingCard飞到目标。
            if (!_cardUI)
                _cardUI = table.createMovingCard(_cardDesc, _startTablePos, _cardHeight);

            var targetDeck = table.getDeckUI(_to);
            var targetHeight = targetDeck.getCardHeight();
            var targetTransform = targetDeck.HoverTransform;

            updateEndFaceup(table, _cardUI);

            //播放动画
            var targetLocalScale = Vector3.one * (targetHeight / _cardUI.height);
            var anim = new CardTransAnim(_cardUI, targetTransform, targetLocalScale, table.toDeckCurveEnd);
            while (!anim.update(table, deltaTime))//没放完动画就让它接着放
                yield return null;
            //摧毁卡牌
            table.removeMovingCard(_cardDesc.id);
        }
        public override Vector3 getCurrentPosition(BattleTableManager table)
        {
            if (_cardUI)
                return table.WorldToTablePosition(_cardUI.transform.position);
            return base.getCurrentPosition(table);
        }

        private float _cardHeight;

        private TableCard _cardUI;
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace Game.BattleTable
{
    [Serializable]
    class FieldToHandAnim : TableAnimation
    {
        [SerializeField]
        int _position;
        [SerializeField]
        private Transform _target;
        int _cardId;
        public FieldToHandAnim(int cardId, int index)
        {
            _cardId = cardId;
            _position = index;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            Vector3 sourcePosition = Vector3.zero;
            if (table.tryGetServant(_cardId, out var servant))
            {
                sourcePosition = servant.transform.position;
                table.removeServant(_cardId);
            }
            if (!table.tryGetHand(_cardId, out _))
            {
                var cardDesc = table.getCardDescriptor(_cardId);
                var hand = table.createHand(cardDesc, _position);
                var positionLerp = hand.positionLerp;
                var pos = sourcePosition;
                positionLerp.setStart(pos);
                positionLerp.setPosition(pos);
                positionLerp.setTarget(hand.transform);
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _deltaTime))
                {
                    yield return null;
                }
            }
            yield break;
        }
    }
}

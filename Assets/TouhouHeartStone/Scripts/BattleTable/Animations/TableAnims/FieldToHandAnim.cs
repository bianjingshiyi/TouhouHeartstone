using System;
using System.Collections;
using TouhouCardEngine;
using UnityEngine;

namespace Game.BattleTable
{
    [Serializable]
    class FieldToHandAnim : TableAnimation
    {
        [SerializeField]
        private Transform _target;
        int _cardId;
        Pile _fromPile;
        Pile _toPile;
        int _position;
        public FieldToHandAnim(int cardId, Pile fromPile, Pile toPile, int position)
        {
            _cardId = cardId;
            _fromPile = fromPile;
            _toPile = toPile;
            _position = position;
        }
        public override IEnumerator update(BattleTableManager table)
        {
            Vector3 sourcePosition = Vector3.zero;
            if (table.tryGetServant(_cardId, out var servant))
            {
                sourcePosition = table.WorldToTablePosition(servant.transform.position);
                table.removeServant(_cardId);
            }
            var anim = new MoveToHandAnim(table.getCardDescriptor(_cardId), sourcePosition, _fromPile, _toPile, _position, table.canSeeCard(_cardId));
            while (!anim.update(table, _deltaTime))
            {
                yield return null;
            }
        }
    }
}

using System;
using UnityEngine;
namespace Game.BattleTable
{
    [Serializable]
    class FieldMoveAnim : TableAnimation
    {
        [SerializeField]
        int _cardId;
        [SerializeField]
        Field _targetField;
        [SerializeField]
        int _position;
        public FieldMoveAnim(int cardId, Field targetField, int index)
        {
            _cardId = cardId;
            _targetField = targetField;
            _position = index;
        }
        public override bool update(BattleTableManager table, float time)
        {
            if (table.tryGetServant(_cardId, out Servant servant))
            {
                var desc = table.getCardDescriptor(_cardId);
                var sourceField = desc.getOwner() == table.player ? table.table.SelfField : table.table.EnemyField;
                var sourcePosition = servant.transform.position;
                if (sourceField.removeServant(servant, false))
                {
                    _targetField.insertServant(_position, servant);
                    servant.transform.position = sourcePosition;
                }
            }
            return true;
        }
    }
}

using BJSYGameCore;
using UnityEngine;
using System.Linq;
using System;
namespace Game.BattleTable
{
    [Serializable]
    class FieldMoveAnim : TableAnimation
    {
        [SerializeField]
        Servant _servant;
        [SerializeField]
        Vector3 _startPosition;
        [SerializeField]
        Timer _timer;
        public FieldMoveAnim(BattleTableManager table, Servant servant, Field field, int index)
        {
            _servant = servant;
            _startPosition = servant.transform.position;
            _timer = new Timer() { duration = table.fieldMoveCurve.keys.Last().time };

            field.insertServant(index, servant);
            _timer.start();
        }
        public override bool update(BattleTableManager table, float time)
        {
            if (!_timer.isExpired())
            {
                _servant.transform.position = Vector3.Lerp(_startPosition, _servant.transform.position, table.fieldMoveCurve.Evaluate(_timer.time));
                return false;
            }
            return true;
        }
    }
}

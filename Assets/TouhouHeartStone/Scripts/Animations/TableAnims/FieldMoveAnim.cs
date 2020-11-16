using BJSYGameCore;
using UnityEngine;
using System.Linq;
using UI;
using System;
namespace Game
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
        public FieldMoveAnim(TableManager table, Servant servant, FieldList field, int index)
        {
            _servant = servant;
            _startPosition = servant.getRoot().position;
            _timer = new Timer() { duration = table.fieldMoveCurve.keys.Last().time };

            field.addChild(servant.rectTransform);
            field.defaultItem.rectTransform.SetAsFirstSibling();
            servant.rectTransform.SetSiblingIndex(index + 1);
            _timer.start();
        }
        public override bool update(TableManager table)
        {
            if (!_timer.isExpired())
            {
                _servant.getRoot().position = Vector3.Lerp(_startPosition, _servant.rectTransform.position, table.fieldMoveCurve.Evaluate(_timer.time));
                return false;
            }
            _servant.getRoot().position = _servant.rectTransform.position;
            return true;
        }
    }
}

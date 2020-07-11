using BJSYGameCore;
using UnityEngine;
using System.Linq;
using UI;
namespace Game
{
    class HandToFieldAnim : TableAnimation
    {
        HandListItem _item;
        Vector3 _startPosition;
        Timer _timer;
        public HandToFieldAnim(TableManager table, HandListItem item, FieldList field, int index)
        {
            _item = item;
            _startPosition = item.Card.rectTransform.position;
            _timer = new Timer() { duration = table.handToFieldCurve.keys.Last().time };

            field.addChild(table.ui.ServantPlaceHolder.rectTransform);
            field.defaultItem.rectTransform.SetAsFirstSibling();
            table.ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
            table.ui.ServantPlaceHolder.display();
            _startPosition = _item.Card.rectTransform.position;
            _timer.start();
        }
        public override bool update(TableManager table)
        {
            if (!_timer.isExpired())
            {
                _item.Card.rectTransform.position = Vector3.Lerp(_startPosition, table.ui.ServantPlaceHolder.rectTransform.position, table.handToFieldCurve.Evaluate(_timer.time));
                return false;
            }
            return true;
        }
    }
}

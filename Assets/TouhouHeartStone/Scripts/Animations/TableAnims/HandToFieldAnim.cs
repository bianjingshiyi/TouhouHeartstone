using BJSYGameCore;
using UnityEngine;
using System.Linq;
using UI;
using System;
namespace Game
{
    [Serializable]
    class HandToFieldAnim : TableAnimation
    {
        [SerializeField]
        HandListItem _item;
        [SerializeField]
        Vector3 _startPosition;
        [SerializeField]
        Timer _timer;
        public HandToFieldAnim(TableManager table, HandListItem item, FieldList field, int index)
        {
            _item = item;
            var animator = _item.GetComponent<Animator>();
            if (animator != null)
                animator.enabled = false;
            _startPosition = item.Card.rectTransform.position;
            table.game.logger.log("Animation", item + "移动到" + index);
            _timer = new Timer() { duration = table.handToFieldCurve.keys.Last().time };

            field.addChild(table.ui.ServantPlaceHolder.rectTransform);
            field.defaultItem.rectTransform.SetAsFirstSibling();
            table.ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
            table.ui.ServantPlaceHolder.display();
            _timer.start();
        }
        public override bool update(TableManager table, float time)
        {
            if (!_timer.isExpired())
            {
                Vector3 newPos = Vector3.Lerp(_startPosition, table.ui.ServantPlaceHolder.rectTransform.position, table.handToFieldCurve.Evaluate(_timer.time));
                _item.Card.rectTransform.position = newPos;
                table.game.logger.log("Animation", _item + "移动到" + newPos);
                return false;
            }
            var animator = _item.GetComponent<Animator>();
            if (animator != null)
                animator.enabled = true;
            table.game.logger.log("Animation", _item + "移动动画结束");
            return true;
        }
    }
}

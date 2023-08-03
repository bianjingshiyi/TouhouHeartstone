using BJSYGameCore;
using UnityEngine;
using System.Linq;
using System;
namespace Game.BattleTable
{
    [Serializable]
    class HandToFieldAnim : TableAnimation
    {
        [SerializeField]
        HandItem _item;
        [SerializeField]
        Servant _servant;
        [SerializeField]
        Vector3 _startPosition;
        [SerializeField]
        Timer _timer;
        public HandToFieldAnim(BattleTableManager table, HandItem item, Field field, int index)
        {
            _item = item;
            var animator = _item.GetComponent<Animator>();
            if (animator != null)
                animator.enabled = false;
            _startPosition = item.card.transform.position;
            _servant = field.insertServant(index);
            table.game.logger.log("Animation", item + "移动到" + index);

            _timer = new Timer() { duration = table.handToFieldCurve.keys.Last().time };
            _timer.start();

        }
        public override bool update(BattleTableManager table, float time)
        {
            if (!_timer.isExpired())
            {
                Vector3 newPos = Vector3.Lerp(_startPosition, _servant.transform.position, table.handToFieldCurve.Evaluate(_timer.time));
                _item.card.transform.position = newPos;
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

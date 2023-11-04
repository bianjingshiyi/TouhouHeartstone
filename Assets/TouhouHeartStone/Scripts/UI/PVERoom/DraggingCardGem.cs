using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class DraggingCardGem : MonoBehaviour
    {
        #region 公有方法
        public void startDrag(CardGem sourceGem)
        {
            _isDragging = true;
            move(sourceGem.transform.position);
            _sourceGem = sourceGem;
            _sourceGem.setVisible(false);
        }
        public void changeSource(CardGem gem)
        {
            if (_sourceGem)
                _sourceGem.setVisible(true);
            _sourceGem = gem;
            if (_sourceGem)
                _sourceGem.setVisible(false);
        }
        public void release()
        {
            _isDragging = false;
        }
        public void move(Vector2 position)
        {
            var pos = transform.position;
            pos.x = position.x;
            pos.y = position.y;
            transform.position = pos;
        }
        public void destroy()
        {
            Destroy(gameObject);
            onDestroy?.Invoke(this);
            onDestroy = null;
        }
        #endregion

        private void Update()
        {
            if (!_isDragging && _sourceGem)
            {
                var trans = _gem.transform;
                var dis = _sourceGem.transform.position - trans.position;
                trans.position += dis * _returnSpeed;
                if (dis.sqrMagnitude < 2000)
                {
                    if (_sourceGem)
                    {
                        _sourceGem.setVisible(true);
                        _sourceGem = null;
                    }
                    destroy();
                }
            }
        }

        public event Action<DraggingCardGem> onDestroy;

        public CardGem gem => _gem;
        [SerializeField]
        private CardGem _gem;
        [SerializeField]
        private float _returnSpeed = 0.5f;
        [SerializeField]
        private bool _isDragging;
        [SerializeField]
        private CardGem _sourceGem;
    }
}

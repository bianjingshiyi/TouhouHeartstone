using UnityEngine;
namespace Game.BattleTable
{
    [ExecuteInEditMode]
    public class PositionLerp : MonoBehaviour
    {
        #region 公有方法

        #region 开始位置
        public void setStartPositionToCurrent()
        {
            setStartPosition(transform.position);
        }
        public void setStartPosition(Vector3 globalPosition)
        {
            if (_isLocalMode)
            {
                _startPosition = globalToLocalPoint(globalPosition);
            }
            else
            {
                _startPosition = globalPosition;
            }
        }
        public void setStartLocalPosition(Vector3 localPosition)
        {
            if (_isLocalMode)
            {
                _startPosition = localPosition;
            }
            else
            {
                _startPosition = localToGlobalPoint(localPosition);
            }
        }
        #endregion

        #region 目标位置
        public void setTargetTransform(Transform target, float stopDistance = 0)
        {
            _targetTransform = target;
            _targetPosition = Vector3.zero;
            _stopDistance = stopDistance;
        }
        public void setTargetPosition(Vector3 targetPos, float stopDistance = 0)
        {
            _targetTransform = null;
            _targetPosition = targetPos;
            _stopDistance = stopDistance;
        }
        public void setTargetPositionToCurrent()
        {
            setTargetPosition(transform.position);
        }
        #endregion

        #region 当前位置
        public void setWorldPosition(Vector3 pos)
        {
            transform.position = pos;
        }
        public Vector3 getWorldPosition()
        {
            return transform.position;
        }
        public void setLocalPosition(Vector3 pos)
        {
            transform.localPosition = pos;
        }
        public Vector3 getLocalPosition()
        {
            return transform.localPosition;
        }
        public void setPositionByTime(float time)
        {
            Vector3 targetPosition = targetTransform ? targetTransform.position : _targetPosition;
            Vector3 startPosition = _startPosition;
            if (_isLocalMode)
            {
                var globalStartPosition = localToGlobalPoint(startPosition);
                Vector3 stoppedPosition = Vector3.MoveTowards(targetPosition, globalStartPosition, _stopDistance);
                stoppedPosition = globalToLocalPoint(stoppedPosition);

                var position = Vector3.Lerp(startPosition, stoppedPosition, time);
                setLocalPosition(position);
            }
            else
            {
                Vector3 stoppedPosition = Vector3.MoveTowards(targetPosition, startPosition, _stopDistance);

                var position = Vector3.Lerp(startPosition, stoppedPosition, time);
                setWorldPosition(position);
            }
        }
        public void setPositionToStart()
        {
            setPositionByTime(0);
        }
        #endregion

        #endregion

        #region 私有方法

        #region 生命周期
        protected void Reset()
        {
            enabled = false;
        }
        protected void OnEnable()
        {
            initStartPosition();
        }
        protected void LateUpdate()
        {
            //不能将位置的更新放在LateUpdate中，LateUpdate中TargetTransform的位置与Update中的不同，可能与动画和Layout有关，此时处于没有计算Layout的状态。
            //也不能在Update中获取位置，然后在LateUpdate中更新位置，更新的位置仍然是错的。
            // 不过在场景重做为Mesh之后好像就没有这个问题了
            setPositionByTime(_time);
        }
        #endregion

        private void initStartPosition()
        {
            if (!Application.isPlaying)
                return;
            setStartPositionToCurrent();
        }
        private Vector3 globalToLocalPoint(Vector3 global)
        {
            if (!transform.parent)
            {
                return global;
            }
            return transform.parent.InverseTransformPoint(global);
        }
        private Vector3 localToGlobalPoint(Vector3 local)
        {
            if (!transform.parent)
            {
                return local;
            }
            return transform.parent.TransformPoint(local);
        }
        #endregion

        #region 属性字段
        public Transform targetTransform => _targetTransform;
        [Range(0, 1)]
        [SerializeField]
        public float _time;
        [SerializeField]
        public bool _isLocalMode;
        [SerializeField]
        Transform _targetTransform;
        [SerializeField]
        float _stopDistance;
        [SerializeField]
        Vector3 _startPosition;
        [SerializeField]
        Vector3 _targetPosition;
        public bool isLocalMode
        {
            get => _isLocalMode;
            set => _isLocalMode = value;
        }
        public Vector3 startPosition
        {
            get => _startPosition;
        }
        #endregion
    }
}

using UnityEngine;
using BJSYGameCore;
using System;
namespace UI
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        float _speed;
        public float speed
        {
            get { return _speed; }
        }
        [SerializeField]
        Direction _direction;
        [SerializeField]
        Transform _target;
        public Transform target
        {
            get { return _target; }
            set { _target = value; }
        }
        [SerializeField]
        SimpleAnim _onHit = new SimpleAnim()
        {
            animName = "Death"
        };
        bool _isOnHitPlayed;
        bool _isOnHitFinished;
        enum Direction
        {
            up,
            right
        }
        protected void Update()
        {
            if (target == null)
                return;
            if (transform.position != target.position)
            {
                if (_direction == Direction.up)
                    transform.up = target.position - transform.position;
                else if (_direction == Direction.right)
                    transform.right = target.position - transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
                if (transform.position != target.position)
                    return;
                else
                {
                    try
                    {
                        _onHit.invokeBeforeAnim();
                    }
                    catch (Exception e)
                    {
                        UberDebug.LogError("UI", this + "播放onHit动画引发异常：" + e);
                        Destroy(gameObject);
                    }
                }
            }
            try
            {
                Animator animator = GetComponentInChildren<Animator>();
                if (!AnimHelper.update(animator, _onHit.animName, ref _isOnHitPlayed, ref _isOnHitFinished))
                    return;
                _onHit.invokeAfterAnim();
            }
            catch (Exception e)
            {
                UberDebug.LogError("UI", this + "播放onHit动画引发异常：" + e);
                Destroy(gameObject);
            }
            Destroy(gameObject);
        }
    }
}

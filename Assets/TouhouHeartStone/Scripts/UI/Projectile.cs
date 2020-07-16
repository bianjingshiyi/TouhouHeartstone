using UnityEngine;
using BJSYGameCore;
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
        Transform _target;
        public Transform target
        {
            get { return _target; }
            set { _target = value; }
        }
        [SerializeField]
        Timer _decayTimer = new Timer() { duration = 1 };
        private void Update()
        {
            if (target == null)
                return;
            if (transform.position != target.position)
            {
                transform.up = target.position - transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
                if (transform.position == target.position)
                {
                    _decayTimer.start();
                }
            }
            if (_decayTimer.isExpired())
            {
                Destroy(gameObject);
            }
        }
    }
}

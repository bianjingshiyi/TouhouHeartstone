using UnityEngine;

namespace Game.Animations
{
    public class LaserEffect : TableEffect
    {
        protected override void updateAnim(float deltaTime)
        {
            Vector3 sourcePos = (sourceTransform ? sourceTransform.position : Vector3.zero) + Vector3.up * heightOffset;
            Vector3 targetPos = (destTransform ? destTransform.position : sourcePos) + Vector3.up * heightOffset;
            if (_laser != null)
            {
                _laser.SetStartPoint(sourcePos);
                _laser.SetEndPoint(targetPos);

                if (time > duration - _decayDuration && !_laser.startDissovle)
                {
                    _laser.DisablePrepare();
                }
            }
            else
            {
                transform.position = sourcePos;
                transform.LookAt(targetPos);
                if (_onHit != null)
                {
                    _onHit.position = targetPos;
                }
            }
        }
        [SerializeField]
        private float _decayDuration = 0.5f;
        [SerializeField]
        private ToonLaser _laser;
        [SerializeField]
        private Transform _onHit;
    }
}
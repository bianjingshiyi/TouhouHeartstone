using UnityEngine;
using UnityEngine.Video;
namespace BJSYGameCore.Video
{
    public class VideoPlayerController : MonoBehaviour
    {
        [SerializeField]
        VideoPlayer _videoPlayer;
        VideoPlayer videoPlayer
        {
            get
            {
                if (_videoPlayer == null)
                    _videoPlayer = GetComponentInChildren<VideoPlayer>();
                return _videoPlayer;
            }
        }
        public float time
        {
            get { return _targetTime < 0 ? normalizedTime : _targetTime; }
        }
        float normalizedTime
        {
            get { return (float)(videoPlayer.time / videoPlayer.length); }
            set { videoPlayer.time = value * videoPlayer.length; }
        }
        [SerializeField]
        float _targetTime = -1;
        public void setTime(float time)
        {
            _targetTime = time;
        }
        [SerializeField]
        bool _isPlaying = false;
        public bool isPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
            }
        }
        protected void Awake()
        {
            videoPlayer.loopPointReached += VideoPlayer_loopPointReached;
        }
        protected void Update()
        {
            if (!_isPlaying && _targetTime < 0)
                _targetTime = normalizedTime;
            if (_targetTime >= 0)
            {
                normalizedTime = _targetTime;
                if (Mathf.Abs(normalizedTime - _targetTime) < 0.01f)
                {
                    if (_isPlaying)
                        _targetTime = -1;
                }
            }
            videoPlayer.Play();
        }
        private void VideoPlayer_loopPointReached(VideoPlayer source)
        {
            setTime(0);
            isPlaying = false;
        }
        public void play(VideoClip clip)
        {
            videoPlayer.clip = clip;
            videoPlayer.Play();
            isPlaying = true;
            setTime(0);
        }
    }
}
using UnityEngine;

namespace GM.Data
{
    public struct Timer
    {
        private float _startTime;
        private float _duration;
        private float _deadline;
        private bool _running;

        public float Duration
        {
            set => _duration = value;
        }

        public bool HasStarted => _running;
        public float Progress => Mathf.Clamp01(Mathf.InverseLerp(_startTime, _startTime + _duration, Time.time));

        public bool HasExpired(out float excess)
        {
            excess = Mathf.Clamp01(Time.time - _deadline);
            return Time.time >= _deadline;
        }

        public void Start(float excess = 0)
        {
            _running = true;
            _deadline = Time.time + _duration - excess;
            _startTime = Time.time;
        }

        public void SetEnabled(bool enabled)
        {
            _running = enabled;
        }

        public void ExtendThisFrame()
        {
            var remaining = _deadline - Time.time;
            _deadline = Time.time + remaining + Time.deltaTime;
        }
    }
}

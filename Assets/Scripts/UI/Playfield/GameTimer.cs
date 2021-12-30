using System;
using TMPro;
using UnityEngine;

namespace GM.UI.Playfield
{
    public class GameTimer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _minutes;
        [SerializeField] private TMP_Text _seconds;
        [SerializeField] private TMP_Text _milliseconds;

        private bool _running;
        private float? _startTime;

        private float CurrentTime => Time.time - _startTime.Value;

        public void StartTimer()
        {
            _startTime = Time.time;
        }

        public void StopTimer()
        {
            _startTime = null;
        }

        public void NewSection(float sectionTime)
        {
            //TODO: Player Best Time
        }

        private void Update()
        {
            if (_startTime.HasValue)
            {
                var time = CurrentTime;
                var timeSeconds = time % 60;

                _minutes.text = (time / 60).ToString("00");
                _seconds.text = Mathf.Floor(timeSeconds).ToString("00");
                _milliseconds.text = timeSeconds.ToString("00.00").Substring(3);
            }
        }

        private void Awake()
        {
            if (!_minutes)
            {
                throw new ArgumentNullException(nameof(_minutes));
            }

            if (!_seconds)
            {
                throw new ArgumentNullException(nameof(_seconds));
            }

            if (!_milliseconds)
            {
                throw new ArgumentNullException(nameof(_milliseconds));
            }
        }
    }
}

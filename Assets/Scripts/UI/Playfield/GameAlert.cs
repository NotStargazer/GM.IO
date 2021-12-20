using System;
using GM.Data;
using TMPro;
using UnityEngine;

namespace GM.UI.Playfield
{
    public enum AlertType
    {
        SectionClear,
        SectionFail,
        TSpin,
    }

    public class GameAlert : MonoBehaviour
    {
        [SerializeField] private Transform Transform;
        [SerializeField] private float _alertDuration;
        [SerializeField] private float _blinkDuration;
        [SerializeField] private AnimationCurve _animationCurve;
        [SerializeField] private TMP_Text _alertText;

        private AlertType _currentAlert;
        private Timer _scaleTimer;
        private Timer _blinkTimer;
        private Vector3 _startSize;
        private bool _blink;

        public void Alert(AlertType alert)
        {
            _currentAlert = alert;
            _scaleTimer.Duration = _alertDuration;
            _scaleTimer.Start();
            _blinkTimer.Duration = _blinkDuration;
            _blinkTimer.Start();

            _alertText.text = _currentAlert switch
            {
                AlertType.SectionClear => "Section Clear",
                AlertType.SectionFail => "Section Fail",
                AlertType.TSpin => "T Spin",
                _ => _alertText.text
            };
        }

        private void Update()
        {
            if (_scaleTimer.HasStarted)
            {
                if (_scaleTimer.HasExpired(out _))
                {
                    _scaleTimer.SetEnabled(false);
                }

                Transform.localScale = _startSize * _animationCurve.Evaluate(_scaleTimer.Progress);

                if (_blinkTimer.HasExpired(out var blinkExcess))
                {
                    _blink = !_blink;
                    _blinkTimer.Start(blinkExcess);

                    _alertText.color = _blink ? Color.yellow : Color.white;
                }
            }
        }

        private void OnValidate()
        {
            Transform = transform;
        }

        private void Awake()
        {
            _startSize = Transform.localScale;
            Transform.localScale = Vector3.zero;
        }
    }
}
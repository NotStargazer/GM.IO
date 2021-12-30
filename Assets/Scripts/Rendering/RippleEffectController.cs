using GM.Data;
using UnityEngine;

namespace GM.UI.Rendering
{
    public class RippleEffectController : MonoBehaviour
    {
        private static readonly int PROGRESS = Shader.PropertyToID("_RippleProgress");
        private static readonly int ASPECT_RATIO = Shader.PropertyToID("_ScreenRatio");
        private static readonly int FOCAL_POINT = Shader.PropertyToID("_FocalPoint");
        private static readonly int RIPPLE_SIZE = Shader.PropertyToID("_RippleSize");

        [SerializeField] private Camera _mainCamera;
        [SerializeField] private AnimationCurve _rippleProgression;
        [SerializeField] private float _effectTime;
        [SerializeField] private float _rippleSize;

        private Timer _rippleTimer;

        public void StartRipple(Vector2 focalPoint)
        {
            var screenFocalPoint = _mainCamera.WorldToViewportPoint(new Vector3(focalPoint.x, focalPoint.y, 0));
            Shader.SetGlobalVector(FOCAL_POINT, screenFocalPoint);
            _rippleTimer.Start();
        }

        private void Update()
        {
            if (_rippleTimer.HasStarted)
            {
                if (_rippleTimer.HasExpired(out _))
                {
                    _rippleTimer.SetEnabled(false);
                }

                Shader.SetGlobalFloat(PROGRESS, _rippleProgression.Evaluate(_rippleTimer.Progress));
            }
        }

        private void Awake()
        {
            Shader.SetGlobalFloat(ASPECT_RATIO, _mainCamera.aspect);
            Shader.SetGlobalVector(FOCAL_POINT, Vector2.zero);
            Shader.SetGlobalFloat(PROGRESS, -1);
            Shader.SetGlobalFloat(RIPPLE_SIZE, _rippleSize);

            _rippleTimer.Duration = _effectTime;
        }
    }
}

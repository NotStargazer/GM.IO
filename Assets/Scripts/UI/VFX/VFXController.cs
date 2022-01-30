using System;
using System.Collections.Generic;
using GM.Data;
using UnityEngine;
using UnityEngine.VFX;

namespace GM.UI
{
    public interface IVFXController
    {
        public void PlayLineBreakVFX(List<int> linesCleared);
    }

    public struct VisualEffectInstance
    {
        public VisualEffect VFX;
        public Transform Transform;
    }

    public class VFXController : MonoBehaviour, IVFXController
    {
        [SerializeField] private Transform _playfieldTransform;

        [SerializeField] private VisualEffect _lineBreakVFXPrefab;

        private VisualEffectInstance[] _lineBreakVFXInstances;

        public void PlayLineBreakVFX(List<int> linesCleared)
        {
            var width = GameData.GetInstance().GridSize.x;
            var loops = 0;

            foreach (var y in linesCleared)
            {
                for (var x = 0; x < width; x++)
                {
                    var instance = _lineBreakVFXInstances[x + width * loops];
                    instance.Transform.position =
                        new Vector3(x + 0.5f, y + 0.5f) + _playfieldTransform.position;
                    instance.VFX.Play();
                }

                loops++;
            }
        }

        private void Awake()
        {
            if (!_playfieldTransform)
            {
                throw new ArgumentNullException(nameof(_playfieldTransform));
            }
            if (!_lineBreakVFXPrefab)
            {
                throw new ArgumentNullException(nameof(_lineBreakVFXPrefab));
            }

            var totalInstances = GameData.GetInstance().GridSize.x * 4;
            _lineBreakVFXInstances = new VisualEffectInstance[totalInstances];

            for (var i = 0; i < totalInstances; i++)
            {
                var vfx = Instantiate(_lineBreakVFXPrefab, transform);
                _lineBreakVFXInstances[i].Transform = vfx.transform;
                _lineBreakVFXInstances[i].VFX = vfx;
            }
        }
    }
}
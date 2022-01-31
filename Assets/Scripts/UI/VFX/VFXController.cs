using System;
using System.Collections.Generic;
using GM.Data;
using UnityEngine;

namespace GM.UI
{
    [Serializable]
    public struct VFXProperties
    {
        public LineBreakVFXProperties LineBreak;
    }

    [Serializable]
    public struct LineBreakVFXProperties
    {
        public Mesh QuadMesh;
        public Material Material;
        public AnimationCurve Animation;
        public int ParticlesGridSize;
        public float ParticlesSpeedMult;
        public float Duration;
    }

    public interface IVFXController
    {
        public void CreateNewVFXInstance(VFXInstance instance);
        public VFXProperties Properties { get; }
    }

    public class VFXController : MonoBehaviour, IVFXController
    {
        [SerializeField] private VFXProperties _properties;
        private List<VFXInstance> _instances;

        public VFXProperties Properties => _properties;

        public void CreateNewVFXInstance(VFXInstance instance)
        {
            instance.Properties = _properties;
            _instances.Add(instance);
        }

        private void Update()
        {
            for (var index = 0; index < _instances.Count; index++)
            {
                var vfxInstance = _instances[index];
                if (vfxInstance.Expired)
                {
                    index--;
                    _instances.Remove(vfxInstance);
                }
                else
                {
                    vfxInstance.Draw();
                }
            }
        }

        private void Awake()
        {
            _instances = new List<VFXInstance>();
        }
    }

    public abstract class VFXInstance
    {
        public VFXProperties Properties { protected get; set; }

        protected Timer _timer;
        public bool Expired => _timer.HasExpired(out _);
        public abstract void Draw();
    }
}
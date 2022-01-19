using System;
using UnityEngine;

namespace GM
{
    [Serializable]
    public struct SFXKey
    {
        public SFX Key;
        public AudioClip Clip;
    }

    public enum SFX
    {
        Lock,
        Land,
        LineClear,
        LineFall,
        Section
    }

    public class SoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        [HideInInspector] [SerializeField] private SFXKey[] _sfxKeys;

        public void PlaySFX(SFX soundEffect, float velocity = 1)
        {
            AudioClip clip = null;

            foreach (var sfxKey in _sfxKeys)
            {
                if (sfxKey.Key == soundEffect)
                {
                    clip = sfxKey.Clip;
                    break;
                }
            }

            if (clip == null)
            {
                throw new ArgumentNullException($"Missing SFX in key: {soundEffect}");
            }

            _source.PlayOneShot(clip, velocity);
        }
    }
}

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

    [Serializable]
    public struct BGMKey
    {
        public BGM Key;
        public AudioClip LeadIn;
        public AudioClip Music;
        public float Tempo;
    }

    public enum SFX
    {
        Lock,
        Land,
        LineClear,
        LineFall,
        SectionPass,
        SectionBell,
        SectionClear,
        SectionFail,
        PreRotate,
        PreHold,
        TSpin,
        Ready,
        Go,
        BlockO,
        BlockJ,
        BlockL,
        BlockZ,
        BlockS,
        BlockT,
        BlockI
    }

    public enum BGM
    {
        RS1,
        RS2,
        RS3
    }

    public class SoundController : MonoBehaviour
    {
        private int _previousBeat;
        private bool _playingLeadIn;
        private BGMKey? _currentMusic;
        private BGMKey? _nextMusic;

        [SerializeField] private AudioSource _source;
        [HideInInspector] [SerializeField] private BGMKey[] _bgmKeys;
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

        public void StartMusic(BGM backgroundMusic)
        {
            foreach (var bgmKey in _bgmKeys)
            {
                if (bgmKey.Key == backgroundMusic)
                {
                    _currentMusic = bgmKey;
                    _playingLeadIn = true;
                    _source.clip = bgmKey.LeadIn;
                    _source.Play();
                    break;
                }
            }
        }

        public void SwitchMusic(BGM backgroundMusic)
        {
            foreach (var bgmKey in _bgmKeys)
            {
                if (bgmKey.Key == backgroundMusic)
                {
                    _nextMusic = bgmKey;
                    _playingLeadIn = true;
                    break;
                }
            }
        }

        public void StopMusic()
        {
            _source.Stop();
        }

        private void Update()
        {
            if (!_currentMusic.HasValue)
            {
                return;
            }

            var current = _currentMusic.Value;

            if (!_nextMusic.HasValue && _playingLeadIn)
            {
                if (_source.time >= current.LeadIn.length)
                {
                    _playingLeadIn = false;
                    _source.clip = current.Music;
                    _source.loop = true;
                    _source.Play();
                }
            }

            if (!_nextMusic.HasValue)
            {
                return;
            }

            var next = _nextMusic.Value;

            var measureLength = 60 / current.Tempo * 8;
            var beat = Mathf.FloorToInt(_source.time % measureLength / measureLength * 8) + 1;
            
            Debug.Log(beat);

            if (_previousBeat > beat)
            {
                if (_playingLeadIn)
                {
                    if (next.LeadIn)
                    {
                        _source.clip = next.LeadIn;
                        _source.loop = false;

                        _currentMusic = _nextMusic;
                        _nextMusic = null;

                        _source.Play();
                    }
                    else
                    {
                        _source.clip = next.Music;
                        _source.loop = true;

                        _currentMusic = _nextMusic;
                        _nextMusic = null;

                        _source.Play();
                    }
                }
            }

            _previousBeat = beat;
        }
    }
}

using System;
using GM.Data;
using TMPro;
using UnityEngine;

namespace GM.UI.Playfield
{
    public class LevelCounter : MonoBehaviour
    {
        private static readonly int PROGRESS = Shader.PropertyToID("_Progress");

        [SerializeField] private TMP_Text _currentLevel;
        [SerializeField] private TMP_Text _sectionLevel;
        [SerializeField] private Renderer _gravityBar;

        private MaterialPropertyBlock _properties;

        public void Set(int currentLevel, float gravity)
        {
            var endLevel = GameData.GetInstance().ProgressionData.EndLevel;

            _currentLevel.text = Mathf.Clamp(currentLevel, 0, endLevel).ToString();
            _sectionLevel.text = Mathf.Clamp(Mathf.FloorToInt(currentLevel * 0.01f + 1) * 100, 0, endLevel).ToString();

            _properties.SetFloat(PROGRESS, gravity);
            _gravityBar.SetPropertyBlock(_properties);
        }

        private void Awake()
        {
            if (!_currentLevel)
            {
                throw new ArgumentNullException(nameof(_currentLevel));
            }

            if (!_sectionLevel)
            {
                throw new ArgumentNullException(nameof(_currentLevel));
            }

            if (!_gravityBar)
            {
                throw new ArgumentNullException(nameof(_currentLevel));
            }

            _properties = new MaterialPropertyBlock();
        }
    }
}

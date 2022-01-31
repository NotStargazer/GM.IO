using System.Collections.Generic;
using GM.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace GM.UI
{
    public class LineBreakVFXInstance : VFXInstance
    {
        private static readonly int PARTICLE_COLOR_TOP = Shader.PropertyToID("_TopColors");
        private static readonly int PARTICLE_COLOR_BOT = Shader.PropertyToID("_BotColors");
        private static readonly int PARTICLE_ST = Shader.PropertyToID("_Particle_ST");

        private Vector2[] _directions;
        private Matrix4x4[] _transforms;
        private MaterialPropertyBlock _properties;
        private int _particleCount;
        private int _visibleCount;
        private float _size;

        public LineBreakVFXInstance(List<DestroyedLine> linesCleared, int gridWidth, Vector3 playfieldPosition, LineBreakVFXProperties properties)
        {
            var arrayIndex = 0;

            var lineCount = gridWidth * properties.ParticlesGridSize * properties.ParticlesGridSize;
            _particleCount = lineCount * 4;
            _visibleCount = lineCount * linesCleared.Count;

            _timer.Duration = properties.Duration;
            _timer.Start();

            _properties = new MaterialPropertyBlock();
            _directions = new Vector2[_visibleCount];
            _transforms = new Matrix4x4[_particleCount];
            var topColor = new Vector4[_particleCount];
            var botColor = new Vector4[_particleCount];
            var textureSTs = new Vector4[_particleCount];

            _size = 1f / properties.ParticlesGridSize;

            foreach (var line in linesCleared)
            {
                var y = line.LineNumber;
                for (var x = 0; x < gridWidth; x++)
                {
                    for (var py = 0; py < properties.ParticlesGridSize; py++)
                    {
                        for (var px = 0; px < properties.ParticlesGridSize; px++)
                        {
                            var xPos = (float)px / properties.ParticlesGridSize;
                            var yPos = (float)py / properties.ParticlesGridSize;
                            
                            var direction = new Vector2((px - 0.5f) * 2, (py - 0.5f) * 2).normalized;
                            direction = Quaternion.AngleAxis(Random.Range(-90f, 90f), Vector3.back) * direction;
                            _directions[arrayIndex] = direction * properties.ParticlesSpeedMult;

                            var transform = Matrix4x4.identity;
                            var position =
                                new Vector3(xPos, yPos)
                                + new Vector3(x, y)
                                + new Vector3(_size * 0.5f, _size * 0.5f)
                                + playfieldPosition;
                            transform.SetTRS(position, Quaternion.identity, Vector3.one * _size);
                            _transforms[arrayIndex] = transform;

                            topColor[arrayIndex] = line.Blocks[x].TopColor.linear;
                            botColor[arrayIndex] = line.Blocks[x].BotColor.linear;

                            textureSTs[arrayIndex] = new Vector4(_size/2, _size, xPos / 2 + line.Blocks[x].TextureST.z, yPos + line.Blocks[x].TextureST.w);

                            arrayIndex++;
                        }
                    }
                }
            }

            _properties.SetVectorArray(PARTICLE_COLOR_TOP, topColor);
            _properties.SetVectorArray(PARTICLE_COLOR_BOT, botColor);
            _properties.SetVectorArray(PARTICLE_ST, textureSTs);
        }

        public override void Draw()
        {
            if (_timer.HasStarted)
            {
                Graphics.DrawMeshInstanced(
                    mesh: Properties.LineBreak.QuadMesh,
                    submeshIndex: 0,
                    material: Properties.LineBreak.Material,
                    matrices: _transforms,
                    count: _particleCount,
                    properties: _properties,
                    castShadows: ShadowCastingMode.Off,
                    receiveShadows: false
                    );
            }

            for (var particleIndex = 0; particleIndex < _visibleCount; particleIndex++)
            {
                var animationPosition = Properties.LineBreak.Animation.Evaluate(_timer.Progress);
                var position = (Vector3)_transforms[particleIndex].GetColumn(3);
                position +=
                    new Vector3(_directions[particleIndex].x, _directions[particleIndex].y)
                    * animationPosition
                    * Time.deltaTime;
                _transforms[particleIndex].SetTRS(position, Quaternion.identity, Vector3.one * _size * animationPosition);
            }
        }
    }
}

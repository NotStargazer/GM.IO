using System;
using UnityEditor;
using UnityEngine;

namespace GM.Editor
{
    [CustomEditor(typeof(SoundController))]
    public class SoundControllerEditor : UnityEditor.Editor
    {
        private bool _sfx;
        private bool _bgm;
        private bool[] _bgmStructs;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var sfxKeys = serializedObject.FindProperty("_sfxKeys");
            var sfxTypes = Enum.GetValues(typeof(SFX));

            var apply = sfxTypes.Length != sfxKeys.arraySize;

            while (sfxTypes.Length != sfxKeys.arraySize)
            {
                if (sfxTypes.Length > sfxKeys.arraySize)
                {
                    sfxKeys.InsertArrayElementAtIndex(sfxKeys.arraySize);
                }
                else if (sfxTypes.Length < sfxKeys.arraySize)
                {
                    sfxKeys.DeleteArrayElementAtIndex(sfxKeys.arraySize - 1);
                }
            }

            var bgmKeys = serializedObject.FindProperty("_bgmKeys");
            var bgmTypes = Enum.GetValues(typeof(BGM));

            apply = bgmKeys.arraySize != bgmTypes.Length || apply;

            while (bgmTypes.Length != bgmKeys.arraySize)
            {
                if (bgmTypes.Length > bgmKeys.arraySize)
                {
                    bgmKeys.InsertArrayElementAtIndex(bgmKeys.arraySize);
                }
                else if (bgmTypes.Length < bgmKeys.arraySize)
                {
                    bgmKeys.DeleteArrayElementAtIndex(bgmKeys.arraySize - 1);
                }
            }

            _bgmStructs ??= new bool[bgmTypes.Length];

            if (apply)
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space(10);
            _sfx = EditorGUILayout.BeginFoldoutHeaderGroup(_sfx, "Sound Effects");
            if (_sfx)
            {
                for (var i = 0; i < sfxTypes.Length; i++)
                {
                    var audioKey = sfxKeys.GetArrayElementAtIndex(i);
                    var key = audioKey.FindPropertyRelative("Key");
                    var audioClip = (AudioClip)audioKey.FindPropertyRelative("Clip").objectReferenceValue;

                    var newClip = (AudioClip)EditorGUILayout.ObjectField(key.enumDisplayNames[i], audioClip,
                        typeof(AudioClip), false);

                    if (newClip)
                    {
                        audioKey.FindPropertyRelative("Clip").objectReferenceValue = newClip;
                        serializedObject.ApplyModifiedProperties();
                    }

                    if (key.enumValueIndex != i)
                    {
                        key.enumValueIndex = i;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);
            _bgm = EditorGUILayout.BeginFoldoutHeaderGroup(_bgm, "Background Music");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (_bgm)
            {
                for (var i = 0; i < bgmTypes.Length; i++)
                {
                    _bgmStructs[i] = EditorGUILayout.BeginFoldoutHeaderGroup(_bgmStructs[i], $"Stage {i + 1}");

                    if (_bgmStructs[i])
                    {
                        var audioKey = bgmKeys.GetArrayElementAtIndex(i);
                        var key = audioKey.FindPropertyRelative("Key");
                        var leadIn = (AudioClip)audioKey.FindPropertyRelative("LeadIn").objectReferenceValue;
                        var music = (AudioClip)audioKey.FindPropertyRelative("Music").objectReferenceValue;

                        var newLead = (AudioClip)EditorGUILayout.ObjectField("Lead In", leadIn,
                            typeof(AudioClip), false);
                        var newMusic = (AudioClip)EditorGUILayout.ObjectField("Music", music,
                            typeof(AudioClip), false);
                        var tempoProp = audioKey.FindPropertyRelative("Tempo");
                        var newTempo = EditorGUILayout.FloatField("Tempo", tempoProp.floatValue);

                        if (newLead || newMusic || tempoProp.floatValue != newTempo)
                        {
                            audioKey.FindPropertyRelative("LeadIn").objectReferenceValue = newLead;
                            audioKey.FindPropertyRelative("Music").objectReferenceValue = newMusic;
                            tempoProp.floatValue = newTempo;
                            serializedObject.ApplyModifiedProperties();
                        }

                        if (key.enumValueIndex != i)
                        {
                            key.enumValueIndex = i;
                            serializedObject.ApplyModifiedProperties();
                        }
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }
        }
    }
}

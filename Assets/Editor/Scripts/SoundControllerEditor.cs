using System;
using UnityEditor;
using UnityEngine;

namespace GM.Editor
{
    [CustomEditor(typeof(SoundController))]
    public class SoundControllerEditor : UnityEditor.Editor
    {
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

            if (apply)
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Sound Effects", EditorStyles.boldLabel);

            for (var i = 0; i < sfxTypes.Length; i++)
            {
                var audioKey = sfxKeys.GetArrayElementAtIndex(i);
                var key = audioKey.FindPropertyRelative("Key");
                var audioClip = (AudioClip)audioKey.FindPropertyRelative("Clip").objectReferenceValue;

                var newClip = (AudioClip)EditorGUILayout.ObjectField(sfxTypes.GetValue(i).ToString(), audioClip, typeof(AudioClip), false);

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
    }
}

using UnityEngine;
using UnityEditor;
using jmayberry.GeneralInfrastructure.Manager;
using System.Linq;

namespace jmayberry.GeneralInfrastructure.Editor {
    [CustomEditor(typeof(WaveManagerBase<,>), true)]
    public class WaveManagerBaseEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            serializedObject.Update();

            SerializedProperty spawnLocationTypeProp = serializedObject.FindProperty("spawnLocationType");
            EditorGUILayout.PropertyField(spawnLocationTypeProp);

            SpawnLocationType spawnLocationType = (SpawnLocationType)spawnLocationTypeProp.enumValueIndex;

            // Draw the properties specific to spawnLocationType
            switch (spawnLocationType) {
                case SpawnLocationType.WithinCircle:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnRadius"), new GUIContent("Spawn Radius"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnRadiusCenter"), new GUIContent("Spawn Radius Center"));
                    break;

                case SpawnLocationType.FromList:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnPoints"), new GUIContent("Spawn Points"));
                    break;

                case SpawnLocationType.WithinBox:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnBox"), new GUIContent("Spawn Box"));
                    break;

                case SpawnLocationType.AlongSpline:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnSpline"), new GUIContent("Spawn Spline"));
                    break;

                case SpawnLocationType.WithinCollider:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnCollider"), new GUIContent("Spawn Collider"));
                    break;
            }

            // Now, manually draw all other properties (excluding those we've already drawn)
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            string[] skipList = {"spawnRadius", "spawnRadiusCenter", "spawnPoints", "spawnBox", "spawnSpline", "spawnCollider"};
            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;

                // Skip properties we've manually handled
                if (skipList.Contains(iterator.name)) {
                    continue;
                }

                EditorGUILayout.PropertyField(iterator, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

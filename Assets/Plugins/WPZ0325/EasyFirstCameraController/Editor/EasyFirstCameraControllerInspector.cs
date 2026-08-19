using UnityEditor;
using UnityEngine;

namespace WPZ0325.EasyFirstCameraController
{
    [CustomEditor(typeof(EasyFirstCameraController))]
    public class EasyFirstCameraControllerInspector : UnityEditor.Editor
    {
        private static readonly string[] _runtimeFieldNames = new string[]
        {
            "_target",
            "_rigidBody",
            "_collider",
            "_moveDir",
            "_rotateDirHorizontal",
            "_rotateDirVertical",
            "_isShifting",
            "_isSpeedUp"
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) 
            { 
                fontSize = 14 ,
                alignment = TextAnchor.MiddleCenter  // 居中
            };
            EditorGUILayout.LabelField("EasyFirstCameraController", titleStyle);
            EditorGUILayout.LabelField("第一人称相机控制器", titleStyle);
            EditorGUILayout.Space(4.0f);

            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (prop.name == "m_Script")
                {
                    continue;
                }
                bool isRuntimeField = System.Array.IndexOf(_runtimeFieldNames, prop.name) >= 0;
                GUI.enabled = !isRuntimeField;
                EditorGUILayout.PropertyField(prop, true);
                GUI.enabled = true;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

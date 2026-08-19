using UnityEditor;
using UnityEngine;

namespace WPZ0325.EasyFirstCameraController
{
    [CustomEditor(typeof(EasyFirstCameraInput))]
    public class EasyFirstCameraInputInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            { 
                fontSize = 14 ,
                alignment = TextAnchor.MiddleCenter  // 居中
            };
            EditorGUILayout.LabelField("EasyFirstCameraInput", titleStyle);
            EditorGUILayout.LabelField("输入模块（按键采样并驱动相机控制器）", titleStyle);
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
                EditorGUILayout.PropertyField(prop, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

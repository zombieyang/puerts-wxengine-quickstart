
using System;
using UnityEditor;
using UnityEngine;

namespace PuertsBeefBallSDK {
    [Serializable]
    public class Prefab {
        public GameObject gameObject;

        public GameObject Instantiate() {
            return UnityEngine.GameObject.Instantiate(gameObject);
        }
    }

    [Serializable]
    public class RemoteResource {
        public UnityEngine.Object resource;

        [HideInInspector]
        public string resourcePath;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RemoteResource))]
    class PrefabEditor: UnityEditor.PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 验证数据正确性，若不正确则清空
            bool isValidValue = false;

            UnityEngine.Object value = property.FindPropertyRelative("resource").objectReferenceValue;
            if (value == null) {
                isValidValue = true;

            } else if (value is GameObject) {
                if (AssetDatabase.GetAssetPath(value.GetInstanceID()).ToLower().Contains("resources")) {
                    isValidValue = true;
                }
            }

            if (!isValidValue) {
                property.FindPropertyRelative("resource").objectReferenceValue = null;
                Debug.LogError("在" + label.text + "字段设置了一个非法资源，现已清空。该资源必须是一个位于resources目录的资源。");
                
            } else {
                property.FindPropertyRelative("resourcePath").stringValue = AssetDatabase.GetAssetPath(value.GetInstanceID());
            }
            
            // 绘制
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(
                position, 
                GUIUtility.GetControlID(FocusType.Passive), 
                label
            );
            EditorGUI.PropertyField(
                new Rect(position.x, position.y, position.width, position.height), 
                property.FindPropertyRelative("resource"), 
                GUIContent.none
            );
            EditorGUI.EndProperty();
        }
    }
#endif
}
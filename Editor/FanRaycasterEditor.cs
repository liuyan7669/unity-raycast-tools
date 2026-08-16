using UnityEditor;

namespace Cowart.RaycastTools.Editor
{
    /// <summary>
    /// 扇形射线组件的自定义 Inspector，仅显示当前模式真正生效的参数。
    /// </summary>
    [CustomEditor(typeof(FanRaycaster))]
    [CanEditMultipleObjects]
    public sealed class FanRaycasterEditor : UnityEditor.Editor
    {
        private SerializedProperty scriptProperty;
        private SerializedProperty modeProperty;
        private SerializedProperty angleProperty;
        private SerializedProperty planeRayCountProperty;
        private SerializedProperty verticalAngleProperty;
        private SerializedProperty verticalPlaneCountProperty;
        private SerializedProperty verticalHeightProperty;
        private SerializedProperty distanceProperty;
        private SerializedProperty detectionModeProperty;
        private SerializedProperty layerMaskProperty;
        private SerializedProperty queryTriggerInteractionProperty;
        private SerializedProperty previewColorProperty;
        private SerializedProperty previewOnlyWhenSelectedProperty;

        private void OnEnable()
        {
            scriptProperty = serializedObject.FindProperty("m_Script");
            modeProperty = serializedObject.FindProperty("mode");
            angleProperty = serializedObject.FindProperty("angle");
            planeRayCountProperty = serializedObject.FindProperty("planeRayCount");
            verticalAngleProperty = serializedObject.FindProperty("verticalAngle");
            verticalPlaneCountProperty = serializedObject.FindProperty("verticalPlaneCount");
            verticalHeightProperty = serializedObject.FindProperty("verticalHeight");
            distanceProperty = serializedObject.FindProperty("distance");
            detectionModeProperty = serializedObject.FindProperty("detectionMode");
            layerMaskProperty = serializedObject.FindProperty("layerMask");
            queryTriggerInteractionProperty = serializedObject.FindProperty("queryTriggerInteraction");
            previewColorProperty = serializedObject.FindProperty("previewColor");
            previewOnlyWhenSelectedProperty = serializedObject.FindProperty("previewOnlyWhenSelected");
        }

        /// <summary>
        /// 绘制扇形射线 Inspector，并根据模式隐藏无效设置以避免误导使用者。
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProperty);
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(modeProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("扇形设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(angleProperty);
            EditorGUILayout.PropertyField(planeRayCountProperty);
            DrawModeProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("物理检测", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(distanceProperty);
            EditorGUILayout.PropertyField(detectionModeProperty);
            EditorGUILayout.PropertyField(layerMaskProperty);
            EditorGUILayout.PropertyField(queryTriggerInteractionProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("编辑器预览", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(previewColorProperty);
            EditorGUILayout.PropertyField(previewOnlyWhenSelectedProperty);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawModeProperties()
        {
            if (modeProperty.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox(
                    "所选组件使用了不同模式。统一 Mode 后会显示该模式的专用参数。",
                    MessageType.Info);
                return;
            }

            switch ((FanRaycastMode)modeProperty.enumValueIndex)
            {
                case FanRaycastMode.Volume:
                    EditorGUILayout.PropertyField(verticalAngleProperty);
                    EditorGUILayout.PropertyField(verticalPlaneCountProperty);
                    break;
                case FanRaycastMode.Solid3D:
                    EditorGUILayout.PropertyField(verticalHeightProperty);
                    EditorGUILayout.PropertyField(verticalPlaneCountProperty);
                    break;
            }
        }
    }
}

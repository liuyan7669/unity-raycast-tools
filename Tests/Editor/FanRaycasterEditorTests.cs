using System.Reflection;
using Cowart.RaycastUtilities.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Cowart.RaycastUtilities.Tests
{
    /// <summary>
    /// 验证扇形射线自定义 Inspector 的创建和序列化字段绑定。
    /// </summary>
    public sealed class FanRaycasterEditorTests
    {
        private static readonly string[] SerializedPropertyFieldNames =
        {
            "scriptProperty",
            "modeProperty",
            "angleProperty",
            "planeRayCountProperty",
            "verticalAngleProperty",
            "verticalPlaneCountProperty",
            "verticalHeightProperty",
            "distanceProperty",
            "detectionModeProperty",
            "layerMaskProperty",
            "queryTriggerInteractionProperty",
            "previewColorProperty",
            "previewOnlyWhenSelectedProperty"
        };

        /// <summary>
        /// 验证 Unity 会创建模式感知 Inspector，且全部显示字段都能解析到序列化属性。
        /// </summary>
        [Test]
        public void UnityCreatesModeAwareEditorAndResolvesAllProperties()
        {
            var gameObject = new GameObject("Fan Raycaster Editor Test");
            var raycaster = gameObject.AddComponent<FanRaycaster>();
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(raycaster);

            try
            {
                Assert.That(editor, Is.TypeOf<FanRaycasterEditor>());

                foreach (string fieldName in SerializedPropertyFieldNames)
                {
                    FieldInfo field = typeof(FanRaycasterEditor).GetField(
                        fieldName,
                        BindingFlags.Instance | BindingFlags.NonPublic);

                    Assert.That(field, Is.Not.Null, $"缺少 Inspector 字段：{fieldName}");
                    Assert.That(
                        field.GetValue(editor),
                        Is.Not.Null,
                        $"未找到对应的序列化属性：{fieldName}");
                }
            }
            finally
            {
                Object.DestroyImmediate(editor);
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace Cowart.RaycastTools.Tests
{
    /// <summary>
    /// 验证方块射线的方向、起点和尺寸换算。
    /// </summary>
    public sealed class BoxRaycasterTests
    {
        private GameObject raycasterObject;
        private GameObject target;
        private BoxRaycaster raycaster;

        /// <summary>
        /// 为每个测试创建方块射线组件和前方目标物体。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            raycasterObject = new GameObject("Box Raycaster Test");
            raycaster = raycasterObject.AddComponent<BoxRaycaster>();
            raycaster.HalfExtents = Vector3.one * 0.5f;
            raycaster.Distance = 5f;

            target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "Box Raycaster Target";
            target.transform.position = new Vector3(0f, 0f, 3f);
            Physics.SyncTransforms();
        }

        /// <summary>
        /// 销毁当前测试创建的临时物体，避免影响后续测试。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(raycasterObject);
        }

        /// <summary>
        /// 验证方块投射从组件物体位置沿自身本地 +Z 轴前进。
        /// </summary>
        [Test]
        public void CastTravelsAlongLocalForwardFromTransformPosition()
        {
            bool didHit = raycaster.Cast(out RaycastHit hit);

            Assert.That(didHit, Is.True);
            Assert.That(hit.collider.gameObject, Is.EqualTo(target));
        }

        /// <summary>
        /// 验证世界半尺寸可按配置选择是否跟随物体缩放。
        /// </summary>
        [Test]
        public void WorldHalfExtentsCanFollowTransformScale()
        {
            raycaster.HalfExtents = new Vector3(0.5f, 1f, 1.5f);
            raycasterObject.transform.localScale = new Vector3(2f, 3f, 4f);
            raycaster.ScaleWithTransform = true;

            Assert.That(raycaster.WorldHalfExtents, Is.EqualTo(new Vector3(1f, 3f, 6f)));

            raycaster.ScaleWithTransform = false;

            Assert.That(raycaster.WorldHalfExtents, Is.EqualTo(new Vector3(0.5f, 1f, 1.5f)));
        }
    }
}

using System;
using NUnit.Framework;
using UnityEngine;

namespace Cowart.RaycastTools.Tests
{
    /// <summary>
    /// 验证不同射线组件共用的结果、事件、检测模式和去重规则。
    /// </summary>
    public sealed class UnifiedRaycasterTests
    {
        private GameObject raycasterObject;
        private GameObject nearTarget;
        private GameObject farTarget;

        /// <summary>
        /// 为每个测试创建射线载体以及远近两个目标物体。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            raycasterObject = new GameObject("Unified Raycaster Test");
            nearTarget = CreateTarget("Near Target", new Vector3(0f, 0f, 2f));
            farTarget = CreateTarget("Far Target", new Vector3(0f, 0f, 4f));
            Physics.SyncTransforms();
        }

        /// <summary>
        /// 销毁当前测试创建的所有临时物体。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(farTarget);
            UnityEngine.Object.DestroyImmediate(nearTarget);
            UnityEngine.Object.DestroyImmediate(raycasterObject);
        }

        /// <summary>
        /// 验证直线、扇形和方块射线都返回同一结果类型、触发同一事件并支持两种检测模式。
        /// </summary>
        [Test]
        public void AllRaycastersUseSameCastResultEventAndDetectionModes()
        {
            var straightRaycaster = raycasterObject.AddComponent<StraightRaycaster>();
            var fanRaycaster = raycasterObject.AddComponent<FanRaycaster>();
            fanRaycaster.Angle = 0f;
            fanRaycaster.PlaneRayCount = 1;
            var boxRaycaster = raycasterObject.AddComponent<BoxRaycaster>();
            boxRaycaster.HalfExtents = Vector3.one * 0.25f;

            RaycasterBase[] raycasters =
            {
                straightRaycaster,
                fanRaycaster,
                boxRaycaster
            };

            foreach (RaycasterBase raycaster in raycasters)
            {
                AssertUnifiedCast(raycaster, RaycastDetectionMode.ClosestOnly, 1);
                AssertUnifiedCast(raycaster, RaycastDetectionMode.AllHits, 2);
            }
        }

        /// <summary>
        /// 验证同一 Collider 被扇形中的多条射线命中时，统一结果只保留最近的一条。
        /// </summary>
        [Test]
        public void FanResultKeepsOnlyClosestHitForSameCollider()
        {
            UnityEngine.Object.DestroyImmediate(farTarget);
            farTarget = null;
            nearTarget.transform.localScale = new Vector3(4f, 2f, 0.5f);
            var fanRaycaster = raycasterObject.AddComponent<FanRaycaster>();
            fanRaycaster.Mode = FanRaycastMode.Plane;
            fanRaycaster.Angle = 30f;
            fanRaycaster.PlaneRayCount = 5;
            fanRaycaster.DetectionMode = RaycastDetectionMode.AllHits;
            Physics.SyncTransforms();

            RaycastResult result = fanRaycaster.Cast();

            Assert.That(result.HitCount, Is.EqualTo(1));
            Assert.That(result.Hits[0].Hit.collider.gameObject, Is.EqualTo(nearTarget));
        }

        private void AssertUnifiedCast(
            RaycasterBase raycaster,
            RaycastDetectionMode detectionMode,
            int expectedHitCount)
        {
            raycaster.Distance = 6f;
            raycaster.DetectionMode = detectionMode;
            int eventCount = 0;
            RaycastResult eventResult = null;
            Action<RaycastResult> handler = result =>
            {
                eventCount++;
                eventResult = result;
            };

            raycaster.CastCompleted += handler;

            try
            {
                RaycastResult returnedResult = raycaster.Cast();

                Assert.That(eventCount, Is.EqualTo(1));
                Assert.That(eventResult, Is.SameAs(returnedResult));
                Assert.That(returnedResult.Source, Is.SameAs(raycaster));
                Assert.That(returnedResult.DetectionMode, Is.EqualTo(detectionMode));
                Assert.That(returnedResult.HitCount, Is.EqualTo(expectedHitCount));
                Assert.That(returnedResult.Hits[0].Hit.collider.gameObject, Is.EqualTo(nearTarget));

                if (expectedHitCount > 1)
                {
                    Assert.That(returnedResult.Hits[1].Hit.collider.gameObject, Is.EqualTo(farTarget));
                    Assert.That(
                        returnedResult.Hits[0].Hit.distance,
                        Is.LessThan(returnedResult.Hits[1].Hit.distance));
                }
            }
            finally
            {
                raycaster.CastCompleted -= handler;
            }
        }

        private static GameObject CreateTarget(string name, Vector3 position)
        {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = name;
            target.transform.position = position;
            target.transform.localScale = Vector3.one * 0.5f;
            return target;
        }
    }
}

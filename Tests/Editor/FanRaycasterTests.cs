using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Cowart.RaycastUtilities.Tests
{
    /// <summary>
    /// 验证三种扇形模式生成的方向、起点和实际命中行为。
    /// </summary>
    public sealed class FanRaycasterTests
    {
        private GameObject gameObject;
        private FanRaycaster raycaster;

        /// <summary>
        /// 为每个测试创建扇形射线组件。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("Fan Raycaster Test");
            raycaster = gameObject.AddComponent<FanRaycaster>();
        }

        /// <summary>
        /// 销毁当前测试创建的临时物体。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        /// <summary>
        /// 验证 Plane 模式会在完整水平张角内均匀生成射线。
        /// </summary>
        [Test]
        public void PlaneModeCreatesDirectionsAcrossFullAngle()
        {
            raycaster.Mode = FanRaycastMode.Plane;
            raycaster.Angle = 90f;
            raycaster.PlaneRayCount = 5;
            var directions = new List<Vector3>();

            int count = raycaster.GetWorldDirections(directions);

            Assert.That(count, Is.EqualTo(5));
            Assert.That(Vector3.Angle(directions[0], Vector3.forward), Is.EqualTo(45f).Within(0.001f));
            Assert.That(directions[2], Is.EqualTo(Vector3.forward).Using(Vector3Comparer.Instance));
            Assert.That(Vector3.Angle(directions[4], Vector3.forward), Is.EqualTo(45f).Within(0.001f));
            Assert.That(directions[0].y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(directions[4].y, Is.EqualTo(0f).Within(0.0001f));
        }

        /// <summary>
        /// 验证 Volume 模式会从同一起点生成上下旋转的多个平面扇形。
        /// </summary>
        [Test]
        public void VolumeModeCreatesVerticallyStackedPlaneFans()
        {
            raycaster.Mode = FanRaycastMode.Volume;
            raycaster.Angle = 90f;
            raycaster.PlaneRayCount = 3;
            raycaster.VerticalAngle = 60f;
            raycaster.VerticalPlaneCount = 3;
            var directions = new List<Vector3>();

            int count = raycaster.GetWorldDirections(directions);

            Assert.That(count, Is.EqualTo(9));

            Vector3 upperCenter = Quaternion.AngleAxis(-30f, Vector3.right) * Vector3.forward;
            Vector3 middleCenter = Vector3.forward;
            Vector3 lowerCenter = Quaternion.AngleAxis(30f, Vector3.right) * Vector3.forward;

            Assert.That(directions[1], Is.EqualTo(upperCenter).Using(Vector3Comparer.Instance));
            Assert.That(directions[4], Is.EqualTo(middleCenter).Using(Vector3Comparer.Instance));
            Assert.That(directions[7], Is.EqualTo(lowerCenter).Using(Vector3Comparer.Instance));

            foreach (Vector3 direction in directions)
            {
                Assert.That(direction.magnitude, Is.EqualTo(1f).Within(0.0001f));
            }

            Assert.That(Vector3.Angle(directions[0], upperCenter), Is.EqualTo(45f).Within(0.001f));
            Assert.That(Vector3.Angle(directions[2], upperCenter), Is.EqualTo(45f).Within(0.001f));
            Assert.That(Vector3.Angle(directions[3], middleCenter), Is.EqualTo(45f).Within(0.001f));
            Assert.That(Vector3.Angle(directions[5], middleCenter), Is.EqualTo(45f).Within(0.001f));
            Assert.That(Vector3.Angle(directions[6], lowerCenter), Is.EqualTo(45f).Within(0.001f));
            Assert.That(Vector3.Angle(directions[8], lowerCenter), Is.EqualTo(45f).Within(0.001f));
        }

        /// <summary>
        /// 验证 Solid3D 模式会沿物体本地 Y 轴复制并平移平行扇面。
        /// </summary>
        [Test]
        public void Solid3DModeCopiesParallelFansAlongLocalUp()
        {
            gameObject.transform.position = new Vector3(2f, 3f, 4f);
            gameObject.transform.rotation = Quaternion.Euler(0f, 30f, 20f);
            raycaster.Mode = FanRaycastMode.Solid3D;
            raycaster.Angle = 90f;
            raycaster.PlaneRayCount = 3;
            raycaster.VerticalHeight = 2f;
            raycaster.VerticalPlaneCount = 3;
            var samples = new List<RaycastSample>();

            int count = raycaster.GetWorldSamples(samples);

            Assert.That(count, Is.EqualTo(9));

            Vector3 expectedLowerOrigin = gameObject.transform.position - gameObject.transform.up;
            Vector3 expectedMiddleOrigin = gameObject.transform.position;
            Vector3 expectedUpperOrigin = gameObject.transform.position + gameObject.transform.up;

            Assert.That(samples[0].Origin, Is.EqualTo(expectedLowerOrigin).Using(Vector3Comparer.Instance));
            Assert.That(samples[3].Origin, Is.EqualTo(expectedMiddleOrigin).Using(Vector3Comparer.Instance));
            Assert.That(samples[6].Origin, Is.EqualTo(expectedUpperOrigin).Using(Vector3Comparer.Instance));

            for (int rayIndex = 0; rayIndex < 3; rayIndex++)
            {
                Assert.That(samples[rayIndex].Direction, Is.EqualTo(samples[rayIndex + 3].Direction).Using(Vector3Comparer.Instance));
                Assert.That(samples[rayIndex].Direction, Is.EqualTo(samples[rayIndex + 6].Direction).Using(Vector3Comparer.Instance));
            }
        }

        /// <summary>
        /// 验证 Solid3D 实际检测会分别使用每个平行扇面的世界起点。
        /// </summary>
        [Test]
        public void Solid3DCastUsesEachParallelPlaneOrigin()
        {
            raycaster.Mode = FanRaycastMode.Solid3D;
            raycaster.Angle = 0f;
            raycaster.PlaneRayCount = 1;
            raycaster.VerticalHeight = 2f;
            raycaster.VerticalPlaneCount = 3;
            raycaster.Distance = 5f;
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.transform.position = new Vector3(0f, 1f, 3f);
            target.transform.localScale = Vector3.one * 0.5f;
            Physics.SyncTransforms();
            var hits = new List<RaycastSampleHit>();

            try
            {
                int count = raycaster.CastAll(hits);

                Assert.That(count, Is.EqualTo(1));
                Assert.That(hits[0].Origin, Is.EqualTo(Vector3.up).Using(Vector3Comparer.Instance));
                Assert.That(hits[0].Hit.collider.gameObject, Is.EqualTo(target));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        private sealed class Vector3Comparer : IEqualityComparer<Vector3>
        {
            /// <summary>
            /// 获取测试共用的向量近似比较器实例。
            /// </summary>
            public static readonly Vector3Comparer Instance = new Vector3Comparer();

            /// <summary>
            /// 判断两个向量是否在测试允许的误差范围内相等。
            /// </summary>
            /// <param name="a">第一个向量。</param>
            /// <param name="b">第二个向量。</param>
            /// <returns>两个向量距离小于误差阈值时为 true。</returns>
            public bool Equals(Vector3 a, Vector3 b)
            {
                return Vector3.Distance(a, b) < 0.0001f;
            }

            /// <summary>
            /// 获取指定向量的哈希码。
            /// </summary>
            /// <param name="value">需要计算哈希码的向量。</param>
            /// <returns>向量自身生成的哈希码。</returns>
            public int GetHashCode(Vector3 value)
            {
                return value.GetHashCode();
            }
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Cowart.RaycastTools.Tests
{
    /// <summary>
    /// 验证直线射线方向与物体本地坐标轴的映射规则。
    /// </summary>
    public sealed class StraightRaycasterTests
    {
        private GameObject gameObject;
        private StraightRaycaster raycaster;

        /// <summary>
        /// 为每个测试创建直线射线组件。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("Straight Raycaster Test");
            raycaster = gameObject.AddComponent<StraightRaycaster>();
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
        /// 验证前、后、左、右始终使用组件物体旋转后的本地坐标轴。
        /// </summary>
        [Test]
        public void DirectionsUseTransformLocalAxes()
        {
            gameObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            Assert.That(raycaster.GetWorldDirection(RayDirections.Forward), Is.EqualTo(Vector3.right).Using(Vector3Comparer.Instance));
            Assert.That(raycaster.GetWorldDirection(RayDirections.Back), Is.EqualTo(Vector3.left).Using(Vector3Comparer.Instance));
            Assert.That(raycaster.GetWorldDirection(RayDirections.Left), Is.EqualTo(Vector3.forward).Using(Vector3Comparer.Instance));
            Assert.That(raycaster.GetWorldDirection(RayDirections.Right), Is.EqualTo(Vector3.back).Using(Vector3Comparer.Instance));
        }

        /// <summary>
        /// 验证只接受单一方向的接口会拒绝方向组合值。
        /// </summary>
        [Test]
        public void CombinedDirectionIsRejected()
        {
            Vector3 direction = raycaster.GetWorldDirection(RayDirections.Forward | RayDirections.Right);

            Assert.That(direction, Is.EqualTo(Vector3.zero));
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

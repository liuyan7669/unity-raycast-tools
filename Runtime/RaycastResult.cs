using System.Collections.Generic;
using UnityEngine;

namespace Cowart.RaycastUtilities
{
    /// <summary>
    /// 一次统一射线检测的结果，记录来源组件、检测方式以及按 Collider 去重后的命中列表。
    /// </summary>
    /// <remarks>同一 Collider 被多条射线命中时只保留距离最近的一条，列表按 <see cref="RaycastHit.distance"/> 升序排列。</remarks>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Cowart.RaycastTools", sourceAssembly: "Cowart.RaycastTools", sourceClassName: "RaycastResult")]
    public sealed class RaycastResult
    {
        private sealed class HitDistanceComparer : IComparer<RaycastSampleHit>
        {
            /// <summary>
            /// 获取命中距离比较器的共用实例。
            /// </summary>
            public static readonly HitDistanceComparer Instance = new HitDistanceComparer();

            /// <summary>
            /// 按 Unity 物理命中距离比较两条采样命中。
            /// </summary>
            /// <param name="x">第一条采样命中。</param>
            /// <param name="y">第二条采样命中。</param>
            /// <returns>小于 0 表示 x 更近，大于 0 表示 y 更近，等于 0 表示距离相同。</returns>
            public int Compare(RaycastSampleHit x, RaycastSampleHit y)
            {
                return x.Hit.distance.CompareTo(y.Hit.distance);
            }
        }

        internal RaycastResult(
            RaycasterBase source,
            RaycastDetectionMode detectionMode,
            List<RaycastSampleHit> rawHits)
        {
            Source = source;
            DetectionMode = detectionMode;
            Hits = new List<RaycastSampleHit>(rawHits.Count);
            var colliderIndices = new Dictionary<Collider, int>();

            foreach (RaycastSampleHit sampleHit in rawHits)
            {
                Collider collider = sampleHit.Hit.collider;

                if (collider == null)
                {
                    continue;
                }

                if (colliderIndices.TryGetValue(collider, out int existingIndex))
                {
                    if (sampleHit.Hit.distance < Hits[existingIndex].Hit.distance)
                    {
                        Hits[existingIndex] = sampleHit;
                    }

                    continue;
                }

                colliderIndices.Add(collider, Hits.Count);
                Hits.Add(sampleHit);
            }

            Hits.Sort(HitDistanceComparer.Instance);
        }

        /// <summary>
        /// 获取发起本次检测的射线组件。
        /// </summary>
        public RaycasterBase Source { get; }

        /// <summary>
        /// 获取本次检测实际使用的命中收集方式。
        /// </summary>
        public RaycastDetectionMode DetectionMode { get; }

        /// <summary>
        /// 获取按 Collider 去重并按距离从近到远排列的命中列表。
        /// </summary>
        /// <remarks>列表属于本次结果对象且允许调用方修改；如需保留原始结果，请避免直接增删其中元素。</remarks>
        public List<RaycastSampleHit> Hits { get; }

        /// <summary>
        /// 获取本次检测是否至少命中一个有效 Collider。
        /// </summary>
        public bool HasHit => Hits.Count > 0;

        /// <summary>
        /// 获取去重后的命中 Collider 数量。
        /// </summary>
        public int HitCount => Hits.Count;

        /// <summary>
        /// 尝试获取距离最近的命中信息。
        /// </summary>
        /// <param name="hit">存在命中时返回列表中的第一条；无命中时返回默认值。</param>
        /// <returns>存在至少一条有效命中时为 true，否则为 false。</returns>
        public bool TryGetClosestHit(out RaycastSampleHit hit)
        {
            if (Hits.Count > 0)
            {
                hit = Hits[0];
                return true;
            }

            hit = default;
            return false;
        }
    }
}

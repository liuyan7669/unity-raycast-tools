using System.Collections.Generic;
using UnityEngine;

namespace Cowart.RaycastUtilities
{
    /// <summary>
    /// 从脚本所绑定物体的位置沿自身本地 +Z 轴执行方块形状投射。
    /// </summary>
    [AddComponentMenu("Cowart/射线/盒形射线检测")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Cowart.RaycastTools", sourceAssembly: "Cowart.RaycastTools", sourceClassName: "BoxRaycaster")]
    [Cowart.InspectorGroups.HideScript]
    public sealed class BoxRaycaster : RaycasterBase
    {
        [Cowart.InspectorGroups.FoldoutGroup("盒形设置", true, style: Cowart.InspectorGroups.InspectorGroupStyle.Framework)]
        [SerializeField]
        [Tooltip("起点方块的本地半尺寸。")]
        private Vector3 halfExtents = Vector3.one * 0.5f;

        [SerializeField]
        [Tooltip("启用后，方块尺寸会乘以挂载物体的 lossyScale。")]
        private bool scaleWithTransform = true;

        /// <summary>
        /// 获取或设置起点方块在物体本地空间中的半尺寸，各分量小于 0 时会自动限制为 0。
        /// </summary>
        public Vector3 HalfExtents
        {
            get => halfExtents;
            set => halfExtents = ClampHalfExtents(value);
        }

        /// <summary>
        /// 获取或设置方块尺寸是否乘以脚本所绑定物体的世界缩放。
        /// </summary>
        public bool ScaleWithTransform
        {
            get => scaleWithTransform;
            set => scaleWithTransform = value;
        }

        /// <summary>
        /// 获取实际参与检测和预览的世界半尺寸。
        /// </summary>
        /// <remarks><see cref="ScaleWithTransform"/> 为 true 时会使用 <see cref="Transform.lossyScale"/> 的绝对值。</remarks>
        public Vector3 WorldHalfExtents
        {
            get
            {
                if (!scaleWithTransform)
                {
                    return halfExtents;
                }

                Vector3 scale = transform.lossyScale;
                scale.x = Mathf.Abs(scale.x);
                scale.y = Mathf.Abs(scale.y);
                scale.z = Mathf.Abs(scale.z);
                return Vector3.Scale(halfExtents, scale);
            }
        }

        /// <summary>
        /// 沿物体自身前方执行一次仅取最近命中的方块投射。
        /// </summary>
        /// <param name="hit">检测成功时返回最近的 Unity 物理命中信息。</param>
        /// <returns>方块投射命中 Collider 时为 true，否则为 false。</returns>
        /// <remarks>此便捷接口不读取 <see cref="RaycasterBase.DetectionMode"/>，也不触发 <see cref="RaycasterBase.CastCompleted"/>；需要统一结果时请调用无参数 <see cref="Cast()"/>。</remarks>
        public bool Cast(out RaycastHit hit)
        {
            return Physics.BoxCast(
                Origin,
                WorldHalfExtents,
                Forward,
                out hit,
                transform.rotation,
                Distance,
                LayerMask,
                QueryTriggerInteraction);
        }

        /// <summary>
        /// 沿物体自身前方执行统一方块投射。
        /// </summary>
        /// <returns>按 Collider 去重并按距离排序的统一结果，同时会触发 <see cref="RaycasterBase.CastCompleted"/>。</returns>
        public override RaycastResult Cast()
        {
            return base.Cast();
        }

        /// <summary>
        /// 使用调用方提供的数组执行不分配结果数组的方块投射。
        /// </summary>
        /// <param name="results">用于接收命中的数组，不能为 null；数组容量不足时多余结果不会写入。</param>
        /// <returns>实际写入数组的命中数量。</returns>
        /// <remarks>此接口直接使用 Unity BoxCastNonAlloc，不按 <see cref="RaycasterBase.DetectionMode"/> 筛选、不统一去重排序，也不触发完成事件。</remarks>
        public int CastNonAlloc(RaycastHit[] results)
        {
            if (results == null)
            {
                throw new System.ArgumentNullException(nameof(results));
            }

            return Physics.BoxCastNonAlloc(
                Origin,
                WorldHalfExtents,
                Forward,
                results,
                transform.rotation,
                Distance,
                LayerMask,
                QueryTriggerInteraction);
        }

        /// <summary>
        /// Inspector 数据变化时校正方块半尺寸和公共参数。
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            halfExtents = ClampHalfExtents(halfExtents);
        }

        /// <summary>
        /// 绘制方块投射的起始方块、结束方块和扫掠边线。
        /// </summary>
        protected override void DrawPreviewGizmos()
        {
            Vector3 start = Origin;
            Vector3 end = start + Forward * Distance;
            Vector3 worldHalfExtents = WorldHalfExtents;

            DrawWireBox(start, worldHalfExtents);
            DrawWireBox(end, worldHalfExtents);
            Gizmos.DrawLine(start, end);

            Vector3 right = transform.right * worldHalfExtents.x;
            Vector3 up = transform.up * worldHalfExtents.y;
            Vector3 forward = transform.forward * worldHalfExtents.z;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 offset = right * x + up * y + forward * z;
                        Gizmos.DrawLine(start + offset, end + offset);
                    }
                }
            }
        }

        /// <summary>
        /// 按当前检测方式收集方块投射的原始命中。
        /// </summary>
        /// <param name="results">接收原始命中的列表。</param>
        protected override void CollectHits(List<RaycastSampleHit> results)
        {
            if (DetectionMode == RaycastDetectionMode.ClosestOnly)
            {
                if (Cast(out RaycastHit closestHit))
                {
                    results.Add(new RaycastSampleHit(Origin, Forward, closestHit));
                }

                return;
            }

            RaycastHit[] hits = Physics.BoxCastAll(
                Origin,
                WorldHalfExtents,
                Forward,
                transform.rotation,
                Distance,
                LayerMask,
                QueryTriggerInteraction);

            foreach (RaycastHit hit in hits)
            {
                results.Add(new RaycastSampleHit(Origin, Forward, hit));
            }
        }

        private void DrawWireBox(Vector3 center, Vector3 worldHalfExtents)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, worldHalfExtents * 2f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = previousMatrix;
        }

        private static Vector3 ClampHalfExtents(Vector3 value)
        {
            value.x = Mathf.Max(0f, value.x);
            value.y = Mathf.Max(0f, value.y);
            value.z = Mathf.Max(0f, value.z);
            return value;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Cowart.RaycastUtilities
{
    /// <summary>
    /// 从脚本所绑定物体的位置沿自身前、后、左、右方向执行直线射线检测。
    /// </summary>
    /// <remarks>前方固定为本地 +Z 轴，后、左、右均以该自身前方为参考。</remarks>
    [AddComponentMenu("Cowart/射线/直线射线检测")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Cowart.RaycastTools", sourceAssembly: "Cowart.RaycastTools", sourceClassName: "StraightRaycaster")]
    [Cowart.InspectorGroups.HideScript]
    public sealed class StraightRaycaster : RaycasterBase
    {
        private static readonly RayDirections[] DirectionOrder =
        {
            RayDirections.Forward,
            RayDirections.Back,
            RayDirections.Left,
            RayDirections.Right
        };

        [Cowart.InspectorGroups.FoldoutGroup("方向设置", true, style: Cowart.InspectorGroups.InspectorGroupStyle.Framework)]
        [SerializeField]
        [Tooltip("需要检测的方向。前方始终是物体本地 +Z，其他方向以该前方为参考。")]
        private RayDirections directions = RayDirections.Forward;

        private readonly List<RaycastSampleHit> legacyHitBuffer = new List<RaycastSampleHit>();

        /// <summary>
        /// 获取或设置统一检测及预览使用的方向组合；无效标志位会被自动移除。
        /// </summary>
        public RayDirections Directions
        {
            get => directions;
            set => directions = value & RayDirections.All;
        }

        /// <summary>
        /// 沿一个指定的本地方向执行仅取最近命中的直线射线检测。
        /// </summary>
        /// <param name="direction">只能传入 Forward、Back、Left、Right 中的一个；组合值会被视为无效。</param>
        /// <param name="hit">检测成功时返回最近的 Unity 物理命中信息。</param>
        /// <returns>命中 Collider 时为 true；方向无效或未命中时为 false。</returns>
        /// <remarks>此便捷接口不读取 <see cref="RaycasterBase.DetectionMode"/>，也不触发 <see cref="RaycasterBase.CastCompleted"/>；需要统一结果时请调用无参数 <see cref="Cast()"/>。</remarks>
        public bool Cast(RayDirections direction, out RaycastHit hit)
        {
            if (!TryGetWorldDirection(direction, out Vector3 worldDirection))
            {
                hit = default;
                return false;
            }

            return CastRay(worldDirection, out hit);
        }

        /// <summary>
        /// 对 <see cref="Directions"/> 中启用的全部方向执行统一检测。
        /// </summary>
        /// <returns>按 Collider 去重并按距离排序的统一结果，同时会触发 <see cref="RaycasterBase.CastCompleted"/>。</returns>
        public override RaycastResult Cast()
        {
            return base.Cast();
        }

        /// <summary>
        /// 沿物体自身前方（本地 +Z）执行仅取最近命中的检测。
        /// </summary>
        /// <param name="hit">检测成功时返回最近的 Unity 物理命中信息。</param>
        /// <returns>命中 Collider 时为 true，否则为 false。</returns>
        /// <remarks>此便捷接口不会触发统一完成事件。</remarks>
        public bool CastForward(out RaycastHit hit)
        {
            return Cast(RayDirections.Forward, out hit);
        }

        /// <summary>
        /// 沿物体自身后方（本地 -Z）执行仅取最近命中的检测。
        /// </summary>
        /// <param name="hit">检测成功时返回最近的 Unity 物理命中信息。</param>
        /// <returns>命中 Collider 时为 true，否则为 false。</returns>
        /// <remarks>此便捷接口不会触发统一完成事件。</remarks>
        public bool CastBack(out RaycastHit hit)
        {
            return Cast(RayDirections.Back, out hit);
        }

        /// <summary>
        /// 沿物体自身左侧（本地 -X）执行仅取最近命中的检测。
        /// </summary>
        /// <param name="hit">检测成功时返回最近的 Unity 物理命中信息。</param>
        /// <returns>命中 Collider 时为 true，否则为 false。</returns>
        /// <remarks>此便捷接口不会触发统一完成事件。</remarks>
        public bool CastLeft(out RaycastHit hit)
        {
            return Cast(RayDirections.Left, out hit);
        }

        /// <summary>
        /// 沿物体自身右侧（本地 +X）执行仅取最近命中的检测。
        /// </summary>
        /// <param name="hit">检测成功时返回最近的 Unity 物理命中信息。</param>
        /// <returns>命中 Collider 时为 true，否则为 false。</returns>
        /// <remarks>此便捷接口不会触发统一完成事件。</remarks>
        public bool CastRight(out RaycastHit hit)
        {
            return Cast(RayDirections.Right, out hit);
        }

        /// <summary>
        /// 按当前 <see cref="Directions"/> 和 <see cref="RaycasterBase.DetectionMode"/> 收集带方向标识的原始命中。
        /// </summary>
        /// <param name="results">用于接收结果的列表；调用时会先清空，不能传入 null。</param>
        /// <returns>写入列表的命中数量。</returns>
        /// <remarks>此兼容接口不按 Collider 去重、不保证跨方向距离排序且不触发统一完成事件；新代码优先使用无参数 <see cref="Cast()"/>。</remarks>
        public int CastAll(List<DirectionalRaycastHit> results)
        {
            if (results == null)
            {
                throw new System.ArgumentNullException(nameof(results));
            }

            results.Clear();

            foreach (RayDirections direction in DirectionOrder)
            {
                if ((directions & direction) == 0)
                {
                    continue;
                }

                if (!TryGetWorldDirection(direction, out Vector3 worldDirection))
                {
                    continue;
                }

                legacyHitBuffer.Clear();
                CollectRayHits(Origin, worldDirection, legacyHitBuffer);

                foreach (RaycastSampleHit sampleHit in legacyHitBuffer)
                {
                    results.Add(new DirectionalRaycastHit(direction, sampleHit.Hit));
                }
            }

            return results.Count;
        }

        /// <summary>
        /// 将一个直线射线本地方向转换为当前物体的世界空间方向。
        /// </summary>
        /// <param name="direction">只能传入 Forward、Back、Left、Right 中的一个。</param>
        /// <returns>有效单一方向对应的单位世界方向；组合值或无效值返回 <see cref="Vector3.zero"/>。</returns>
        public Vector3 GetWorldDirection(RayDirections direction)
        {
            return TryGetWorldDirection(direction, out Vector3 worldDirection)
                ? worldDirection
                : Vector3.zero;
        }

        /// <summary>
        /// Inspector 数据变化时移除不受支持的方向标志位。
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            directions &= RayDirections.All;
        }

        /// <summary>
        /// 为当前启用的每个直线方向绘制 Scene 视图预览线。
        /// </summary>
        protected override void DrawPreviewGizmos()
        {
            foreach (RayDirections direction in DirectionOrder)
            {
                if ((directions & direction) != 0 && TryGetWorldDirection(direction, out Vector3 worldDirection))
                {
                    DrawPreviewLine(worldDirection);
                }
            }
        }

        /// <summary>
        /// 按当前方向组合和检测方式收集原始命中。
        /// </summary>
        /// <param name="results">接收原始命中的列表。</param>
        protected override void CollectHits(List<RaycastSampleHit> results)
        {
            foreach (RayDirections direction in DirectionOrder)
            {
                if ((directions & direction) != 0 && TryGetWorldDirection(direction, out Vector3 worldDirection))
                {
                    CollectRayHits(Origin, worldDirection, results);
                }
            }
        }

        private bool TryGetWorldDirection(RayDirections direction, out Vector3 worldDirection)
        {
            switch (direction)
            {
                case RayDirections.Forward:
                    worldDirection = transform.forward;
                    return true;
                case RayDirections.Back:
                    worldDirection = -transform.forward;
                    return true;
                case RayDirections.Left:
                    worldDirection = -transform.right;
                    return true;
                case RayDirections.Right:
                    worldDirection = transform.right;
                    return true;
                default:
                    worldDirection = Vector3.zero;
                    return false;
            }
        }
    }
}

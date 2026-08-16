using UnityEngine;

namespace Cowart.RaycastTools
{
    /// <summary>
    /// 直线射线的方向标识及对应的 Unity 物理命中结果。
    /// </summary>
    public readonly struct DirectionalRaycastHit
    {
        /// <summary>
        /// 创建一条带方向标识的直线射线命中信息。
        /// </summary>
        /// <param name="direction">产生本次命中的单一方向。</param>
        /// <param name="hit">Unity 物理系统返回的命中结果。</param>
        public DirectionalRaycastHit(RayDirections direction, RaycastHit hit)
        {
            Direction = direction;
            Hit = hit;
        }

        /// <summary>
        /// 获取产生本次命中的方向。
        /// </summary>
        public RayDirections Direction { get; }

        /// <summary>
        /// 获取 Unity 物理系统返回的命中信息。
        /// </summary>
        public RaycastHit Hit { get; }
    }
}

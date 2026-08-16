using UnityEngine;

namespace Cowart.RaycastTools
{
    /// <summary>
    /// 一条射线采样产生的命中信息，包含检测时使用的起点、方向和 Unity 物理命中结果。
    /// </summary>
    public readonly struct RaycastSampleHit
    {
        /// <summary>
        /// 使用方向和物理命中结果创建兼容旧接口的命中信息。
        /// </summary>
        /// <param name="direction">检测时使用的世界空间方向。</param>
        /// <param name="hit">Unity 物理系统返回的命中结果。</param>
        /// <remarks>此重载无法获知真实起点，因此 <see cref="Origin"/> 为 <see cref="Vector3.zero"/>；新代码建议使用包含起点的重载。</remarks>
        public RaycastSampleHit(Vector3 direction, RaycastHit hit)
            : this(Vector3.zero, direction, hit)
        {
        }

        /// <summary>
        /// 使用完整的射线采样数据创建命中信息。
        /// </summary>
        /// <param name="origin">检测时使用的世界空间起点。</param>
        /// <param name="direction">检测时使用的世界空间方向。</param>
        /// <param name="hit">Unity 物理系统返回的命中结果。</param>
        public RaycastSampleHit(Vector3 origin, Vector3 direction, RaycastHit hit)
        {
            Origin = origin;
            Direction = direction;
            Hit = hit;
        }

        /// <summary>
        /// 获取检测时使用的世界空间起点。
        /// </summary>
        public Vector3 Origin { get; }

        /// <summary>
        /// 获取检测时使用的世界空间方向。
        /// </summary>
        public Vector3 Direction { get; }

        /// <summary>
        /// 获取 Unity 物理系统返回的命中信息。
        /// </summary>
        public RaycastHit Hit { get; }
    }
}

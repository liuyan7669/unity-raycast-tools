using System;

namespace Cowart.RaycastTools
{
    /// <summary>
    /// 直线射线使用的物体本地方向，可通过按位或同时选择多个方向。
    /// </summary>
    [Flags]
    public enum RayDirections
    {
        /// <summary>
        /// 不检测任何方向。
        /// </summary>
        None = 0,

        /// <summary>
        /// 物体自身前方，即本地 +Z 轴。
        /// </summary>
        Forward = 1 << 0,

        /// <summary>
        /// 物体自身后方，即本地 -Z 轴。
        /// </summary>
        Back = 1 << 1,

        /// <summary>
        /// 以物体自身前方为参考的左侧，即本地 -X 轴。
        /// </summary>
        Left = 1 << 2,

        /// <summary>
        /// 以物体自身前方为参考的右侧，即本地 +X 轴。
        /// </summary>
        Right = 1 << 3,

        /// <summary>
        /// 同时检测前、后、左、右四个方向。
        /// </summary>
        All = Forward | Back | Left | Right
    }
}

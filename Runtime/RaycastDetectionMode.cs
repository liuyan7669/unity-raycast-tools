namespace Cowart.RaycastTools
{
    /// <summary>
    /// 射线检测命中的收集方式。
    /// </summary>
    public enum RaycastDetectionMode
    {
        /// <summary>
        /// 每条射线或每次形状投射只检查最近且无遮挡的命中物体。
        /// </summary>
        ClosestOnly,

        /// <summary>
        /// 穿透检测路径上的所有命中物体。
        /// </summary>
        AllHits
    }
}

namespace Cowart.RaycastUtilities
{
    /// <summary>
    /// 射线检测命中的收集方式。
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Cowart.RaycastTools", sourceAssembly: "Cowart.RaycastTools", sourceClassName: "RaycastDetectionMode")]
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

using UnityEngine;

namespace Cowart.RaycastUtilities
{
    /// <summary>
    /// 一条待检测射线的世界空间起点和方向。
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Cowart.RaycastTools", sourceAssembly: "Cowart.RaycastTools", sourceClassName: "RaycastSample")]
    public readonly struct RaycastSample
    {
        /// <summary>
        /// 创建一条射线采样数据。
        /// </summary>
        /// <param name="origin">射线的世界空间起点。</param>
        /// <param name="direction">射线的世界空间方向，调用方应传入非零方向。</param>
        public RaycastSample(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        /// <summary>
        /// 获取射线的世界空间起点。
        /// </summary>
        public Vector3 Origin { get; }

        /// <summary>
        /// 获取射线的世界空间方向。
        /// </summary>
        public Vector3 Direction { get; }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cowart.RaycastTools
{
    /// <summary>
    /// 扇形射线的空间生成方式。
    /// </summary>
    public enum FanRaycastMode
    {
        /// <summary>
        /// 以物体自身本地 +Z 为中心，在本地 XZ 平面生成一个水平扇面。
        /// </summary>
        Plane,

        /// <summary>
        /// 在 Plane 基础上从同一起点绕本地 X 轴上下旋转多个扇面。
        /// </summary>
        Volume,

        /// <summary>
        /// 复制 Plane 扇面并沿物体自身本地 Y 轴上下平移，组成有高度的立体扇形。
        /// </summary>
        Solid3D
    }

    /// <summary>
    /// 从脚本所绑定物体生成平面扇形、垂直张开的多扇面或上下平移的立体扇形射线。
    /// </summary>
    [AddComponentMenu("Cowart/Raycast Tools/Fan Raycaster")]
    public sealed class FanRaycaster : RaycasterBase
    {
        private readonly List<RaycastSample> sampleBuffer = new List<RaycastSample>();

        [SerializeField]
        [Tooltip("Plane 是单个扇面；Volume 从同一起点上下张开；Solid3D 沿本地 Y 轴平移多个平行扇面。")]
        private FanRaycastMode mode = FanRaycastMode.Plane;

        [SerializeField, Range(0f, 360f)]
        [Tooltip("每个扇面的水平完整张角。")]
        private float angle = 90f;

        [SerializeField, Range(1, 128)]
        [Tooltip("每个扇面从左到右的射线数量。")]
        private int planeRayCount = 9;

        [SerializeField, Range(0f, 180f)]
        [Tooltip("Volume 模式从最下层到最上层的垂直完整张角。")]
        private float verticalAngle = 60f;

        [FormerlySerializedAs("volumeRingCount")]
        [SerializeField, Range(1, 32)]
        [Tooltip("Volume 或 Solid3D 模式的垂直扇面数量。")]
        private int verticalPlaneCount = 5;

        [SerializeField, Min(0f)]
        [Tooltip("Solid3D 模式从最下层到最上层的总高度。")]
        private float verticalHeight = 2f;

        /// <summary>
        /// 获取或设置扇形的生成模式。
        /// </summary>
        public FanRaycastMode Mode
        {
            get => mode;
            set
            {
                mode = value;
                ClampValues();
            }
        }

        /// <summary>
        /// 获取或设置每个扇面的水平完整张角，单位为度，有效范围为 0 到 360。
        /// </summary>
        public float Angle
        {
            get => angle;
            set
            {
                angle = value;
                ClampValues();
            }
        }

        /// <summary>
        /// 获取或设置每个扇面从左到右的射线数量，有效范围为 1 到 128。
        /// </summary>
        /// <remarks>张角为 0 时只生成一条中心射线，即使该值大于 1。</remarks>
        public int PlaneRayCount
        {
            get => planeRayCount;
            set => planeRayCount = Mathf.Clamp(value, 1, 128);
        }

        /// <summary>
        /// 获取或设置 Volume 模式从最下层到最上层的垂直完整张角，单位为度，有效范围为 0 到 180。
        /// </summary>
        /// <remarks>该属性只影响 Volume 模式。</remarks>
        public float VerticalAngle
        {
            get => verticalAngle;
            set => verticalAngle = Mathf.Clamp(value, 0f, 180f);
        }

        /// <summary>
        /// 获取或设置 Volume 或 Solid3D 模式的垂直扇面数量，有效范围为 1 到 32。
        /// </summary>
        /// <remarks>Plane 模式不使用该属性；垂直角度或总高度为 0 时对应模式只生成一个有效扇面。</remarks>
        public int VerticalPlaneCount
        {
            get => verticalPlaneCount;
            set => verticalPlaneCount = Mathf.Clamp(value, 1, 32);
        }

        /// <summary>
        /// 获取或设置 Solid3D 模式最下层到最上层的总高度，小于 0 时会自动限制为 0。
        /// </summary>
        /// <remarks>该属性只影响 Solid3D 模式，高度沿物体自身本地 Y 轴分布。</remarks>
        public float VerticalHeight
        {
            get => verticalHeight;
            set => verticalHeight = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 获取或设置旧版体积扇形的层数，等价于 <see cref="VerticalPlaneCount"/>。
        /// </summary>
        /// <remarks>仅为已有调用保留，新代码请改用 <see cref="VerticalPlaneCount"/>。</remarks>
        [System.Obsolete("Use VerticalPlaneCount instead.")]
        public int VolumeRingCount
        {
            get => VerticalPlaneCount;
            set => VerticalPlaneCount = value;
        }

        /// <summary>
        /// 获取或设置旧版每层射线数量，等价于 <see cref="PlaneRayCount"/>。
        /// </summary>
        /// <remarks>仅为已有调用保留，新代码请改用 <see cref="PlaneRayCount"/>。</remarks>
        [System.Obsolete("Use PlaneRayCount instead.")]
        public int VolumeRaysPerRing
        {
            get => PlaneRayCount;
            set => PlaneRayCount = value;
        }

        /// <summary>
        /// 计算当前模式下所有射线的世界空间方向。
        /// </summary>
        /// <param name="directions">用于接收方向的列表；调用时会先清空，不能传入 null。</param>
        /// <returns>写入列表的方向数量。</returns>
        /// <remarks>Solid3D 的平行扇面会产生重复方向；如需区分每层起点，请使用 <see cref="GetWorldSamples"/>。</remarks>
        public int GetWorldDirections(List<Vector3> directions)
        {
            if (directions == null)
            {
                throw new System.ArgumentNullException(nameof(directions));
            }

            directions.Clear();
            GetWorldSamples(sampleBuffer);

            foreach (RaycastSample sample in sampleBuffer)
            {
                directions.Add(sample.Direction);
            }

            return directions.Count;
        }

        /// <summary>
        /// 计算当前模式下所有射线的世界空间起点和方向。
        /// </summary>
        /// <param name="samples">用于接收采样数据的列表；调用时会先清空，不能传入 null。</param>
        /// <returns>写入列表的采样数量。</returns>
        /// <remarks>Plane 和 Volume 的采样共用组件位置；Solid3D 的采样起点沿物体自身本地 Y 轴分层平移。</remarks>
        public int GetWorldSamples(List<RaycastSample> samples)
        {
            if (samples == null)
            {
                throw new System.ArgumentNullException(nameof(samples));
            }

            samples.Clear();

            switch (mode)
            {
                case FanRaycastMode.Plane:
                    FillPlaneSamples(samples, Origin);
                    break;
                case FanRaycastMode.Volume:
                    FillVolumeSamples(samples);
                    break;
                case FanRaycastMode.Solid3D:
                    FillSolid3DSamples(samples);
                    break;
            }

            return samples.Count;
        }

        /// <summary>
        /// 按当前模式和 <see cref="RaycasterBase.DetectionMode"/> 收集每条采样射线的原始命中。
        /// </summary>
        /// <param name="results">用于接收结果的列表；调用时会先清空，不能传入 null。</param>
        /// <returns>写入列表的原始命中数量。</returns>
        /// <remarks>同一 Collider 可能被多条射线重复记录，且结果不保证全局距离排序，也不会触发统一完成事件；新代码优先使用无参数 <see cref="Cast()"/>。</remarks>
        public int CastAll(List<RaycastSampleHit> results)
        {
            if (results == null)
            {
                throw new System.ArgumentNullException(nameof(results));
            }

            results.Clear();
            CollectHits(results);

            return results.Count;
        }

        /// <summary>
        /// 按当前扇形模式执行统一检测。
        /// </summary>
        /// <returns>按 Collider 去重并按距离排序的统一结果，同时会触发 <see cref="RaycasterBase.CastCompleted"/>。</returns>
        public override RaycastResult Cast()
        {
            return base.Cast();
        }

        /// <summary>
        /// Inspector 数据变化时校正全部扇形参数和公共参数。
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            ClampValues();
        }

        /// <summary>
        /// 按当前模式绘制对应的 Scene 视图扇形预览。
        /// </summary>
        protected override void DrawPreviewGizmos()
        {
            switch (mode)
            {
                case FanRaycastMode.Plane:
                    DrawPlanePreview();
                    break;
                case FanRaycastMode.Volume:
                    DrawVolumePreview();
                    break;
                case FanRaycastMode.Solid3D:
                    DrawSolid3DPreview();
                    break;
            }
        }

        /// <summary>
        /// 按当前模式生成采样，并按检测方式收集原始命中。
        /// </summary>
        /// <param name="results">接收原始命中的列表。</param>
        protected override void CollectHits(List<RaycastSampleHit> results)
        {
            GetWorldSamples(sampleBuffer);

            foreach (RaycastSample sample in sampleBuffer)
            {
                CollectRayHits(sample.Origin, sample.Direction, results);
            }
        }

        private void FillPlaneSamples(List<RaycastSample> samples, Vector3 origin)
        {
            int horizontalRayCount = GetEffectiveHorizontalRayCount();
            float halfAngle = angle * 0.5f;

            for (int rayIndex = 0; rayIndex < horizontalRayCount; rayIndex++)
            {
                float horizontalT = horizontalRayCount == 1 ? 0.5f : rayIndex / (horizontalRayCount - 1f);
                float currentHorizontalAngle = Mathf.Lerp(-halfAngle, halfAngle, horizontalT);
                Vector3 worldDirection = transform.TransformDirection(GetLocalPlaneDirection(currentHorizontalAngle)).normalized;
                samples.Add(new RaycastSample(origin, worldDirection));
            }
        }

        private void FillVolumeSamples(List<RaycastSample> samples)
        {
            int horizontalRayCount = GetEffectiveHorizontalRayCount();
            int planeCount = GetEffectiveVolumePlaneCount();
            float horizontalHalfAngle = angle * 0.5f;
            float verticalHalfAngle = verticalAngle * 0.5f;

            for (int planeIndex = 0; planeIndex < planeCount; planeIndex++)
            {
                float verticalT = planeCount == 1 ? 0.5f : planeIndex / (planeCount - 1f);
                float currentVerticalAngle = Mathf.Lerp(-verticalHalfAngle, verticalHalfAngle, verticalT);

                for (int rayIndex = 0; rayIndex < horizontalRayCount; rayIndex++)
                {
                    float horizontalT = horizontalRayCount == 1 ? 0.5f : rayIndex / (horizontalRayCount - 1f);
                    float currentHorizontalAngle = Mathf.Lerp(-horizontalHalfAngle, horizontalHalfAngle, horizontalT);
                    Vector3 localDirection = GetLocalVolumeDirection(currentHorizontalAngle, currentVerticalAngle);
                    Vector3 worldDirection = transform.TransformDirection(localDirection).normalized;
                    samples.Add(new RaycastSample(Origin, worldDirection));
                }
            }
        }

        private void FillSolid3DSamples(List<RaycastSample> samples)
        {
            int planeCount = GetEffectiveSolid3DPlaneCount();
            float halfHeight = verticalHeight * 0.5f;

            for (int planeIndex = 0; planeIndex < planeCount; planeIndex++)
            {
                float verticalT = planeCount == 1 ? 0.5f : planeIndex / (planeCount - 1f);
                float heightOffset = Mathf.Lerp(-halfHeight, halfHeight, verticalT);
                Vector3 planeOrigin = Origin + transform.up * heightOffset;
                FillPlaneSamples(samples, planeOrigin);
            }
        }

        private void DrawPlanePreview()
        {
            DrawFanPreview(Origin, 0f, out _, out _);
        }

        private void DrawVolumePreview()
        {
            int planeCount = GetEffectiveVolumePlaneCount();
            float verticalHalfAngle = verticalAngle * 0.5f;

            for (int planeIndex = 0; planeIndex < planeCount; planeIndex++)
            {
                float verticalT = planeCount == 1 ? 0.5f : planeIndex / (planeCount - 1f);
                float currentVerticalAngle = Mathf.Lerp(-verticalHalfAngle, verticalHalfAngle, verticalT);
                DrawFanPreview(Origin, currentVerticalAngle, out _, out _);
            }
        }

        private void DrawSolid3DPreview()
        {
            int planeCount = GetEffectiveSolid3DPlaneCount();
            float halfHeight = verticalHeight * 0.5f;
            Vector3 previousOrigin = Vector3.zero;
            Vector3 previousFirstEnd = Vector3.zero;
            Vector3 previousLastEnd = Vector3.zero;

            for (int planeIndex = 0; planeIndex < planeCount; planeIndex++)
            {
                float verticalT = planeCount == 1 ? 0.5f : planeIndex / (planeCount - 1f);
                float heightOffset = Mathf.Lerp(-halfHeight, halfHeight, verticalT);
                Vector3 planeOrigin = Origin + transform.up * heightOffset;
                DrawFanPreview(planeOrigin, 0f, out Vector3 firstEnd, out Vector3 lastEnd);

                if (planeIndex > 0)
                {
                    Gizmos.DrawLine(previousOrigin, planeOrigin);
                    Gizmos.DrawLine(previousFirstEnd, firstEnd);

                    if ((lastEnd - firstEnd).sqrMagnitude > 0.000001f)
                    {
                        Gizmos.DrawLine(previousLastEnd, lastEnd);
                    }
                }

                previousOrigin = planeOrigin;
                previousFirstEnd = firstEnd;
                previousLastEnd = lastEnd;
            }
        }

        private void DrawFanPreview(
            Vector3 origin,
            float verticalRotation,
            out Vector3 firstEnd,
            out Vector3 lastEnd)
        {
            int horizontalRayCount = GetEffectiveHorizontalRayCount();
            float horizontalHalfAngle = angle * 0.5f;
            Vector3 previousEnd = Vector3.zero;
            firstEnd = Vector3.zero;
            lastEnd = Vector3.zero;

            for (int rayIndex = 0; rayIndex < horizontalRayCount; rayIndex++)
            {
                float horizontalT = horizontalRayCount == 1 ? 0.5f : rayIndex / (horizontalRayCount - 1f);
                float currentHorizontalAngle = Mathf.Lerp(-horizontalHalfAngle, horizontalHalfAngle, horizontalT);
                Vector3 localDirection = GetLocalVolumeDirection(currentHorizontalAngle, verticalRotation);
                Vector3 worldDirection = transform.TransformDirection(localDirection).normalized;
                Vector3 end = origin + worldDirection * Distance;
                Gizmos.DrawLine(origin, end);

                if (rayIndex == 0)
                {
                    firstEnd = end;
                }
                else
                {
                    Gizmos.DrawLine(previousEnd, end);
                }

                previousEnd = end;
                lastEnd = end;
            }
        }

        private int GetEffectiveHorizontalRayCount()
        {
            return planeRayCount == 1 || Mathf.Approximately(angle, 0f)
                ? 1
                : planeRayCount;
        }

        private int GetEffectiveVolumePlaneCount()
        {
            return verticalPlaneCount == 1 || Mathf.Approximately(verticalAngle, 0f)
                ? 1
                : verticalPlaneCount;
        }

        private int GetEffectiveSolid3DPlaneCount()
        {
            return verticalPlaneCount == 1 || Mathf.Approximately(verticalHeight, 0f)
                ? 1
                : verticalPlaneCount;
        }

        private static Vector3 GetLocalPlaneDirection(float degrees)
        {
            return Quaternion.AngleAxis(degrees, Vector3.up) * Vector3.forward;
        }

        private static Vector3 GetLocalVolumeDirection(float horizontalAngle, float verticalAngle)
        {
            Vector3 planeDirection = GetLocalPlaneDirection(horizontalAngle);
            return Quaternion.AngleAxis(verticalAngle, Vector3.right) * planeDirection;
        }

        private void ClampValues()
        {
            angle = Mathf.Clamp(angle, 0f, 360f);
            planeRayCount = Mathf.Clamp(planeRayCount, 1, 128);
            verticalAngle = Mathf.Clamp(verticalAngle, 0f, 180f);
            verticalPlaneCount = Mathf.Clamp(verticalPlaneCount, 1, 32);
            verticalHeight = Mathf.Max(0f, verticalHeight);
        }
    }
}

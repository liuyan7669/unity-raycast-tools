using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cowart.RaycastTools
{
    /// <summary>
    /// 所有射线组件的公共基类，统一提供检测参数、编辑器预览、检测结果和完成事件。
    /// </summary>
    /// <remarks>脚本所绑定物体的世界坐标是默认起点，自身本地 +Z 轴是默认前方。</remarks>
    public abstract class RaycasterBase : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        [Tooltip("射线或形状投射的最大距离。")]
        private float distance = 5f;

        [SerializeField]
        [Tooltip("ClosestOnly 只保留每条检测线的最近命中；AllHits 会穿透并收集路径上的全部命中。")]
        private RaycastDetectionMode detectionMode = RaycastDetectionMode.ClosestOnly;

        [SerializeField]
        [Tooltip("参与检测的物理层。")]
        private LayerMask layerMask = ~0;

        [SerializeField]
        [Tooltip("射线是否检测 Trigger，UseGlobal 表示遵循 Physics.queriesHitTriggers。")]
        private QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

        [SerializeField]
        [Tooltip("Scene 视图预览线颜色。")]
        private Color previewColor = Color.green;

        [SerializeField]
        [Tooltip("启用后，仅在选中当前物体时绘制预览。")]
        private bool previewOnlyWhenSelected;

        /// <summary>
        /// 获取或设置最大检测距离，赋值小于 0 时会自动限制为 0。
        /// </summary>
        public float Distance
        {
            get => distance;
            set => distance = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 获取或设置检测方式，决定只检查无遮挡的最近物体还是穿透检查所有物体。
        /// </summary>
        public RaycastDetectionMode DetectionMode
        {
            get => detectionMode;
            set => detectionMode = value;
        }

        /// <summary>
        /// 获取或设置参与物理检测的层遮罩。
        /// </summary>
        public LayerMask LayerMask
        {
            get => layerMask;
            set => layerMask = value;
        }

        /// <summary>
        /// 获取或设置 Trigger 碰撞体的检测规则；UseGlobal 表示遵循 Unity 全局物理设置。
        /// </summary>
        public QueryTriggerInteraction QueryTriggerInteraction
        {
            get => queryTriggerInteraction;
            set => queryTriggerInteraction = value;
        }

        /// <summary>
        /// 获取或设置 Scene 视图中的预览线颜色，默认是绿色。
        /// </summary>
        public Color PreviewColor
        {
            get => previewColor;
            set => previewColor = value;
        }

        /// <summary>
        /// 获取或设置是否仅在选中脚本所绑定物体时绘制 Scene 预览。
        /// </summary>
        public bool PreviewOnlyWhenSelected
        {
            get => previewOnlyWhenSelected;
            set => previewOnlyWhenSelected = value;
        }

        /// <summary>
        /// 获取默认世界空间起点，即脚本所绑定物体的 <see cref="Transform.position"/>。
        /// </summary>
        public Vector3 Origin => transform.position;

        /// <summary>
        /// 获取默认世界空间前方，即脚本所绑定物体自身本地 +Z 轴对应的方向。
        /// </summary>
        public Vector3 Forward => transform.forward;

        /// <summary>
        /// 每次调用统一 <see cref="Cast()"/> 完成后同步触发，并传出与方法返回值相同的结果对象。
        /// </summary>
        /// <remarks>旧版便捷接口不会触发此事件；订阅者应在不再使用时及时取消订阅。</remarks>
        public event Action<RaycastResult> CastCompleted;

        /// <summary>
        /// 按当前组件配置执行一次统一检测，并通过返回值和 <see cref="CastCompleted"/> 事件给出结果。
        /// </summary>
        /// <returns>本次检测的新结果对象；无命中时仍会返回对象，但其命中列表为空。</returns>
        /// <remarks>结果会按 Collider 去重，同一 Collider 被多条射线命中时只保留距离最近的一条，并按命中距离升序排列。</remarks>
        public virtual RaycastResult Cast()
        {
            var rawHits = new List<RaycastSampleHit>();
            CollectHits(rawHits);
            var result = new RaycastResult(this, detectionMode, rawHits);
            CastCompleted?.Invoke(result);
            return result;
        }

        /// <summary>
        /// 从组件默认起点向指定世界方向执行一次仅取最近命中的射线检测。
        /// </summary>
        /// <param name="direction">射线的世界空间方向。</param>
        /// <param name="hit">检测成功时返回最近的 Unity 物理命中信息。</param>
        /// <returns>命中 Collider 时为 true，否则为 false。</returns>
        /// <remarks>此辅助方法不读取 <see cref="DetectionMode"/>；需要按模式收集结果时请使用 <see cref="CollectRayHits"/>。</remarks>
        protected bool CastRay(Vector3 direction, out RaycastHit hit)
        {
            return CastRay(Origin, direction, out hit);
        }

        /// <summary>
        /// 从指定世界起点向指定世界方向执行一次仅取最近命中的射线检测。
        /// </summary>
        /// <param name="origin">射线的世界空间起点。</param>
        /// <param name="direction">射线的世界空间方向。</param>
        /// <param name="hit">检测成功时返回最近的 Unity 物理命中信息。</param>
        /// <returns>命中 Collider 时为 true，否则为 false。</returns>
        /// <remarks>此辅助方法不读取 <see cref="DetectionMode"/>；需要按模式收集结果时请使用 <see cref="CollectRayHits"/>。</remarks>
        protected bool CastRay(Vector3 origin, Vector3 direction, out RaycastHit hit)
        {
            return Physics.Raycast(
                origin,
                direction,
                out hit,
                distance,
                layerMask,
                queryTriggerInteraction);
        }

        /// <summary>
        /// 按当前 <see cref="DetectionMode"/> 收集一条射线的命中信息并追加到指定列表。
        /// </summary>
        /// <param name="origin">射线的世界空间起点。</param>
        /// <param name="direction">射线的世界空间方向。</param>
        /// <param name="results">接收命中的列表；本方法不会主动清空已有内容。</param>
        protected void CollectRayHits(
            Vector3 origin,
            Vector3 direction,
            List<RaycastSampleHit> results)
        {
            if (detectionMode == RaycastDetectionMode.ClosestOnly)
            {
                if (CastRay(origin, direction, out RaycastHit closestHit))
                {
                    results.Add(new RaycastSampleHit(origin, direction, closestHit));
                }

                return;
            }

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                direction,
                distance,
                layerMask,
                queryTriggerInteraction);

            foreach (RaycastHit hit in hits)
            {
                results.Add(new RaycastSampleHit(origin, direction, hit));
            }
        }

        /// <summary>
        /// 由具体射线组件按当前配置收集原始命中并追加到指定列表。
        /// </summary>
        /// <param name="results">接收原始命中的列表；统一结果会在之后负责按 Collider 去重和排序。</param>
        protected abstract void CollectHits(List<RaycastSampleHit> results);

        /// <summary>
        /// Inspector 中修改数据时校正公共参数的有效范围。
        /// </summary>
        protected virtual void OnValidate()
        {
            distance = Mathf.Max(0f, distance);
        }

        /// <summary>
        /// 由具体射线组件绘制其 Scene 视图预览图形。
        /// </summary>
        protected abstract void DrawPreviewGizmos();

        /// <summary>
        /// 从默认起点沿指定方向绘制一条长度为 <see cref="Distance"/> 的预览线。
        /// </summary>
        /// <param name="direction">预览线的世界空间方向，方法内部会进行归一化。</param>
        protected void DrawPreviewLine(Vector3 direction)
        {
            Gizmos.DrawLine(Origin, Origin + direction.normalized * distance);
        }

        /// <summary>
        /// 在未启用“仅选中时预览”时绘制 Scene 视图预览。
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            if (!previewOnlyWhenSelected)
            {
                DrawPreview();
            }
        }

        /// <summary>
        /// 在启用“仅选中时预览”时绘制 Scene 视图预览。
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            if (previewOnlyWhenSelected)
            {
                DrawPreview();
            }
        }

        private void DrawPreview()
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = previewColor;
            DrawPreviewGizmos();
            Gizmos.color = previousColor;
        }
    }
}

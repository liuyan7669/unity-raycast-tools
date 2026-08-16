using System.Collections.Generic;
using UnityEngine;

namespace Cowart.RaycastTools.Samples
{
    /// <summary>
    /// Raycast Tools 全功能示例，集中演示统一事件接口和每种射线的便捷调用方式。
    /// </summary>
    public sealed class RaycastAllFeaturesExample : MonoBehaviour
    {
        [Header("射线组件")]
        [SerializeField]
        [Tooltip("演示前、后、左、右四个自身方向的直线射线。")]
        private StraightRaycaster straightRaycaster;

        [SerializeField]
        [Tooltip("演示单个水平扇面的 Plane 扇形射线。")]
        private FanRaycaster planeFanRaycaster;

        [SerializeField]
        [Tooltip("演示从同一起点上下旋转多个扇面的 Volume 扇形射线。")]
        private FanRaycaster volumeFanRaycaster;

        [SerializeField]
        [Tooltip("演示沿自身 Y 轴上下平移多个平行扇面的 Solid3D 扇形射线。")]
        private FanRaycaster solid3DFanRaycaster;

        [SerializeField]
        [Tooltip("演示沿物体自身 +Z 方向前进的方块射线。")]
        private BoxRaycaster boxRaycaster;

        [Header("自动演示")]
        [SerializeField]
        [Tooltip("进入 Play Mode 后是否定时调用所有组件的统一 Cast()。")]
        private bool castAutomatically = true;

        [SerializeField, Min(0.1f)]
        [Tooltip("自动执行统一检测的时间间隔，单位为秒。")]
        private float automaticCastInterval = 0.5f;

        private readonly Dictionary<RaycasterBase, RaycastResult> latestResults =
            new Dictionary<RaycasterBase, RaycastResult>();
        private readonly List<DirectionalRaycastHit> straightHits = new List<DirectionalRaycastHit>();
        private readonly List<Vector3> fanDirections = new List<Vector3>();
        private readonly List<RaycastSample> fanSamples = new List<RaycastSample>();
        private readonly List<RaycastSampleHit> fanHits = new List<RaycastSampleHit>();
        private readonly RaycastHit[] boxHits = new RaycastHit[16];

        private float nextAutomaticCastTime;
        private string lastOperation = "等待执行检测。";

        private void OnEnable()
        {
            Subscribe(straightRaycaster);
            Subscribe(planeFanRaycaster);
            Subscribe(volumeFanRaycaster);
            Subscribe(solid3DFanRaycaster);
            Subscribe(boxRaycaster);
            nextAutomaticCastTime = Time.unscaledTime;
        }

        private void OnDisable()
        {
            Unsubscribe(straightRaycaster);
            Unsubscribe(planeFanRaycaster);
            Unsubscribe(volumeFanRaycaster);
            Unsubscribe(solid3DFanRaycaster);
            Unsubscribe(boxRaycaster);
        }

        private void Update()
        {
            if (!castAutomatically || Time.unscaledTime < nextAutomaticCastTime)
            {
                return;
            }

            CastAllRaycasters();
            nextAutomaticCastTime = Time.unscaledTime + automaticCastInterval;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 460f, 360f), GUI.skin.box);
            GUILayout.Label("Raycast Tools 全功能示例");
            GUILayout.Label("Scene 视图中的绿色线是编辑器预览，黄色线是最近一次运行时命中路径。");

            if (GUILayout.Button("执行全部统一 Cast()"))
            {
                CastAllRaycasters();
            }

            if (GUILayout.Button("切换全部 ClosestOnly / AllHits"))
            {
                ToggleDetectionMode();
            }

            if (GUILayout.Button("执行直线射线便捷接口"))
            {
                RunStraightConvenienceExamples();
            }

            if (GUILayout.Button("读取三种扇形采样并执行 CastAll"))
            {
                RunFanSamplingExamples();
            }

            if (GUILayout.Button("执行方块 Cast(out) / CastNonAlloc"))
            {
                RunBoxConvenienceExamples();
            }

            castAutomatically = GUILayout.Toggle(castAutomatically, "自动定时执行统一 Cast()", GUILayout.Height(22f));
            GUILayout.Space(6f);
            DrawResultLine("Straight", straightRaycaster);
            DrawResultLine("Fan Plane", planeFanRaycaster);
            DrawResultLine("Fan Volume", volumeFanRaycaster);
            DrawResultLine("Fan Solid3D", solid3DFanRaycaster);
            DrawResultLine("Box", boxRaycaster);
            GUILayout.Space(6f);
            GUILayout.Label(string.Concat("最近操作：", lastOperation));
            GUILayout.EndArea();
        }

        /// <summary>
        /// 调用所有示例组件的统一无参数 Cast()，每个组件都会同步触发 CastCompleted 事件。
        /// </summary>
        /// <remarks>这是业务代码最推荐的统一入口；返回值与事件参数是同一个 RaycastResult 对象。</remarks>
        public void CastAllRaycasters()
        {
            CastIfAvailable(straightRaycaster);
            CastIfAvailable(planeFanRaycaster);
            CastIfAvailable(volumeFanRaycaster);
            CastIfAvailable(solid3DFanRaycaster);
            CastIfAvailable(boxRaycaster);
            lastOperation = "已执行全部统一 Cast()，结果已通过事件接收。";
        }

        /// <summary>
        /// 在只检查最近无遮挡物体和穿透检查全部物体之间切换所有示例组件。
        /// </summary>
        public void ToggleDetectionMode()
        {
            RaycastDetectionMode currentMode = straightRaycaster != null
                ? straightRaycaster.DetectionMode
                : RaycastDetectionMode.ClosestOnly;
            RaycastDetectionMode nextMode = currentMode == RaycastDetectionMode.ClosestOnly
                ? RaycastDetectionMode.AllHits
                : RaycastDetectionMode.ClosestOnly;

            SetAllDetectionModes(nextMode);
        }

        /// <summary>
        /// 为场景中的所有示例射线统一设置命中收集方式。
        /// </summary>
        /// <param name="mode">ClosestOnly 只检查最近无遮挡物体；AllHits 会穿透并检查路径上的所有物体。</param>
        public void SetAllDetectionModes(RaycastDetectionMode mode)
        {
            SetDetectionMode(straightRaycaster, mode);
            SetDetectionMode(planeFanRaycaster, mode);
            SetDetectionMode(volumeFanRaycaster, mode);
            SetDetectionMode(solid3DFanRaycaster, mode);
            SetDetectionMode(boxRaycaster, mode);
            lastOperation = string.Concat("全部组件已切换为 ", mode, "。再次执行检测即可查看结果差异。");
        }

        /// <summary>
        /// 演示直线射线的单方向 Cast、四个方向便捷方法、方向换算和带方向批量结果。
        /// </summary>
        /// <remarks>这些兼容接口不会触发 CastCompleted；新业务代码需要统一返回时优先使用无参数 Cast()。</remarks>
        public void RunStraightConvenienceExamples()
        {
            if (straightRaycaster == null)
            {
                lastOperation = "StraightRaycaster 未绑定。";
                return;
            }

            Vector3 worldForward = straightRaycaster.GetWorldDirection(RayDirections.Forward);
            bool genericForwardHit = straightRaycaster.Cast(RayDirections.Forward, out _);
            bool forwardHit = straightRaycaster.CastForward(out _);
            bool backHit = straightRaycaster.CastBack(out _);
            bool leftHit = straightRaycaster.CastLeft(out _);
            bool rightHit = straightRaycaster.CastRight(out _);
            int hitCount = straightRaycaster.CastAll(straightHits);

            lastOperation = string.Format(
                "直线接口：Forward={0}，便捷命中 F/B/L/R={1}/{2}/{3}/{4}，CastAll={5}，世界前方={6}",
                genericForwardHit,
                forwardHit,
                backHit,
                leftHit,
                rightHit,
                hitCount,
                worldForward);
        }

        /// <summary>
        /// 演示 Plane、Volume、Solid3D 三种扇形的世界方向、完整采样以及原始 CastAll 结果。
        /// </summary>
        /// <remarks>Solid3D 的各层方向相同但起点不同，因此需要区分层级时应读取 RaycastSample.Origin。</remarks>
        public void RunFanSamplingExamples()
        {
            int planeCount = ReadFanSamples(planeFanRaycaster);
            int volumeCount = ReadFanSamples(volumeFanRaycaster);
            int solid3DCount = ReadFanSamples(solid3DFanRaycaster);
            lastOperation = string.Format(
                "扇形原始命中：Plane={0}，Volume={1}，Solid3D={2}。采样起点和方向已通过 GetWorldSamples 读取。",
                planeCount,
                volumeCount,
                solid3DCount);
        }

        /// <summary>
        /// 演示方块射线的最近命中 Cast(out RaycastHit) 和调用方数组 CastNonAlloc。
        /// </summary>
        /// <remarks>这两个便捷接口不触发 CastCompleted；需要统一结果和检测模式时请使用无参数 Cast()。</remarks>
        public void RunBoxConvenienceExamples()
        {
            if (boxRaycaster == null)
            {
                lastOperation = "BoxRaycaster 未绑定。";
                return;
            }

            bool closestHit = boxRaycaster.Cast(out RaycastHit hit);
            int hitCount = boxRaycaster.CastNonAlloc(boxHits);
            string closestName = closestHit && hit.collider != null ? hit.collider.name : "无";
            lastOperation = string.Format(
                "方块接口：最近命中={0}，CastNonAlloc 写入数量={1}。",
                closestName,
                hitCount);
        }

        private void Subscribe(RaycasterBase raycaster)
        {
            if (raycaster != null)
            {
                raycaster.CastCompleted += HandleCastCompleted;
            }
        }

        private void Unsubscribe(RaycasterBase raycaster)
        {
            if (raycaster != null)
            {
                raycaster.CastCompleted -= HandleCastCompleted;
            }
        }

        private void HandleCastCompleted(RaycastResult result)
        {
            latestResults[result.Source] = result;

            foreach (RaycastSampleHit sampleHit in result.Hits)
            {
                Debug.DrawLine(sampleHit.Origin, sampleHit.Hit.point, Color.yellow, automaticCastInterval);
            }
        }

        private static void CastIfAvailable(RaycasterBase raycaster)
        {
            if (raycaster != null)
            {
                raycaster.Cast();
            }
        }

        private static void SetDetectionMode(RaycasterBase raycaster, RaycastDetectionMode mode)
        {
            if (raycaster != null)
            {
                raycaster.DetectionMode = mode;
            }
        }

        private int ReadFanSamples(FanRaycaster raycaster)
        {
            if (raycaster == null)
            {
                return 0;
            }

            raycaster.GetWorldDirections(fanDirections);
            raycaster.GetWorldSamples(fanSamples);
            return raycaster.CastAll(fanHits);
        }

        private void DrawResultLine(string label, RaycasterBase raycaster)
        {
            if (raycaster == null)
            {
                GUILayout.Label(string.Concat(label, "：未绑定"));
                return;
            }

            if (!latestResults.TryGetValue(raycaster, out RaycastResult result))
            {
                GUILayout.Label(string.Concat(label, "：", raycaster.DetectionMode, "，尚未执行"));
                return;
            }

            GUILayout.Label(string.Format(
                "{0}：{1}，去重命中 {2}",
                label,
                result.DetectionMode,
                result.HitCount));
        }

        private void OnValidate()
        {
            automaticCastInterval = Mathf.Max(0.1f, automaticCastInterval);
        }
    }
}

using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 防災施策ユニットの生成 Factory（Season 3 W2）
    ///
    /// 専用 Prefab を持たず、プリミティブ（円柱）+ 効果範囲リングをコードで構築する。
    /// 見た目は機能検証用の仮実装（UI デザイン再調整フェーズで差し替え予定）。
    /// 効果パラメータの定数はバランス調整対象（W3 デモ準備時に調整）。
    /// </summary>
    internal static class InfrastructureFactory
    {
        private const string _PARENT_CONTAINER_NAME = "Infrastructures";
        private const int _RING_SEGMENT_COUNT = 48;
        private const float _RING_LINE_WIDTH = 0.5f;
        // 効果範囲リングの表示高さ（ローカル値・親スケール1.2倍で実高約30m）
        // 地面の起伏や建物に埋まらないよう、住宅の屋根高さより上に固定表示する（2026-06-13 フィードバック）
        private const float _RING_HEIGHT_OFFSET = 25f;
        private const string _RING_SHADER_NAME = "HDRP/Unlit";

        // 施策パラメータ（コスト, 効果半径[m], 鎮火力/秒）
        // 半径は 2026-06-13 テストプレイのフィードバックで拡大（建物 1 棟分では狭すぎる）。
        // 「何個置けばマップをカバーできるか」起点の正式バランスは W3 デモ準備時に調整
        // コストはエンティティクラス（Hydrant/Cistern/Plaza）の ItemStruct からも参照される
        internal const int HYDRANT_COST = 50;
        private const float _HYDRANT_RADIUS = 50f;
        private const float _HYDRANT_POWER = 0.25f;

        internal const int CISTERN_COST = 30;
        private const float _CISTERN_RADIUS = 70f;
        private const float _CISTERN_POWER = 0.06f;

        internal const int PLAZA_COST = 100;
        private const float _PLAZA_RADIUS = 50f;
        private const float _PLAZA_POWER = 0f;  // W2 では記録のみ（人的被害補正は W3 の結果計算で使用）

        private static readonly Color _HYDRANT_COLOR = new Color(0.9f, 0.2f, 0.15f);
        private static readonly Color _CISTERN_COLOR = new Color(0.15f, 0.4f, 0.9f);
        private static readonly Color _PLAZA_COLOR = new Color(0.2f, 0.8f, 0.3f);

        /// <summary>生成連番（ユニット名の一意化用）</summary>
        private static int _spawnCounter = 0;

        private const float _RAY_ORIGIN_HEIGHT = 500f;
        private const float _RAY_MAX_DISTANCE = 1000f;

        /// <summary>
        /// 施策ユニットを生成して配置する
        /// </summary>
        /// <param name="keepXZ">true: XZ を維持して DEM 高さに接地（プレイヤーのマーカー配置用）。
        /// false: 周囲の最低点を探して接地（建物屋根角など不正確な座標用）</param>
        internal static bool SpawnInfrastructure(GameEnum.ModelsType infraType, Vector3 spawnPoint, bool keepXZ = false)
        {
            if (!TryGetSpec(infraType, out int cost, out float radius, out float power, out Color color))
            {
                Debug.LogWarning($"[InfrastructureFactory] 未対応の施策タイプ: {infraType}");
                return false;
            }

            GameObject unitObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _spawnCounter = _spawnCounter + 1;
            unitObject.name = infraType.ToString() + _spawnCounter.ToString();
            if (keepXZ)
            {
                unitObject.transform.position = SnapToDemHeight(spawnPoint);
            }
            else
            {
                unitObject.transform.position = SnapToGround(spawnPoint);
            }
            unitObject.transform.localScale = new Vector3(1.5f, 1.2f, 1.5f);
            unitObject.transform.SetParent(GetParentContainer().transform, true);

            Renderer bodyRenderer = unitObject.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = color;
            }

            InfrastructureUnit unit = unitObject.AddComponent<InfrastructureUnit>();
            unit.InfraType = infraType;
            unit.EffectRadius = radius;
            unit.ExtinguishPowerPerTick = power;
            unit.Cost = cost;

            BuildRangeRing(unitObject, radius, color);
            InvestmentLedger.RecordInvestment(infraType, cost);

            Debug.Log($"[InfrastructureFactory] {infraType} を配置: pos={spawnPoint}, 半径={radius}, コスト={cost}");
            return true;
        }

        /// <summary>
        /// 施策タイプごとのパラメータを取得
        /// </summary>
        private static bool TryGetSpec(GameEnum.ModelsType infraType, out int cost, out float radius, out float power, out Color color)
        {
            cost = 0;
            radius = 0f;
            power = 0f;
            color = Color.white;

            if (infraType == GameEnum.ModelsType.Hydrant)
            {
                cost = HYDRANT_COST;
                radius = _HYDRANT_RADIUS;
                power = _HYDRANT_POWER;
                color = _HYDRANT_COLOR;
                return true;
            }
            if (infraType == GameEnum.ModelsType.Cistern)
            {
                cost = CISTERN_COST;
                radius = _CISTERN_RADIUS;
                power = _CISTERN_POWER;
                color = _CISTERN_COLOR;
                return true;
            }
            if (infraType == GameEnum.ModelsType.Plaza)
            {
                cost = PLAZA_COST;
                radius = _PLAZA_RADIUS;
                power = _PLAZA_POWER;
                color = _PLAZA_COLOR;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 効果範囲リング（LineRenderer の円）を構築
        /// </summary>
        private static void BuildRangeRing(GameObject unitObject, float radius, Color color)
        {
            GameObject ringObject = new GameObject("EffectRangeRing");
            ringObject.transform.SetParent(unitObject.transform, false);

            LineRenderer ring = ringObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = _RING_SEGMENT_COUNT;
            ring.startWidth = _RING_LINE_WIDTH;
            ring.endWidth = _RING_LINE_WIDTH;

            Shader unlitShader = Shader.Find(_RING_SHADER_NAME);
            if (unlitShader != null)
            {
                Material ringMaterial = new Material(unlitShader);
                ringMaterial.color = color;
                ring.material = ringMaterial;
            }

            // 親（円柱）のスケールの影響を打ち消しつつ円を描く
            Vector3 parentScale = unitObject.transform.localScale;
            for (int i = 0; i < _RING_SEGMENT_COUNT; i++)
            {
                float angle = 2f * Mathf.PI * i / _RING_SEGMENT_COUNT;
                float x = Mathf.Cos(angle) * radius / parentScale.x;
                float z = Mathf.Sin(angle) * radius / parentScale.z;
                ring.SetPosition(i, new Vector3(x, _RING_HEIGHT_OFFSET, z));
            }
        }

        /// <summary>
        /// 配置点を地面（DEM）に接地させる
        /// random_doom_building 由来の配置点は建物屋根の角になるため（2026-06-13 テストで判明）、
        /// 周囲 8 方向 + 中心の Raycast で最も低いヒット点（= 屋根ではなく地面の可能性が高い）を選ぶ
        /// </summary>
        private static Vector3 SnapToGround(Vector3 point)
        {
            const float _PROBE_OFFSET_DISTANCE = 6f;
            const int _PROBE_DIRECTION_COUNT = 8;

            Vector3 bestPoint = point;
            bool hasHit = TryRaycastDown(point.x, point.z, out Vector3 centerHit);
            if (hasHit)
            {
                bestPoint = centerHit;
            }

            for (int i = 0; i < _PROBE_DIRECTION_COUNT; i++)
            {
                float angle = 2f * Mathf.PI * i / _PROBE_DIRECTION_COUNT;
                float probeX = point.x + Mathf.Cos(angle) * _PROBE_OFFSET_DISTANCE;
                float probeZ = point.z + Mathf.Sin(angle) * _PROBE_OFFSET_DISTANCE;

                if (!TryRaycastDown(probeX, probeZ, out Vector3 probeHit))
                {
                    continue;
                }
                if (!hasHit || probeHit.y < bestPoint.y)
                {
                    bestPoint = probeHit;
                    hasHit = true;
                }
            }

            return bestPoint;
        }

        /// <summary>
        /// 指定 XZ から下方向に Raycast し、ヒット点を返す（ヒット無しなら false）
        /// </summary>
        private static bool TryRaycastDown(float x, float z, out Vector3 hitPoint)
        {
            Vector3 rayOrigin = new Vector3(x, _RAY_ORIGIN_HEIGHT, z);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _RAY_MAX_DISTANCE))
            {
                hitPoint = hit.point;
                return true;
            }

            hitPoint = Vector3.zero;
            return false;
        }

        /// <summary>
        /// XZ を維持したまま DEM（Ground レイヤー）の高さに接地する
        /// プレイヤーのマーカー配置で位置がずれないようにするための接地方式
        /// （2026-06-13 Task 4 フィードバック対応）
        /// </summary>
        private static Vector3 SnapToDemHeight(Vector3 point)
        {
            int groundLayerMask = 1 << LayerMask.NameToLayer(nameof(GameEnum.LayerType.Ground));
            Vector3 rayOrigin = new Vector3(point.x, _RAY_ORIGIN_HEIGHT, point.z);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _RAY_MAX_DISTANCE, groundLayerMask))
            {
                return new Vector3(point.x, hit.point.y, point.z);
            }

            // DEM レイヤーにヒットしない場合は周辺最低点方式にフォールバック
            Debug.LogWarning($"[InfrastructureFactory] Ground レイヤーが見つからないため周辺接地にフォールバック: {point}");
            return SnapToGround(point);
        }

        /// <summary>
        /// 施策ユニットの親コンテナを取得（無ければ作成）
        /// </summary>
        private static GameObject GetParentContainer()
        {
            GameObject container = GameObject.Find(_PARENT_CONTAINER_NAME);
            if (container == null)
            {
                container = new GameObject(_PARENT_CONTAINER_NAME);
            }
            return container;
        }
    }
}

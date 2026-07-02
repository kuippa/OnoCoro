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
        // 施策のコスト・半径・鎮火力は InfrastructureConfig（staging/infrastructures.yaml）に外部化（W3 Task4）。
        // 色だけは演出用に固定で持つ
        private static readonly Color _HYDRANT_COLOR = new Color(0.9f, 0.2f, 0.15f);
        private static readonly Color _CISTERN_COLOR = new Color(0.15f, 0.4f, 0.9f);
        private static readonly Color _PLAZA_COLOR = new Color(0.2f, 0.8f, 0.3f);

        /// <summary>生成連番（ユニット名の一意化用）</summary>
        private static int _spawnCounter = 0;

        private const float _RAY_ORIGIN_HEIGHT = 500f;
        private const float _RAY_MAX_DISTANCE = 1000f;

        /// <summary>
        /// 施策ユニットを生成して配置する
        /// 配置点は DEM（Ground レイヤー）の高さに接地される
        /// </summary>
        internal static bool SpawnInfrastructure(GameEnum.ModelsType infraType, Vector3 spawnPoint)
        {
            if (!TryGetSpec(infraType, out int cost, out float radius, out float power, out Color color))
            {
                Debug.LogWarning($"[InfrastructureFactory] 未対応の施策タイプ: {infraType}");
                return false;
            }

            GameObject unitObject = InstantiateInfrastructureBody(infraType);
            if (unitObject == null)
            {
                Debug.LogWarning($"[InfrastructureFactory] {infraType} のプレファブが取得できません");
                return false;
            }
            _spawnCounter = _spawnCounter + 1;
            unitObject.name = infraType.ToString() + _spawnCounter.ToString();
            // 右クリック撤去の識別用タグ（TagManager 登録済みの Hydrant/Cistern のみ。
            // Plaza はタグ未登録のため設定しない＝未定義タグ設定の例外を回避）
            if (infraType == GameEnum.ModelsType.Hydrant || infraType == GameEnum.ModelsType.Cistern)
            {
                unitObject.tag = infraType.ToString();
            }
            unitObject.transform.position = SnapToDemHeight(spawnPoint);
            unitObject.transform.SetParent(GetParentContainer().transform, true);

            // 効果パラメータは config 由来なので、プレファブに何が入っていても実行時に上書きする
            InfrastructureUnit unit = unitObject.GetComponent<InfrastructureUnit>();
            if (unit == null)
            {
                unit = unitObject.AddComponent<InfrastructureUnit>();
            }
            unit.InfraType = infraType;
            unit.EffectRadius = radius;
            unit.ExtinguishPowerPerTick = power;
            unit.Cost = cost;

            // 効果範囲リングは半径が config 依存のため、既存があれば作り直す
            RebuildRangeRing(unitObject, radius, color);
            InvestmentLedger.RecordInvestment(infraType, cost);

            Debug.Log($"[InfrastructureFactory] {infraType} を配置: pos={spawnPoint}, 半径={radius}, コスト={cost}");
            return true;
        }

        /// <summary>
        /// 施策タイプに対応するプレファブを PrefabManager から取得して生成する
        /// </summary>
        private static GameObject InstantiateInfrastructureBody(GameEnum.ModelsType infraType)
        {
            GameObject prefab = null;
            if (infraType == GameEnum.ModelsType.Hydrant)
            {
                prefab = PrefabManager.HydrantPrefab;
            }
            else if (infraType == GameEnum.ModelsType.Cistern)
            {
                prefab = PrefabManager.CisternPrefab;
            }
            else if (infraType == GameEnum.ModelsType.Plaza)
            {
                prefab = PrefabManager.HydrantPrefab;  // Plaza は暫定（メニュー外）。専用プレファブ未作成
            }

            if (prefab == null)
            {
                return null;
            }
            return Object.Instantiate(prefab);
        }

        /// <summary>
        /// 施策タイプごとのパラメータを取得
        /// コスト・半径・鎮火力は InfrastructureConfig（YAML 外部化）から、色は固定（演出）
        /// </summary>
        private static bool TryGetSpec(GameEnum.ModelsType infraType, out int cost, out float radius, out float power, out Color color)
        {
            cost = 0;
            radius = 0f;
            power = 0f;
            color = Color.white;

            if (!InfrastructureConfig.TryGet(infraType, out InfrastructureSpec spec))
            {
                return false;
            }
            cost = spec.Cost;
            radius = spec.Radius;
            power = spec.Power;

            if (infraType == GameEnum.ModelsType.Hydrant)
            {
                color = _HYDRANT_COLOR;
                return true;
            }
            if (infraType == GameEnum.ModelsType.Cistern)
            {
                color = _CISTERN_COLOR;
                return true;
            }
            if (infraType == GameEnum.ModelsType.Plaza)
            {
                color = _PLAZA_COLOR;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 効果範囲リングを（プレファブに既存があれば破棄して）現在の半径で構築し直す
        /// </summary>
        private static void RebuildRangeRing(GameObject unitObject, float radius, Color color)
        {
            Transform existingRing = unitObject.transform.Find("EffectRangeRing");
            if (existingRing != null)
            {
                Object.Destroy(existingRing.gameObject);
            }
            BuildRangeRing(unitObject, radius, color);
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
        /// XZ を維持したまま DEM（Ground レイヤー）の高さに接地する
        ///
        /// [NOTE] 全コライダー対象の「周辺最低点」方式は、マップ下の奈落トラップの
        /// コライダー（Y 約 -30）を拾って画面外に配置される事故があった（2026-06-13 テストで判明）。
        /// 必ず Ground レイヤー限定で Raycast すること。
        /// 中心で当たらない場合は周囲 8 方向を探査し、それでも当たらなければ
        /// 既存の DEM 安全位置アルゴリズム（DemController）にフォールバックする
        /// </summary>
        private static Vector3 SnapToDemHeight(Vector3 point)
        {
            const float _PROBE_OFFSET_DISTANCE = 6f;
            const int _PROBE_DIRECTION_COUNT = 8;

            int groundLayerMask = 1 << LayerMask.NameToLayer(nameof(GameEnum.LayerType.Ground));

            if (TryRaycastDown(point.x, point.z, groundLayerMask, out Vector3 centerHit))
            {
                return new Vector3(point.x, centerHit.y, point.z);
            }

            for (int i = 0; i < _PROBE_DIRECTION_COUNT; i++)
            {
                float angle = 2f * Mathf.PI * i / _PROBE_DIRECTION_COUNT;
                float probeX = point.x + Mathf.Cos(angle) * _PROBE_OFFSET_DISTANCE;
                float probeZ = point.z + Mathf.Sin(angle) * _PROBE_OFFSET_DISTANCE;

                if (TryRaycastDown(probeX, probeZ, groundLayerMask, out Vector3 probeHit))
                {
                    return probeHit;
                }
            }

            Debug.LogWarning($"[InfrastructureFactory] DEM（Ground レイヤー）に接地できないため安全位置にフォールバック: {point}");
            return DemController.GetDemRndAbovePosition(0.5f);
        }

        /// <summary>
        /// 指定 XZ から下方向に Raycast し、ヒット点を返す（ヒット無しなら false）
        /// </summary>
        private static bool TryRaycastDown(float x, float z, int layerMask, out Vector3 hitPoint)
        {
            Vector3 rayOrigin = new Vector3(x, _RAY_ORIGIN_HEIGHT, z);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _RAY_MAX_DISTANCE, layerMask))
            {
                hitPoint = hit.point;
                return true;
            }

            hitPoint = Vector3.zero;
            return false;
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

using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 防災施策ユニット（Season 3 W2: 統計効果型インフラ）
    ///
    /// 効果範囲内の FireCube を周期的に減衰させる（リアルタイム撃退ではなく
    /// 「どこに投資したか」が被害を決める統計効果型）。
    /// 効果パラメータは InfrastructureFactory が設定する。
    /// バランス調整は W3 のデモ準備時に行う（値はすべて Factory の定数）。
    /// </summary>
    public class InfrastructureUnit : MonoBehaviour, IItemStructProvider, IUnitStructProvider
    {
        /// <summary>
        /// IItemStructProvider 実装。InfoWindowCtrl.GetParentObject が本コンポーネントで
        /// 遡上を止めるために必要（未実装だと親コンテナ Infrastructures まで遡ってしまい
        /// 右クリック撤去が機能しなかった・2026-07-03）
        /// </summary>
        public ItemStruct ItemStruct => BuildItemStruct();

        /// <summary>IUnitStructProvider 実装（情報ウィンドウ表示用）</summary>
        public UnitStruct UnitStruct => GetUnitStruct();

        /// <summary>効果チェックの周期（秒）</summary>
        private const float _EFFECT_TICK_INTERVAL = 1.0f;

        /// <summary>このスケール以下になった FireCube は鎮火（消滅）とみなす</summary>
        private const float _EXTINGUISH_SCALE_THRESHOLD = 0.3f;

        internal GameEnum.ModelsType InfraType { get; set; }
        internal float EffectRadius { get; set; } = 0f;

        /// <summary>1 tick あたりの鎮火力（FireCube のスケールを (1 - 値) 倍にする。0 なら効果なし）</summary>
        internal float ExtinguishPowerPerTick { get; set; } = 0f;

        internal int Cost { get; set; } = 0;

        private float _tickTimer = 0f;

        /// <summary>
        /// 情報ウィンドウ表示用の UnitStruct を返す（右クリック撤去のため）。
        /// 表示名は和名（消火栓/防火水槽/避難広場）。DeleteCost は撤去時の BIT 返金額
        /// </summary>
        internal UnitStruct GetUnitStruct()
        {
            return new UnitStruct(
                GetDisplayName(InfraType),  // 和名
                this.gameObject.name,       // UnitID
                1,                          // Lv
                "防災装置。右クリックで撤去できます",  // Info
                0,                          // UpdateCost
                Cost,                       // DeleteCost（撤去で BIT を返金）
                0,                          // BaseScore
                GlobalConst.SHORT_SCORE1_SCALE  // ScoreType（BIT）
            );
        }

        /// <summary>
        /// プレイヤー操作による撤去。BIT を返金し投資台帳を巻き戻してから破棄する
        /// </summary>
        internal void RemoveByUser()
        {
            ScoreCtrl.UpdateAndDisplayScore(Cost, GlobalConst.SHORT_SCORE1_SCALE);  // BIT 返金
            InvestmentLedger.RefundInvestment(InfraType, Cost);                     // 台帳巻き戻し
            Debug.Log($"[InfrastructureUnit] {InfraType} を撤去（BIT {Cost} 返金）");
            GameObjectTreat.DestroyAll(this.gameObject);
        }

        /// <summary>
        /// IItemStructProvider 用の ItemStruct（配置済みユニットの参照用・最低限の内容）
        /// </summary>
        private ItemStruct BuildItemStruct()
        {
            return new ItemStruct(
                GetDisplayName(InfraType),
                this.gameObject.name,
                GetDisplayName(InfraType),
                "防災装置。右クリックで撤去できます",
                Cost,
                GlobalConst.SHORT_SCORE1_SCALE,
                0f,
                1,
                "",
                "",
                0
            );
        }

        private static string GetDisplayName(GameEnum.ModelsType type)
        {
            if (type == GameEnum.ModelsType.Hydrant)
            {
                return "消火栓";
            }
            if (type == GameEnum.ModelsType.Cistern)
            {
                return "防火水槽";
            }
            if (type == GameEnum.ModelsType.Plaza)
            {
                return "避難広場";
            }
            return type.ToString();
        }

        private void Update()
        {
            if (ExtinguishPowerPerTick <= 0f)
            {
                return;
            }

            _tickTimer += Time.deltaTime;
            if (_tickTimer < _EFFECT_TICK_INTERVAL)
            {
                return;
            }
            _tickTimer = 0f;
            ApplyExtinguishEffect();
        }

        /// <summary>
        /// 効果範囲内の FireCube を減衰させ、しきい値以下なら鎮火（消滅）させる
        /// </summary>
        private void ApplyExtinguishEffect()
        {
            GameObject[] fireCubes = GameObject.FindGameObjectsWithTag(nameof(GameEnum.TagType.FireCube));
            foreach (GameObject fireCube in fireCubes)
            {
                float distance = Vector3.Distance(this.transform.position, fireCube.transform.position);
                if (distance > EffectRadius)
                {
                    continue;
                }

                fireCube.transform.localScale = fireCube.transform.localScale * (1f - ExtinguishPowerPerTick);
                if (fireCube.transform.localScale.x < _EXTINGUISH_SCALE_THRESHOLD)
                {
                    // [NOTE] 鎮火は FireCube ごとに発生し高頻度のためログは出さない（ログ洪水対策 2026-07-03）
                    Destroy(fireCube);
                }
            }
        }
    }
}

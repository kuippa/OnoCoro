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
    public class InfrastructureUnit : MonoBehaviour
    {
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
                    Debug.Log($"[InfrastructureUnit] {InfraType} が {fireCube.name} を鎮火しました");
                    Destroy(fireCube);
                }
            }
        }
    }
}

using System.Collections.Generic;

namespace CommonsUtility
{
    /// <summary>
    /// 防災施策 1 種のバランス値（コスト・効果半径・鎮火力）
    /// </summary>
    internal struct InfrastructureSpec
    {
        public int Cost;
        public float Radius;
        public float Power;

        public InfrastructureSpec(int cost, float radius, float power)
        {
            Cost = cost;
            Radius = radius;
            Power = power;
        }
    }

    /// <summary>
    /// 防災施策のバランス値ホルダー（W3 Task4・外部化）
    ///
    /// 既定値を持ち、staging/infrastructures.yaml があれば InfrastructureYamlProvider が上書きする。
    /// 再コンパイルなしで施策のコスト・半径・鎮火力を調整できるようにするための仕組み。
    /// InfrastructureFactory（生成・効果）と Hydrant/Cistern/Plaza（メニュー表示コスト）が参照する。
    /// </summary>
    internal static class InfrastructureConfig
    {
        private static Dictionary<GameEnum.ModelsType, InfrastructureSpec> _specs = BuildDefaults();

        /// <summary>
        /// 既定値（YAML が無い/未指定の施策はこれが使われる）
        /// </summary>
        private static Dictionary<GameEnum.ModelsType, InfrastructureSpec> BuildDefaults()
        {
            return new Dictionary<GameEnum.ModelsType, InfrastructureSpec>
            {
                { GameEnum.ModelsType.Hydrant, new InfrastructureSpec(50, 30f, 0.25f) },
                { GameEnum.ModelsType.Cistern, new InfrastructureSpec(120, 70f, 0.06f) },
                { GameEnum.ModelsType.Plaza, new InfrastructureSpec(100, 50f, 0f) },
            };
        }

        /// <summary>
        /// 指定施策のバランス値を上書き登録（YAML プロバイダから呼ぶ）
        /// </summary>
        internal static void Set(GameEnum.ModelsType type, InfrastructureSpec spec)
        {
            _specs[type] = spec;
        }

        /// <summary>
        /// 指定施策のバランス値を取得（未定義なら false）
        /// </summary>
        internal static bool TryGet(GameEnum.ModelsType type, out InfrastructureSpec spec)
        {
            return _specs.TryGetValue(type, out spec);
        }

        /// <summary>
        /// 指定施策のコストを取得（未定義なら 0）。エンティティの ItemStruct 用
        /// </summary>
        internal static int GetCost(GameEnum.ModelsType type)
        {
            if (_specs.TryGetValue(type, out InfrastructureSpec spec))
            {
                return spec.Cost;
            }
            return 0;
        }

        /// <summary>
        /// 既定値に戻す（ステージロード時のリセット用）
        /// </summary>
        internal static void ResetToDefaults()
        {
            _specs = BuildDefaults();
        }
    }
}

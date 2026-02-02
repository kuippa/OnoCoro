namespace CommonsUtility
{
    /// <summary>
    /// UI フォントサイズ規定（HTML H1～H7）
    /// 
    /// TextMeshPro のフォントサイズを管理するための enum です。
    /// HTML の見出しレベル（H1～H7）に統一し、直感的に理解できるように設計しました。
    /// H1 が最大（最重要）、H7 が最小（最補足的）です。
    /// 
    /// 解像度が変わった場合でも、Reference Resolution に基づいて
    /// 自動的にスケーリングされます。
    /// 
    /// 使用例:
    /// textMeshPro.SetFontSize(UIFont.H2);
    /// textMeshPro.ApplyUIStyle(UIFont.H1, TextAlignmentOptions.Center);
    /// </summary>
    public enum UIFont
    {
        /// <summary>H1 (64px) - 最大・メインタイトル・ゲームロゴ用</summary>
        H1 = 64,

        /// <summary>H2 (48px) - 超大・ステージタイトル・大きなボタン用</summary>
        H2 = 48,

        /// <summary>H3 (36px) - 大・セクション見出し・ダイアログタイトル用</summary>
        H3 = 36,

        /// <summary>H4 (28px) - 中・サブヘッダー・メニュー見出し用</summary>
        H4 = 28,

        /// <summary>H5 (20px) - 標準・本文・ボタンテキスト用</summary>
        H5 = 20,

        /// <summary>H6 (16px) - 小・ラベル・補足情報用</summary>
        H6 = 16,

        /// <summary>H7 (12px) - 最小・注釈・ツールチップ用</summary>
        H7 = 12,
    }
}

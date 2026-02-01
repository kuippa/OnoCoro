# Access Modifiers Policy - OnoCoro

このドキュメントは、OnoCoro プロジェクトで使用する C# アクセス修飾子（Access Modifiers）の詳細ガイドです。

**関連ドキュメント**: [AGENTS.md](../../AGENTS.md) - Project Guidelines (概要版)

---

## 基本原則

**MANDATORY**: `internal` をすべてのマネージャー・システム・ユーティリティクラスのデフォルトとして使用します。`public` は公開インターフェースのみに使用します。

### なぜ Assembly 境界が重要なのか

OnoCoro では、C# `namespace` だけでは真の カプセル化に不十分であるため、**Assembly 境界のカプセル化を優先します**。`internal` 修飾子は以下を提供します：

- **Assembly 境界保護** - 意図しない外部アクセスを防止
- **Recovery フェーズセーフティ** - グローバル状態の依存性を明確化
- **将来の拡張性** - プラグイン/DLC アーキテクチャをサポート（API 破損なし）

---

## アクセス修飾子の使い分けガイド

| 修飾子 | 使用場面 | 例 | 理由 |
|--------|---------|-----|------|
| **public** | 公開 API、安定した契約 | インターフェース定義、メインエントリーポイント | 後方互換性を保証 |
| **internal** | プロジェクト内実装 | GameConfig, Manager, Utility クラス | Assembly 内のみに制限 |
| **protected** | 継承拡張ポイント | ベースコントローラークラス | 意図的なサブクラス化をサポート |
| **private** | クラス内のみ | ヘルパーメソッド、キャッシュ変数 | 実装詳細を隠蔽 |

---

## デフォルトパターン：internal

### Manager クラス

```csharp
// ✅ CORRECT: Manager クラスは internal
internal class GameConfig : MonoBehaviour
{
    internal static string _APP_GAME_MODE = GlobalConst.GAME_MODE_DEBUG;
    internal static DebugLevel DebugLevel { get; set; } = DebugLevel.All;
}
```

### Utility クラス

```csharp
// ✅ CORRECT: Utility クラスは internal
internal static class LogUtility
{
    public static void WriteLog(LogLevel level, string message) { }
}
```

### 公開インターフェースでの実装隠蔽

```csharp
// ✅ CORRECT: 公開インターフェースを公開、実装は internal
public interface IGameConfig { }  // 安定した公開 API
internal class GameConfig : IGameConfig { } // 内部実装
```

---

## public を使用する場合

`public` は以下の場合のみ使用します：

### 1. 公開インターフェース・契約

```csharp
// ✅ OK: 公開インターフェースは expected
public interface IPrefabManager
{
    GameObject GetPrefab(string prefabName);
}
```

### 2. メインエントリーポイント

```csharp
// ✅ OK: Scene コントローラーは public でもよい
public class GameMainController : MonoBehaviour { }
```

### 3. アセット参照（Unity Inspector が必要）

```csharp
// ✅ OK: Inspector アクセスが必要な場合
[SerializeField]
private PrefabManager prefabManager;
```

### NOT public

```csharp
// ❌ NG: 公開の理由なし
public static class LogUtility { }  // → internal を使用
```

---

## Recovery フェーズの文脈（OnoCoro 向け重要）

OnoCoro は 2 年前のバックアップからの復旧プロジェクトなため、アクセス修飾子の区別は以下を意味します：

- **`public`** = 「これは安定し文書化された API です。変更しません」
- **`internal`** = 「これは実装詳細です。リファクタリング時に変更される可能性があります」

この区別は以下を防ぎます：

- グローバル状態への意図しないアクセス
- 内部実装詳細への結合
- 復旧コード リファクタリング時の回帰
- 誤った API サーフェス拡張

---

## 実装例：GameConfig デザイン

### 基本パターン

```csharp
// ✅ CORRECT: アクセスを制限、必要に応じて interface で公開
internal sealed class GameConfig : MonoBehaviour
{
    // すべての状態を internal - 外部操作を防止
    internal static string _APP_GAME_MODE = GlobalConst.GAME_MODE_DEBUG;
    internal static DebugLevel DebugLevel { get; set; } = DebugLevel.All;
    internal static string LogFileName { get; set; } = GlobalConst._LOG_FILE_NAME;
}
```

### 読み取り専用アクセスが必要な場合

```csharp
// 外部コードが読み取り専用でアクセスする必要がある場合は interface を使用
public interface IGameConfigProvider
{
    string GetGameMode();
    DebugLevel GetDebugLevel();
}

// IGameConfigProvider の internal 実装
internal class GameConfigProvider : IGameConfigProvider
{
    public string GetGameMode() => GameConfig._APP_GAME_MODE;
    public DebugLevel GetDebugLevel() => GameConfig.DebugLevel;
}
```

---

## Pre-Commit チェックリスト（アクセス修飾子）

コード変更をレビューするときに確認します：

- [ ] **デフォルト internal**: Manager/System/Utility クラスが `internal` になっているか（正当な理由がない限り）
- [ ] **無駄な public**: 「将来のために」 `public` にしていないか
- [ ] **インターフェース駆動**: 外部アクセスが必要な場合、`public interface` で公開し、実装は `internal` で隠蔽しているか
- [ ] **Recovery ポリシー準拠**: グローバル状態が外部操作から保護されているか
- [ ] **Assembly 境界尊重**: `namespace` だけでのカプセル化に依存していないか

---

**Last Updated**: 2026-02-02
**Project**: OnoCoro (Unity 6.3 Geospatial Visualization)

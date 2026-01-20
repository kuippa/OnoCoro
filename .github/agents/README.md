# OnoCoro Agent Collection

このフォルダには GitHub Copilot Code で実行される **専門エージェント** が含まれています。各エージェントは特定のドメインに特化した判断と委任を行います。

## 🤖 エージェント一覧

### 優先度 A: 必須エージェント

| Agent | 説明 | 用途 | コマンド |
|-------|------|------|---------|
| **planner.md** | 機能実装計画エージェント | 機能要件を分解・計画化 | `/planner` |
| **code-reviewer.md** | シニアコードレビューエージェント | コード品質・規約チェック | `/code-reviewer` |
| **recovery-validator.md** | Recovery フェーズ検証エージェント | 防御的プログラミング検証 | `/recovery-validator` |
| **plateau-specialist.md** | PLATEAU SDK 専門エージェント | GIS/地理データ処理検証 | `/plateau-specialist` |

### エージェントの役割

#### 1. Planner（機能実装計画エージェント）

**目的**: 機能要件を実装可能な計画に変換

**使用シーン**:
- 新機能の実装計画が必要
- 要件が複雑でスコープ不明
- 複数フェーズに分割する必要がある

**出力**:
- 実装フェーズの分割
- 各フェーズの受け入れ基準
- 依存関係の特定
- 適切なレビュアーの割り当て

```
/planner
Feature: RainDrop puddle system の実装

→ 計画書自動生成
```

---

#### 2. Code Reviewer（コードレビューエージェント）

**目的**: AGENTS.md の規約を厳密にチェック

**使用シーン**:
- コード品質のレビューが必要
- AGENTS.md 規約遵守確認
- マージ前の最終チェック

**チェック項目**:
- ✅ 必須ブレース確認
- ✅ Null チェック実装
- ✅ マジックナンバー/文字列検出
- ✅ PrefabManager 使用確認
- ✅ メソッド長チェック（40行以内）

```
/code-reviewer
File: Assets/Scripts/RainDropsCtrl.cs

→ 違反リスト + 修正提案
```

---

#### 3. Recovery Validator（Recovery フェーズ検証エージェント）

**目的**: 防御的プログラミング = バックアップ復旧の特殊性に対応

**使用シーン**:
- Recovery フェーズコードの検証
- Null 安全性の確認
- データ欠損時の挙動検証

**検証項目**:
- ✅ Transform.Find() null チェック
- ✅ GetComponent() null チェック
- ✅ Early return パターン確認
- ✅ ネストされた if 検出
- ✅ エラーメッセージの有無

```
/recovery-validator
File: Assets/Scripts/RainDropsCtrl.cs
Focus: ChangeColliderSize() method

→ Recovery フェーズ適合性レポート
```

---

#### 4. PLATEAU Specialist（PLATEAU SDK 専門エージェント）

**目的**: GIS データ・座標変換・メッシュ生成の検証

**使用シーン**:
- PLATEAU SDK 統合コードの検証
- 座標変換の正確性確認
- メモリ効率最適化の検証

**検証項目**:
- ✅ CityGML パースの安全性
- ✅ 座標変換の正確性（WGS84 ↔ Unity）
- ✅ メッシュ生成の正確性
- ✅ メモリ効率（大規模データセット対応）
- ✅ エラーハンドリング

```
/plateau-specialist
File: Assets/Scripts/PlateauDataManager.cs
Method: LoadCityData()

→ PLATEAU SDK 実装適合性レポート
```

---

## 🚀 使用方法

### ステップ 1: エージェント委任コマンド実行

VS Code 内で Copilot Chat を開き、エージェント名を指定：

```
/planner
Feature: [機能説明]

または

/code-reviewer
File: [ファイルパス]

または

/recovery-validator
File: [ファイルパス]
Focus: [検証対象]

または

/plateau-specialist
File: [ファイルパス]
Method: [メソッド名]
```

### ステップ 2: エージェント分析実行

エージェントが以下を実行：
1. コード/要件を分析
2. 関連するドキュメント参照
3. OnoCoro 固有のガイドライン適用
4. 詳細なレポート生成

### ステップ 3: アクション実行

エージェントの推奨に基づいて：
- コード修正
- 追加テスト実施
- アーキテクチャ改善
- 再レビュー

---

## 🎯 推奨される委任フロー

### 新機能開発フロー

```
1️⃣ /planner
   機能: [要件]
   → 実装計画書生成

2️⃣ 実装作業
   コード実装

3️⃣ /code-reviewer
   File: [実装ファイル]
   → 規約チェック

4️⃣ /recovery-validator
   File: [実装ファイル]
   Focus: Null safety
   → Recovery フェーズ対応確認

5️⃣ /plateau-specialist （PLATEAU 関連の場合）
   File: [PLATEAU コード]
   → 地理データ処理検証

6️⃣ マージ
   すべての agent が OK
```

### 既存コード修正フロー

```
1️⃣ /recovery-validator
   File: [修正対象]
   → 防御的プログラミング確認

2️⃣ /code-reviewer
   File: [修正後]
   → 規約遵守確認

3️⃣ マージ
```

### PLATEAU SDK 統合フロー

```
1️⃣ /planner
   Feature: PLATEAU [機能]
   → 計画書生成

2️⃣ 実装

3️⃣ /plateau-specialist
   File: [実装ファイル]
   → 座標変換・メモリ効率確認

4️⃣ /code-reviewer
   File: [実装ファイル]
   → 規約確認

5️⃣ マージ
```

---

## 📋 エージェント選択ガイド

### どのエージェントを選ぶ？

| 状況 | エージェント | 理由 |
|------|------------|------|
| 複雑な機能の計画が必要 | `/planner` | スコープ分割が得意 |
| コードの品質を確認したい | `/code-reviewer` | 規約チェックに特化 |
| Recovery フェーズのコード | `/recovery-validator` | Null 安全性に特化 |
| PLATEAU SDK の実装 | `/plateau-specialist` | GIS・座標系の専門家 |
| 複数の懸念がある | すべて順番に実行 | 網羅的チェック |

---

## 🔗 関連ドキュメント

### エージェント基盤
- [AGENTS.md](../../AGENTS.md) - 全体ガイドライン
- [docs/coding-standards.md](../../docs/coding-standards.md) - C# 規約

### 専門知識
- [docs/recovery-workflow.md](../../docs/recovery-workflow.md) - Recovery フェーズ
- [.github/instructions/unity-csharp-recovery.instructions.md](../../.github/instructions/unity-csharp-recovery.instructions.md) - Recovery C# パターン
- [.github/instructions/plateau-sdk-geospatial.instructions.md](../../.github/instructions/plateau-sdk-geospatial.instructions.md) - PLATEAU SDK 詳細

### コマンド・スキル
- [.github/commands/](../../.github/commands/) - Slash commands
- [.github/skills/](../../.github/skills/) - Agent Skills

---

## 💡 ベストプラクティス

### エージェント活用のコツ

1. **段階的委任**
   - まず `/planner` で計画
   - 実装後に `/code-reviewer`
   - Recovery フェーズなら `/recovery-validator` も実行

2. **具体的な指示**
   ```
   ❌ /code-reviewer File: RainDropsCtrl.cs
   ✅ /code-reviewer
      File: RainDropsCtrl.cs
      Focus: ChangeColliderSize() method for null safety
   ```

3. **修正→再レビュー**
   - エージェントの指摘に基づき修正
   - 修正後は **必ず再レビュー**
   - OK が出るまで繰り返す

4. **複数エージェントの活用**
   - コード品質 → `/code-reviewer`
   - Recovery 安全性 → `/recovery-validator`
   - GIS データ → `/plateau-specialist`
   - 計画段階 → `/planner`

---

## 📊 エージェント能力マトリックス

| 能力 | Planner | Code Reviewer | Recovery Validator | PLATEAU Specialist |
|------|---------|---------------|-------------------|------------------|
| 計画・分解 | ⭐⭐⭐ | ⭐ | ⭐ | ⭐ |
| 規約チェック | ⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ |
| Null 安全性 | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| 地理データ処理 | ⭐ | ⭐ | ⭐ | ⭐⭐⭐ |
| パフォーマンス | ⭐⭐ | ⭐⭐ | ⭐ | ⭐⭐⭐ |
| アーキテクチャ | ⭐⭐⭐ | ⭐⭐ | ⭐ | ⭐⭐ |

---

## 🔄 エージェント間の連携

```
Planner
  ↓ (計画書を生成)
Code Reviewer (構文・品質チェック)
  ↓ (コード修正提案)
Recovery Validator (Null 安全性チェック)
  ↓ (Recovery 適合性確認)
PLATEAU Specialist (GIS データ処理チェック)
  ↓ (座標変換・メモリ確認)
✅ マージ ready
```

---

## 📞 トラブルシューティング

### Q: エージェントが要件を理解していない

**A**: より具体的な指示を提供：

```
❌ /planner Feature: New system

✅ /planner
   Feature: Implement puddle physics with gravity simulation
   Requirements:
   - Use PrefabManager for puddle prefab
   - Integrate with RainDropsCtrl
   - Must follow Recovery phase null safety
   - Target: 3 days effort
```

### Q: エージェントの判定に異議がある

**A**: 別のエージェント

に委任：

```
Code Reviewer が "✅ OK" → Recovery Validator で再確認
Recovery Validator が "⚠️ Issue" → Code Reviewer と協議
```

---

## 🎓 エージェント開発ガイド

新しいエージェントを追加したい場合：

1. [make-skill-template](../../.github/skills/make-skill-template/SKILL.md) を参照
2. `.md` ファイル作成（このフォルダに）
3. FRONTMATTER 追加（name, description）
4. 役割・チェックリスト定義
5. このファイル (README.md) に追加
6. `.github/copilot/README.md` に記載

---

**Last Updated**: 2026-01-20  
**Agent Version**: 1.0 (everything-claude-code adapted for OnoCoro)

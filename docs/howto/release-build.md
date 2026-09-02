---
title: ビルド版のリリース手順
description: ビルドから GitHub Release 公開までの手順と落とし穴
---

# ビルド版のリリース手順

数か月に一度しかやらないので毎回思い出せない作業。

バージョン番号の決め方そのものは
[project-rules/versioning.md](../project-rules/versioning.md) にある。
ここでは **ビルドしてから公開するまでの流れ**を扱う。

---

## 0. 前提

| 項目 | 値 |
|------|-----|
| リポジトリ | https://github.com/kuippa/OnoCoro |
| ビルドターゲット | StandaloneWindows64 |
| 出力先の命名 | `G:\unity\OnoCoro<YYYYMMDD>_build_prot` |
| zip の命名 | `OnoCoro2026 Prototype <YYYY.MM.DD.HH.mm>.zip` |

[NOTE] `gh` コマンドはこの環境に入っていない。
**GitHub Release の作成はブラウザから手作業**で行う。

---

## 1. ビルド前

### コミット状態を揃える

リリースしたビルドがどのコードから作られたか後から追えなくなるので、
**ビルド前に作業ツリーをきれいにして push しておく**。

```powershell
git status --short --branch
```

[WARN] `git add -A` は `* text=auto` の影響で大量の CRLF 警告を出す。
警告の量に驚いてもファイルが壊れているわけではないが、
**警告に紛れて意図しないファイルが混ざる**ので、パスを明示して add する。

### 同梱物を確認する

`Assets/StreamingAssets/staging/` の YAML はビルドにそのまま入る。
プレイヤーが解凍後に編集できてしまうので、
**実験用の設定や未完成のステージが残っていないか**を見ておく。

### 開発用の表示物を消す

シーンによっては作業用の備忘録（`cvsMemo`）が最前面に出したままになっている。
**配布物には載せない**ので、対象シーンで非アクティブにしてからビルドする。

対象は [ui-sorting-order.md](../reference/ui-sorting-order.md) の
「シーンに直接置かれているもの」を参照。

### バージョン番号

[NOTE] **手で変えるものは無い。**
ビルド開始時に `BuildVersionStamper` が `BuildDate.txt` の BUILD を +1 し、
`PlayerSettings.bundleVersion` にも同じ値を入れる。
仕組みは [versioning.md](../project-rules/versioning.md) を参照。

### TextMeshPro のシェーダー設定

[IMPORTANT] エディタでは見えていた UI テキストが、
ビルドすると真っ白／不可視になることがある。
Always Included Shaders の設定漏れが原因。
手順は [TEXMESHPRO_BUILD_SETTINGS.md](../TEXMESHPRO_BUILD_SETTINGS.md) を参照。

---

## 2. ビルド

Unity Editor から通常どおりビルドする。
ビルド開始時に `BuildVersionStamper` が走り、
`Assets/Resources/BuildDate.txt` の BUILD 番号が自動で +1 され、
`PlayerSettings.bundleVersion` にも同じ値が入る。

ビルド後の `BuildDate.txt` を確認する:

```powershell
Get-Content Assets\Resources\BuildDate.txt
```

```
0
0
24                 ← ビルド番号
2026.09.03.01.45   ← ビルド日時
StandaloneWindows64
```

この 5 行を "." で繋いだものがゲーム内のバージョン表示になる。
**タグ名とリリースノートはこの番号に合わせる**。

---

## 3. ビルド後の後始末

### DoNotShip フォルダを消す

```powershell
Remove-Item "G:\unity\OnoCoro<YYYYMMDD>_build_prot\Onokoro_BurstDebugInformation_DoNotShip" -Recurse -Force
```

Unity が名前で "DoNotShip" と言っているとおり、配布物に含めない。
サイズは小さいが、配布物にデバッグ情報を混ぜないための作法。

[NOTE] **ビルドのたびに再生成される。** 一度消しても、作り直したら再度消すこと。

### 動作確認（ここが一番大事）

[IMPORTANT] **エディタで動いたことは、ビルドで動く保証にならない。**

エディタはステージシーンを直接 Play するが、
ビルドは必ず タイトル → ステージ とシーン遷移する。
この差でだけ壊れる不具合が実際に出ている
（詳細は [unity-design-patterns.md](../project-rules/unity-design-patterns.md)
の「シーン寿命とオブジェクトの生成場所」）。

**タイトル画面から入って**、最低限これを確認する。

- [ ] タイトルからステージへ入れる
- [ ] ESC メニューが開き、**ゲーム終了とタイトルへ戻るが動く**
- [ ] UI テキストが表示されている（TextMeshPro のシェーダー漏れ確認）
- [ ] ステージのイベントが一通り走る（時間があれば完走する）
- [ ] タイトル画面のバージョン表示が今回のビルド番号になっている

うまく動かないときはログを見る。ビルド版のログはここに出る。

```
C:\Users\<ユーザー名>\AppData\LocalLow\Hagurachaya\Onokoro\<YYYYMMDD>_onocoro.log
```

Unity 標準の `Player.log` も同じフォルダにある。
自前のログのほうが読みやすいので、まずは日付つきのファイルを見る。

[NOTE] 「設定を読み込んだログは出ているのに、その機能の担当オブジェクトの
ログが一行も無い」ときは、オブジェクトが生成されていない。

---

## 4. zip に固める

DoNotShip を消したあとのフォルダごと固める。

```powershell
Compress-Archive -Path "G:\unity\OnoCoro<YYYYMMDD>_build_prot\*" `
                 -DestinationPath "G:\unity\OnoCoro2026 Prototype <YYYY.MM.DD.HH.mm>.zip"
```

[NOTE] 実績値: 展開 1,737 MB → zip 657 MB。
GitHub Release の添付ファイル上限は 1 ファイル 2 GB なので収まるが、
**アップロードに時間がかかる**ので余裕を見ておく。

---

## 5. ドキュメントを更新する

Release を作る前に、リポジトリ側を先に整える。

- [ ] `CHANGELOG.md` に今回の変更をまとめる
- [ ] `RELEASE_NOTES.md` を今回の内容で書き換える
- [ ] `README.md` のバージョンバッジを更新する

`RELEASE_NOTES.md` には最低限これを入れる。

```markdown
**Release Date**: YYYY-MM-DD
**Tag**: `v0.0.<BUILD>-prototype`
**Build Number**: <BUILD>
**In-Game Display**: Version: 0.0.<BUILD>.<日時>.StandaloneWindows64
```

ダウンロードリンクは Release を作ってから確定するので、
先に書く場合は次の形式になる。

```
https://github.com/kuippa/OnoCoro/releases/download/<タグ名>/<zipファイル名>
```

[WARN] zip のファイル名に**空白を入れるとリンクが壊れやすい**。
アップロード時に `OnoCoro_v0.0.24-prototype.zip` のような
空白なしの名前に変えておくとよい。

---

## 6. タグを打つ

```powershell
git tag v0.0.<BUILD>-prototype
git push origin v0.0.<BUILD>-prototype
```

タグ名の規則は [versioning.md](../project-rules/versioning.md) を参照。
プロトタイプ段階は `v0.0.<BUILD>-prototype`。

過去のタグ:

```powershell
git tag --sort=-creatordate | Select-Object -First 5
```

---

## 7. GitHub Release を作る

`gh` が無いのでブラウザから行う。

1. https://github.com/kuippa/OnoCoro/releases/new を開く
2. **Choose a tag** で 6 で push したタグを選ぶ
3. **Release title** に `OnoCoro v0.0.<BUILD>-prototype` を入れる
4. 本文に `RELEASE_NOTES.md` の内容を貼る
5. zip をドラッグしてアップロードする（時間がかかる）
6. プロトタイプ段階なら **Set as a pre-release** にチェックを入れる
7. Publish release

---

## 8. 公開後の確認

- [ ] Release ページから zip がダウンロードできる
- [ ] ダウンロードした zip を**別フォルダに解凍して起動する**
      （ビルドフォルダを直接触っていると、同梱漏れに気付けない）
- [ ] `RELEASE_NOTES.md` のリンクが 404 になっていない

---

## 落とし穴まとめ

| 症状 | 原因 |
|------|------|
| エディタでは動く機能がビルドで動かない | シーン遷移の有無の差。生成場所を疑う |
| UI テキストが出ない | Always Included Shaders の設定漏れ |
| ダウンロードリンクが 404 | zip 名の空白、またはタグ名の不一致 |
| どのコードのビルドか分からない | ビルド前に push していない |

---

## 実績

| 日付 | タグ | ビルド番号 | 備考 |
|------|------|-----------|------|
| 2026-03-10 | v0.0.21-prototype | 21 | |
| 2026-09-03 | （未確定） | 24 | CityHack 2026 版。zip 657 MB |

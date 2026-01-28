using System.Collections;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

/// <summary>
/// UI コントローラーの共通基底クラス
/// すべてのコントローラークラス（*Ctrl）はこれを継承して使用します。
/// 
/// 責務:
/// - IsInitialized フラグの一元管理
/// - 初期化パターン（必須 vs オンデマンド）の制御
/// - 初期化完了通知
/// - InitializationManager との連携
/// - 各子クラスの初期化タイミング制御
/// 
/// 初期化パターン:
/// 
/// [パターン A - 必須初期化]（IsInitializationRequired = true）
/// - ゲーム開始時に必ず初期化が必要
/// - Panels（5個）, Dialogs（4個）が該当
/// - InitializeManagers() が IsInitialized = true を待機
/// - 流れ: Start() → Initialize() コルーチン → IsInitialized = true
/// 
/// [パターン B - オンデマンド初期化]（IsInitializationRequired = false）
/// - UI 表示時などに遅延初期化
/// - HUD（8個）, Controls（5個）が該当
/// - Start() で即座に IsInitialized = true に設定
/// - InitializeManagers() は完了を待機しない
/// - 実際の初期化は UI 表示時などに明示的に呼び出す
/// 
/// 使用方法（パターン A - 必須初期化）:
/// <code>
/// public class MyCtrl : UIControllerBase
/// {
///     private Button _button;
///     
///     protected override void Awake()
///     {
///         base.Awake();
///         _button = GetComponentInChildren<Button>();
///     }
///     
///     protected override IEnumerator Initialize()
///     {
///         _button.onClick.AddListener(OnClick);
///         yield return null;
///     }
/// }
/// </code>
/// 
/// 使用方法（パターン B - オンデマンド初期化）:
/// <code>
/// public class MyHUDCtrl : UIControllerBase
/// {
///     protected override bool IsInitializationRequired => false;
///     
///     protected override void Awake()
///     {
///         base.Awake();
///         // 最小限の参照取得のみ
///     }
///     
///     protected override IEnumerator Initialize()
///     {
///         // 呼ばれない（isInitialized は Start() で true に）
///         yield return null;
///     }
///     
///     public void InitializeWhenNeeded()
///     {
///         // UI 表示時などに明示的に呼び出す
///     }
/// }
/// </code>
/// </summary>
public abstract class UIControllerBase : MonoBehaviour, IInitializable
{
    /// <summary>
    /// 初期化が完了したかどうか
    /// 
    /// パターン A（必須）: Initialize() コルーチン完了後に true
    /// パターン B（オンデマンド）: Start() で即座に true
    /// 
    /// IInitializable インターフェースを実装
    /// </summary>
    public bool IsInitialized { get; protected set; } = false;

    /// <summary>
    /// 初期化が必須かどうか
    /// 
    /// true（デフォルト）: ゲーム開始時に必須で初期化
    /// - Panels（EscMenuCtrl, TabMenuCtrl, NoticeCtrl, DebugInfoCtrl, SpawnMarkerPointerCtrl）
    /// - Dialogs（EventLogCtrl, GameTimerCtrl, InfoWindowCtrl, MessageBoxCtrl）
    /// - InitializeManagers() が IsInitialized = true を待機
    /// - Start() で Initialize() コルーチンを実行
    /// 
    /// false: オンデマンド初期化（UI 表示時など）
    /// - HUD（MarkerIndicatorCtrl, MarkerPointerCtrl, PathMakerCtrl, ScoreCtrl, TelopCtrl, TooltipInfoCtrl, MouseOverTipsCtrl, CircularIndicator）
    /// - Controls（ClickCtrl, ClosebtnCtrl, OkbtnCtrl, WindowCloseCtrl, WindowDragCtrl）
    /// - InitializeManagers() は完了を待機しない（Start() で即座に IsInitialized = true）
    /// - 実際の初期化は UI 表示時などに明示的に呼び出す
    /// </summary>
    protected virtual bool IsInitializationRequired => true;

    /// <summary>
    /// 子クラスが初期化を開始する前の前処理
    /// 各子クラスで必要に応じてオーバーライド
    /// 
    /// 用途:
    /// - コンポーネント参照の取得
    /// - フィールド初期化
    /// - イベント購読など
    /// 
    /// 実装例:
    /// <code>
    /// protected override void Awake()
    /// {
    ///     base.Awake();
    ///     _button = GetComponentInChildren<Button>();
    /// }
    /// </code>
    /// </summary>
    protected virtual void Awake()
    {
        // Awake 時は IsInitialized = false のまま
        IsInitialized = false;
    }

    /// <summary>
    /// 子クラスの初期化メイン処理
    /// Initialize() コルーチンで呼び出され、完了後に IsInitialized = true に設定されます。
    /// 
    /// パターン A（必須初期化）の場合のみ呼ばれます。
    /// パターン B（オンデマンド初期化）の場合は呼ばれません。
    /// 
    /// 実装上の注意:
    /// - コルーチンで非同期処理に対応
    /// - yield return null で1フレーム待機可能
    /// - yield return new WaitUntil(...) で特定条件待機可能
    /// - 必ず yield する（空コルーチンの場合も yield return null）
    /// </summary>
    protected abstract IEnumerator Initialize();

    /// <summary>
    /// Start() からの呼び出しパターン
    /// 初期化パターンを区別:
    /// 
    /// パターン A（必須初期化）:
    /// - InitializeAsync() コルーチンを開始
    /// - Initialize() が完了したら IsInitialized = true に設定
    /// 
    /// パターン B（オンデマンド初期化）:
    /// - IsInitialized を即座に true に設定
    /// - Initialize() は呼ばない
    /// 
    /// 子クラスでオーバーライドする場合は、base.Start() を最初に呼び出してください：
    /// <code>
    /// protected override void Start()
    /// {
    ///     base.Start();  // ← これが初期化を制御
    ///     // その他の Start() 処理...
    /// }
    /// </code>
    /// </summary>
    protected virtual void Start()
    {
        // [重要] 初期化が必須でなければ、すぐに IsInitialized = true に設定
        if (!IsInitializationRequired)
        {
            IsInitialized = true;
            Debug.Log($"[UIControllerBase] {this.GetType().Name} (オンデマンド初期化) スキップ");
            return;
        }

        // 初期化が必須なら、コルーチン開始
        StartCoroutine(InitializeAsync());
    }

    /// <summary>
    /// 初期化コルーチン
    /// Initialize() を実行し、完了後に IsInitialized = true を設定します。
    /// 
    /// パターン A（必須初期化）の場合のみ実行されます。
    /// 
    /// 流れ:
    /// 1. Initialize() コルーチン開始
    /// 2. Initialize() が yield で待機
    /// 3. Initialize() が完了
    /// 4. IsInitialized = true に設定
    /// 5. Debug.Log で完了メッセージを出力
    /// </summary>
    protected virtual IEnumerator InitializeAsync()
    {
        // Initialize() コルーチン実行
        yield return Initialize();

        // 初期化完了フラグを設定
        IsInitialized = true;

        Debug.Log($"[UIControllerBase] {this.GetType().Name} 初期化完了");
    }

    /// <summary>
    /// 初期化完了を待機するためのヘルパープロパティ
    /// 外部から yield return new WaitUntil(() => uiCtrl.IsReady) で使用可能
    /// </summary>
    public bool IsReady => IsInitialized;

    /// <summary>
    /// デバッグ用：現在の初期化状態を文字列で返す
    /// </summary>
    public string GetInitializationStatus()
    {
        return $"{this.GetType().Name}: IsInitialized={IsInitialized}, IsRequired={IsInitializationRequired}";
    }

    /// <summary>
    /// IInitializable インターフェース実装
    /// コンポーネント名を取得（ログ出力用）
    /// </summary>
    public string GetComponentName()
    {
        return this.GetType().Name;
    }
}


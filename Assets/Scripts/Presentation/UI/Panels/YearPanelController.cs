using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 年サイクル操作パネル（Season 3 W1 Task 4）
    ///
    /// 画面上部に「Year N / M」表示と Start Year ボタンを表示する。
    /// シーンへの手動配置は不要（起動時に自己構築し DontDestroyOnLoad で常駐、
    /// YearCycleSystem が有効なステージでのみ表示される）。
    /// フェーズ購読の初期化順序問題を避けるため、Update でのポーリングで状態を反映する。
    /// </summary>
    public class YearPanelController : MonoBehaviour
    {
        private const string _HOST_OBJECT_NAME = "YearPanelController";
        private const string _CANVAS_NAME = "YearPanelCanvas";
        private const int _CANVAS_SORT_ORDER = 100;
        private static readonly Vector2 _REFERENCE_RESOLUTION = new Vector2(1920f, 1080f);

        private const string _LABEL_FORMAT_YEAR = "Year {0} / {1}";
        private const string _LABEL_PLACEMENT_NOTE = "準備中：施策を配置して開始してください";
        private const string _LABEL_RUNNING_NOTE = "進行中…";
        private const string _LABEL_FINISHED = "シミュレーション終了";
        private const string _BUTTON_FORMAT_START = "Start Year {0}";

        private static readonly Color _PANEL_BG_COLOR = new Color(0f, 0f, 0f, 0.6f);
        private static readonly Color _BUTTON_BG_COLOR = new Color(0.85f, 0.3f, 0.1f, 0.95f);

        private GameObject _canvasRoot = null;
        private TextMeshProUGUI _yearLabel = null;
        private TextMeshProUGUI _noteLabel = null;
        private GameObject _startButtonRoot = null;
        private TextMeshProUGUI _startButtonLabel = null;

        private YearCyclePhase _displayedPhase = YearCyclePhase.Inactive;
        private int _displayedYear = -1;

        /// <summary>
        /// 起動時に自動生成（シーン配置不要のブートストラップ）
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            GameObject host = new GameObject(_HOST_OBJECT_NAME);
            host.AddComponent<YearPanelController>();
            DontDestroyOnLoad(host);
        }

        private void Awake()
        {
            BuildPanelUI();
            SetPanelVisible(false);
        }

        private void Update()
        {
            YearCyclePhase currentPhase = YearCycleSystem.CurrentPhase;
            int currentYear = YearCycleSystem.CurrentYear;

            // シーン遷移で EventLoader が消えた場合（タイトル画面等）はパネルを隠す
            if (currentPhase != YearCyclePhase.Inactive && EventLoader.instance == null)
            {
                currentPhase = YearCyclePhase.Inactive;
                currentYear = 0;
            }

            if (currentPhase == _displayedPhase && currentYear == _displayedYear)
            {
                return;
            }

            _displayedPhase = currentPhase;
            _displayedYear = currentYear;
            RefreshPanel(currentPhase, currentYear);
        }

        /// <summary>
        /// Start Year ボタン押下 → 現在年を開始
        /// </summary>
        private void OnClickStartYear()
        {
            GameTimerCtrl gameTimer = GameTimerCtrl.GetInstance();
            if (gameTimer == null)
            {
                Debug.LogWarning("[YearPanelController] GameTimerCtrl が見つかりません");
                return;
            }

            if (!gameTimer.StartYearCycle())
            {
                Debug.LogWarning("[YearPanelController] 年の開始に失敗しました");
            }
        }

        /// <summary>
        /// フェーズに応じて表示を更新
        /// </summary>
        private void RefreshPanel(YearCyclePhase phase, int year)
        {
            if (phase == YearCyclePhase.Inactive)
            {
                SetPanelVisible(false);
                return;
            }

            SetPanelVisible(true);
            int totalYears = GetTotalYears();

            if (phase == YearCyclePhase.Finished)
            {
                _yearLabel.SetText(_LABEL_FINISHED);
                _noteLabel.SetText("");
                _startButtonRoot.SetActive(false);
                return;
            }

            _yearLabel.SetText(string.Format(_LABEL_FORMAT_YEAR, year, totalYears));

            if (phase == YearCyclePhase.Placement)
            {
                _noteLabel.SetText(_LABEL_PLACEMENT_NOTE);
                _startButtonLabel.SetText(string.Format(_BUTTON_FORMAT_START, year));
                _startButtonRoot.SetActive(true);
            }
            else
            {
                _noteLabel.SetText(_LABEL_RUNNING_NOTE);
                _startButtonRoot.SetActive(false);
            }
        }

        private int GetTotalYears()
        {
            if (EventLoader.instance == null)
            {
                return 0;
            }
            return EventLoader.instance.GetYearCount();
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (_canvasRoot != null)
            {
                _canvasRoot.SetActive(isVisible);
            }
        }

        /// <summary>
        /// パネル UI をコードで構築（Canvas + 年ラベル + 注記 + Start ボタン）
        /// </summary>
        private void BuildPanelUI()
        {
            _canvasRoot = BuildCanvasRoot();

            GameObject panel = BuildPanelBackground(_canvasRoot.transform);
            _yearLabel = BuildLabel(panel.transform, "YearLabel", 36f, new Vector2(0f, 36f), new Vector2(420f, 48f));
            _noteLabel = BuildLabel(panel.transform, "NoteLabel", 20f, new Vector2(0f, -4f), new Vector2(420f, 30f));
            BuildStartButton(panel.transform);
        }

        private GameObject BuildCanvasRoot()
        {
            GameObject canvasObject = new GameObject(_CANVAS_NAME);
            canvasObject.transform.SetParent(this.transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = _CANVAS_SORT_ORDER;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _REFERENCE_RESOLUTION;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject;
        }

        private GameObject BuildPanelBackground(Transform parent)
        {
            GameObject panelObject = new GameObject("YearPanel");
            panelObject.transform.SetParent(parent, false);

            Image background = panelObject.AddComponent<Image>();
            background.color = _PANEL_BG_COLOR;

            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -12f);
            rect.sizeDelta = new Vector2(460f, 160f);
            return panelObject;
        }

        private TextMeshProUGUI BuildLabel(Transform parent, string objectName, float fontSize, Vector2 position, Vector2 size)
        {
            GameObject labelObject = new GameObject(objectName);
            labelObject.transform.SetParent(parent, false);

            TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;

            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return label;
        }

        private void BuildStartButton(Transform parent)
        {
            _startButtonRoot = new GameObject("StartYearButton");
            _startButtonRoot.transform.SetParent(parent, false);

            Image buttonImage = _startButtonRoot.AddComponent<Image>();
            buttonImage.color = _BUTTON_BG_COLOR;

            Button button = _startButtonRoot.AddComponent<Button>();
            button.onClick.AddListener(OnClickStartYear);

            RectTransform rect = _startButtonRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -46f);
            rect.sizeDelta = new Vector2(260f, 52f);

            _startButtonLabel = BuildLabel(_startButtonRoot.transform, "StartYearButtonLabel", 26f, Vector2.zero, new Vector2(260f, 52f));
        }
    }
}

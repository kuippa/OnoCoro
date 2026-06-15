using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 結果表示パネル（全画面・Season 3 W3 Task 2/3）
    ///
    /// 年末（YearResult フェーズ）に「Year N 結果」、全年完走（Finished フェーズ）に
    /// 「3 年間の総括」を全画面で表示する。表示中は GameSpeedManager.SetGameSpeed(0) で
    /// ゲーム進行を停止する（テロップ TelopCtrl の全画面オーバーレイ方式を参考）。
    /// シーン配置不要（起動時に自己構築・DontDestroyOnLoad 常駐）。
    /// </summary>
    public class ResultPanelController : MonoBehaviour
    {
        private const string _HOST_OBJECT_NAME = "ResultPanelController";
        private const string _CANVAS_NAME = "ResultPanelCanvas";
        private const int _CANVAS_SORT_ORDER = 200;  // YearPanel(100) より前面
        private static readonly Vector2 _REFERENCE_RESOLUTION = new Vector2(1920f, 1080f);

        private static readonly Color _BG_COLOR = new Color(0.05f, 0.05f, 0.08f, 0.92f);
        private static readonly Color _BUTTON_COLOR = new Color(0.85f, 0.3f, 0.1f, 0.95f);
        private static readonly Color _SUBBUTTON_COLOR = new Color(0.3f, 0.35f, 0.45f, 0.95f);

        private GameObject _canvasRoot = null;
        private TextMeshProUGUI _titleLabel = null;
        private TextMeshProUGUI _bodyLabel = null;
        private GameObject _nextButtonRoot = null;
        private TextMeshProUGUI _nextButtonLabel = null;
        private GameObject _titleButtonRoot = null;

        private YearCyclePhase _displayedPhase = YearCyclePhase.Inactive;
        private bool _isPaused = false;
        private float _speedBeforePause = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            GameObject host = new GameObject(_HOST_OBJECT_NAME);
            host.AddComponent<ResultPanelController>();
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
            if (currentPhase == _displayedPhase)
            {
                return;
            }
            _displayedPhase = currentPhase;
            RefreshPanel(currentPhase);
        }

        private void RefreshPanel(YearCyclePhase phase)
        {
            if (phase == YearCyclePhase.YearResult)
            {
                ShowYearResult(YearCycleSystem.CurrentYear);
                return;
            }
            if (phase == YearCyclePhase.Finished)
            {
                ShowSummary();
                return;
            }
            HidePanel();
        }

        /// <summary>
        /// 年末の結果を全画面表示し進行停止
        /// </summary>
        private void ShowYearResult(int year)
        {
            string body;
            if (DamageReportSystem.TryGetYearResult(year, out YearResult result))
            {
                body = BuildYearResultText(result);
            }
            else
            {
                body = "結果データを取得できませんでした。";
            }

            _titleLabel.SetText($"Year {year} 結果");
            _bodyLabel.SetText(body);
            bool isFinalYear = EventLoader.instance != null && year >= EventLoader.instance.GetYearCount();
            _nextButtonLabel.SetText(isFinalYear ? "総括を見る" : "次の年へ");
            _nextButtonRoot.SetActive(true);
            _titleButtonRoot.SetActive(false);

            SetPanelVisible(true);
            PauseGame();
        }

        /// <summary>
        /// 全年完走後の総括を全画面表示
        /// </summary>
        private void ShowSummary()
        {
            YearResult summary = DamageReportSystem.GetSummary();

            _titleLabel.SetText("3 年間の総括");
            _bodyLabel.SetText(BuildSummaryText(summary));
            _nextButtonLabel.SetText("もう一度");
            _nextButtonRoot.SetActive(true);
            _titleButtonRoot.SetActive(true);

            SetPanelVisible(true);
            PauseGame();
        }

        private void HidePanel()
        {
            SetPanelVisible(false);
            ResumeGame();
        }

        private string BuildYearResultText(YearResult result)
        {
            string savedNote = result.SavedBuildings > 0
                ? $"  → 施策で {result.SavedBuildings} 棟を延焼から救いました"
                : "  → 施策の効果はまだ出ていません";

            return
                $"地震倒壊: {result.QuakeCollapse} 棟（避けられない初期被害）\n" +
                $"火災延焼: {result.FireSpread} 棟\n" +
                savedNote + "\n\n" +
                $"今年の投資: {result.Investment} ゴールド\n" +
                $"ROI: {result.Roi:F1}（投資100あたり救った棟数）";
        }

        private string BuildSummaryText(YearResult summary)
        {
            return
                $"累計投資: {summary.Investment} ゴールド\n" +
                $"地震倒壊（累計）: {summary.QuakeCollapse} 棟\n" +
                $"火災延焼（累計）: {summary.FireSpread} 棟\n" +
                $"救った建物（累計）: {summary.SavedBuildings} 棟\n" +
                $"総合 ROI: {summary.Roi:F1}\n" +
                $"避難カバー率: {summary.EvacuationCoverage:F0}%\n\n" +
                BuildSummaryMessage(summary);
        }

        private string BuildSummaryMessage(YearResult summary)
        {
            if (summary.SavedBuildings == 0)
            {
                return "施策を増やせば、もっと多くの建物を火災から守れたかもしれません。";
            }
            if (summary.Roi >= 2f)
            {
                return "少ない投資で大きな被害を防げました。効率的な防災投資です。";
            }
            return "防災投資で火災被害を抑えられました。配置場所を工夫するとさらに効果的です。";
        }

        private void OnClickNext()
        {
            if (_displayedPhase == YearCyclePhase.YearResult)
            {
                ResumeGame();
                YearCycleSystem.AdvanceFromResult();
                return;
            }
            if (_displayedPhase == YearCyclePhase.Finished)
            {
                ResumeGame();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        private void OnClickBackToTitle()
        {
            ResumeGame();
            SceneLoaderManager.LoadScene(SceneLoaderManager.LoadSceneName.TitlteStart.ToString());
        }

        private void PauseGame()
        {
            if (_isPaused)
            {
                return;
            }
            _speedBeforePause = GameSpeedManager.GetGameSpeed();
            if (_speedBeforePause <= 0.01f)
            {
                _speedBeforePause = 1f;
            }
            GameSpeedManager.SetGameSpeed(0f);
            _isPaused = true;
        }

        private void ResumeGame()
        {
            if (!_isPaused)
            {
                return;
            }
            GameSpeedManager.SetGameSpeed(_speedBeforePause);
            _isPaused = false;
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (_canvasRoot != null)
            {
                _canvasRoot.SetActive(isVisible);
            }
        }

        // =============================================
        // UI 構築（コードで全画面パネルを生成）
        // =============================================

        private void BuildPanelUI()
        {
            _canvasRoot = BuildCanvasRoot();
            GameObject panel = BuildFullScreenBackground(_canvasRoot.transform);

            _titleLabel = BuildLabel(panel.transform, "ResultTitle", 56f, new Vector2(0f, 260f), new Vector2(1200f, 90f), TextAlignmentOptions.Center);
            _bodyLabel = BuildLabel(panel.transform, "ResultBody", 34f, new Vector2(0f, 0f), new Vector2(1100f, 380f), TextAlignmentOptions.Center);

            _nextButtonRoot = BuildButton(panel.transform, "NextButton", new Vector2(0f, -300f), _BUTTON_COLOR, OnClickNext, out _nextButtonLabel);
            _titleButtonRoot = BuildButton(panel.transform, "TitleButton", new Vector2(0f, -380f), _SUBBUTTON_COLOR, OnClickBackToTitle, out TextMeshProUGUI titleButtonLabel);
            titleButtonLabel.SetText("タイトルへ");
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

        private GameObject BuildFullScreenBackground(Transform parent)
        {
            GameObject panelObject = new GameObject("ResultBackground");
            panelObject.transform.SetParent(parent, false);

            Image background = panelObject.AddComponent<Image>();
            background.color = _BG_COLOR;  // 不透明背景 + GraphicRaycaster で入力ブロック

            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panelObject;
        }

        private TextMeshProUGUI BuildLabel(Transform parent, string objectName, float fontSize, Vector2 position, Vector2 size, TextAlignmentOptions alignment)
        {
            GameObject labelObject = new GameObject(objectName);
            labelObject.transform.SetParent(parent, false);

            TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;

            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return label;
        }

        private GameObject BuildButton(Transform parent, string objectName, Vector2 position, Color color, UnityEngine.Events.UnityAction onClick, out TextMeshProUGUI label)
        {
            GameObject buttonObject = new GameObject(objectName);
            buttonObject.transform.SetParent(parent, false);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = color;

            Button button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(onClick);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(320f, 64f);

            label = BuildLabel(buttonObject.transform, objectName + "Label", 30f, Vector2.zero, new Vector2(320f, 64f), TextAlignmentOptions.Center);
            return buttonObject;
        }
    }
}

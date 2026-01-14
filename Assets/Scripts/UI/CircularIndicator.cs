using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// using System.Collections.Generic;
using System;
using CommonsUtility;
using TMPro;

public class CircularIndicator : MonoBehaviour
{
    private float _innerRadius = 0.8f;
    private float _outerRadius = 1.2f;
    private int _segments = 360;  // 多角形の頂点数
    private float _duration = 5f;
    private Color _filledColor = Color.green;
    private Mesh _mesh;
    private float _currentTime = 0f;
    private bool _isRunning = false;
    private MeshRenderer _meshRenderer;
    // private Text _timeText;
    private TextMeshPro _textMesh;
    private Action _onCompleteCallback;  // コールバック関数を保持する変数


    private void Awake()
    {
        // GameObject txtTime = this.gameObject.transform.Find("Canvas/txtTime").gameObject;
        // txtTime.SetActive(false);
        // _timeText = txtTime.GetComponent<Text>();
        // _timeText.text = _duration.ToString("F1");

        GameObject cvsTime = this.gameObject.transform.Find("tmpMesh").gameObject;        
        cvsTime.SetActive(false);
        _textMesh = cvsTime.GetComponent<TextMeshPro>();
        _textMesh.text = _duration.ToString("F1");

        _mesh = new Mesh();
        _mesh.name = "CircularRibbonMesh";
        this.GetComponent<MeshFilter>().mesh = _mesh;
        _meshRenderer = this.GetComponent<MeshRenderer>();
        _filledColor = _meshRenderer.material.color;
        CreateMesh();

        StartIndicator(_duration, null, this.gameObject);
    }

    private void CreateMesh(int startAngle = 0, int endAngle = 360)
    {
        Vector3[] vertices = new Vector3[_segments * 2];
        int[] triangles = new int[_segments * 6];
        Vector2[] uv = new Vector2[_segments * 2];
        Color[] colors = new Color[_segments * 2];

        for (int i = startAngle; i < endAngle; i++)
        {
            float angle = i * Mathf.PI * 2f / _segments;
            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);

            vertices[i * 2] = new Vector3(x * _innerRadius, y * _innerRadius, 0);
            vertices[i * 2 + 1] = new Vector3(x * _outerRadius, y * _outerRadius, 0);

            uv[i * 2] = new Vector2((float)i / _segments, 0);
            uv[i * 2 + 1] = new Vector2((float)i / _segments, 1);

            if (i < _segments - 1)
            {
                triangles[i * 6] = i * 2;
                triangles[i * 6 + 1] = i * 2 + 1;
                triangles[i * 6 + 2] = i * 2 + 2;
                triangles[i * 6 + 3] = i * 2 + 1;
                triangles[i * 6 + 4] = i * 2 + 3;
                triangles[i * 6 + 5] = i * 2 + 2;
            }
            else
            {
                triangles[i * 6] = i * 2;
                triangles[i * 6 + 1] = i * 2 + 1;
                triangles[i * 6 + 2] = 0;
                triangles[i * 6 + 3] = i * 2 + 1;
                triangles[i * 6 + 4] = 1;
                triangles[i * 6 + 5] = 0;
            }
        }
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uv;
        if (_meshRenderer.materials != null) {
            // _meshRenderer.material.color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f, 0.5f, 0.5f);
            _meshRenderer.material.color = _filledColor;
        }
    }

    private void SetTimeText(float time)
    {
        // _timeText.gameObject.SetActive(true);
        // _timeText.text = time.ToString("F1");
        _textMesh.gameObject.SetActive(true);
        _textMesh.text = time.ToString("F1");
    }

    private void UpdateMesh(float fillAmount)
    {
        Color[] colors = new Color[_segments * 2];
        int fillSegments = Mathf.RoundToInt(fillAmount * _segments);
        CreateMesh(0, fillSegments);
    }

    internal void StartIndicator(float duration, Action onCompleteCallback = null, GameObject parent = null)
    {

        this.transform.SetParent(parent.transform, false);

        _duration = duration;
        _onCompleteCallback = onCompleteCallback;  // コールバックを保存
        StartFill();
    }

    private void StartFill()
    {
        _currentTime = 0f;
        _isRunning = true;
    }

    // テスト用：色を切り替える関数
    private void ToggleColors()
    {
        _filledColor = UnityEngine.Random.ColorHSV();
        UpdateMesh(_currentTime / _duration);
    }

    private IEnumerator DelayMethod(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        // コールバックがセットされている場合は実行
        _onCompleteCallback?.Invoke();
        _mesh.Clear();
        GameObjectTreat.DestroyAll(this.gameObject);
    }

    private void Update()
    {
        if (_isRunning)
        {
            _currentTime += Time.deltaTime;
            float fillAmount = Mathf.Clamp01(_currentTime / _duration);
            UpdateMesh(fillAmount);
            SetTimeText(_duration - _currentTime);
            if (fillAmount >= 1f)
            {
                // 遅延してこのオブジェクトを破棄する
                StartCoroutine(DelayMethod(0.2f));

                _isRunning = false;
            }
        }

        // テスト用：スペースキーを押すと色が切り替わる
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     ToggleColors();
        // }
    }
}

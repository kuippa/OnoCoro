using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 桜の木（ステージオーナメント）を制御
    /// 
    /// 責務：
    /// - マテリアル管理（初期状態 ← → 透明状態）
    /// - 外部からの指示でマテリアル切り替え
    /// 
    /// 使用方法：
    /// 1. GameObject に AttachComponent
    /// 2. Initialize() で初期化
    /// 3. SetMatToInvisible() / SetMatToOriginal() で切り替え
    /// </summary>
    public class TreeSakura : MonoBehaviour
    {
        private const string _PLANT_SAKURA_NAME = "Plant_Sakura";
        private const int _ORIGINAL_MAT_INDEX = 0;
        private const string _INVISIBLE_MAT_PATH = "Materials/Invisible";

        private Material _originalMaterial = null;
        private Material _invisibleMaterial = null;
        private Renderer _renderer = null;

        /// <summary>
        /// 初期化
        /// オブジェクト構造：TreeSakura (このスクリプト) → Plant_Sakura (子) → MeshRenderer
        /// </summary>
        internal void Initialize()
        {
            // 子オブジェクト "Plant_Sakura" を取得
            Transform plantSakura = transform.Find(_PLANT_SAKURA_NAME);
            if (plantSakura == null)
            {
                Debug.LogWarning($"[TreeSakura] Child object '{_PLANT_SAKURA_NAME}' not found");
                return;
            }

            // MeshRenderer を取得
            _renderer = plantSakura.GetComponent<Renderer>();
            if (_renderer == null)
            {
                Debug.LogWarning($"[TreeSakura] Renderer not found on '{_PLANT_SAKURA_NAME}'");
                return;
            }

            // オリジナルマテリアルはオブジェクトから直接取得
            if (_renderer.materials.Length > _ORIGINAL_MAT_INDEX)
            {
                _originalMaterial = _renderer.materials[_ORIGINAL_MAT_INDEX];
            }
            else
            {
                Debug.LogWarning($"[TreeSakura] Material at index {_ORIGINAL_MAT_INDEX} not found");
                return;
            }

            // 透明マテリアルは Resources から読み込む
            _invisibleMaterial = Resources.Load<Material>(_INVISIBLE_MAT_PATH);
            if (_invisibleMaterial == null)
            {
                Debug.LogWarning("[TreeSakura] Invisible material loading failed");
            }
            SetMatToInvisible();
        }

        /// <summary>
        /// 透明マテリアルに切り替え
        /// </summary>
        internal void SetMatToInvisible()
        {
            if (_renderer == null || _invisibleMaterial == null)
            {
                return;
            }

            _renderer.material = _invisibleMaterial;
            Debug.Log("[TreeSakura] Material changed to Invisible");
        }

        /// <summary>
        /// 元のマテリアルに戻す
        /// </summary>
        internal void SetMatToOriginal()
        {
            if (_renderer == null || _originalMaterial == null)
            {
                return;
            }

            _renderer.material = _originalMaterial;
            Debug.Log("[TreeSakura] Material changed to Original");
        }

        private void Awake()
        {
            Initialize();
        }

    }
}

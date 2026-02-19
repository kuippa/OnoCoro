using UnityEngine;

namespace CommonsUtility
{
    /// <summary>
    /// オブジェクトの初期スポーン位置を記憶する汎用コンポーネント。
    /// NarakuTriggerHandler が全試行失敗時に戻す先として参照する。
    /// スポーン処理側で AgentJumpToStartPosition 等の後に SpawnOrigin をセットすること。
    /// </summary>
    internal class SpawnOriginTracker : MonoBehaviour
    {
        internal Vector3 SpawnOrigin { get; private set; } = Vector3.zero;

        internal void SetSpawnOrigin(Vector3 origin)
        {
            SpawnOrigin = origin;
        }

        internal bool HasSpawnOrigin()
        {
            return SpawnOrigin != Vector3.zero;
        }
    }
}

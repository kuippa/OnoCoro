using System.Collections;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

public class DustBoxCtrl : MonoBehaviour
{
    private const float _DUST_CHECK_TIME = 2.5f;

    /// <summary>
    /// Garbage が進入した時に呼ばれます
    /// DustBoxTriggerHandler から呼び出されます
    /// </summary>
    /// <param name="other">進入した Collider（Garbage）</param>
    internal void OnGarbageEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        StartCoroutine(DeleteDust(other));
    }

    private IEnumerator DeleteDust(Collider other)
    {
        yield return new WaitForSeconds(_DUST_CHECK_TIME);
        
        // オブジェクトが既に破棄されていないかチェック
        if (other == null)
        {
            yield break;
        }
        
        // スコア計算
        GarbageCube garbageCube = other.gameObject.GetComponent<GarbageCube>();
        if (garbageCube != null)
        {
            UnitStruct unitStruct = garbageCube.GetUnitStruct();
            int score = ScoreCtrl.GetTotalGarbageScore(other);
            ScoreCtrl.UpdateAndDisplayScore(score, unitStruct.ScoreType);
        }
        
        // ゴミオブジェクトを削除
        GameObjectTreat.DestroyAll(other.gameObject);
    }

    private void Awake()
    {
        // DustBoxTriggerHandler をアタッチ
        GameObjectTreat.GetOrAddComponent<DustBoxTriggerHandler>(gameObject);
    }
}

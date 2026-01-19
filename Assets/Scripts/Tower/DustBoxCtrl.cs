using System.Collections;
using CommonsUtility;
using UnityEngine;

public class DustBoxCtrl : MonoBehaviour
{
    private const float _DUST_CHECK_TIME = 2.5f;

    private void OnTriggerEnter(Collider other)
    {
        // ゴミがダストボックスに入ったら削除処理を開始
        if (other.gameObject.tag == GameEnum.TagType.Garbage.ToString())
        {
            StartCoroutine(DeleteDust(other));
        }
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
}

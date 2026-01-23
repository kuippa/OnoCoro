using System.Collections;
using CommonsUtility;
using UnityEngine;


public class WaterSphereCtrl : MonoBehaviour
{
    private const float _FLOWING_TIME = 15f; // x秒後に消去


    private IEnumerator InvokeWithGameObject(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        WaterFlowing(obj);
    }

    private void WaterFlowing(GameObject target)
    {
        GameObjectTreat.DestroyAll(target);
    }

    private void Start()
    {
        // x秒後を消去
        StartCoroutine(InvokeWithGameObject(this.gameObject, _FLOWING_TIME));
    }
}

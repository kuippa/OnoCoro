using System;
using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public class PuddleController : MonoBehaviour
{
    private const float PUDDLE_INCREASE = 4f;    // 雨粒あたりの増加サイズm^2

    private float GetPuddleDimension(GameObject puddle)
    {
        // 水たまりの面積を取得する
        Vector3 scale = puddle.transform.localScale;
        return scale.x * scale.z;
    }

    private void ChangePuddleSize()
    {
        // 1個の雨粒につき、1平米ずつ大きくする

        // 現在の水たまりのサイズを取得
        float puddleDimension = GetPuddleDimension(this.gameObject);
        float newScale = MathF.Sqrt(puddleDimension + PUDDLE_INCREASE);
        this.transform.localScale = new Vector3(newScale, 0, newScale);
    }

    private Vector3 GetPuddleHighestPoint(GameObject puddle, GameObject other_puddle)
    {
        // 水たまりの最高点を取得する
        Vector3 highestPoint = puddle.transform.position;
        if (other_puddle.transform.position.y > highestPoint.y)
        {
            highestPoint.y = other_puddle.transform.position.y;
        }
        return highestPoint;
    }

    private void MergerPuddle(GameObject other_puddle)
    {
        // 他の水たまりと合体する
        float puddleDimension = GetPuddleDimension(this.gameObject);
        float othrDimension = GetPuddleDimension(other_puddle);
        float newScale = MathF.Sqrt(puddleDimension + othrDimension);

        if (puddleDimension > othrDimension)
        {
            // 他の水たまりを消す
            this.transform.localScale = new Vector3(newScale, 0, newScale);
            // this.transform.position = GetPuddleHighestPoint(this.gameObject, other_puddle);
            GameObjectTreat.DestroyAll(other_puddle);
        }
        else
        {
            // 自分を消す
            other_puddle.transform.localScale = new Vector3(newScale, 0, newScale);
            // other_puddle.transform.position = GetPuddleHighestPoint(other_puddle, this.gameObject);
            GameObjectTreat.DestroyAll(this.gameObject);
        }
    }

    /// <summary>
    /// RainDrop が進入した時に呼ばれます
    /// PuddleTriggerHandler から呼び出されます
    /// </summary>
    /// <param name="other">進入した Collider（RainDrop）</param>
    internal void OnRainDropEnter(Collider other)
    {
        if (other == null || other.gameObject == null)
        {
            return;
        }

        ChangePuddleSize();
        GameObjectTreat.DestroyAll(other.gameObject);
    }

    /// <summary>
    /// 他の Puddle が進入した時に呼ばれます
    /// PuddleTriggerHandler から呼び出されます
    /// </summary>
    /// <param name="other">進入した Collider（Puddle）</param>
    internal void OnPuddleEnter(Collider other)
    {
        if (other == null || other.gameObject == null)
        {
            return;
        }

        MergerPuddle(other.gameObject);
    }

    private void Awake()
    {
        // PuddleTriggerHandler をアタッチ
        GameObjectTreat.GetOrAddComponent<PuddleTriggerHandler>(gameObject);
    }
}

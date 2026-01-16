// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// SweepCtrl
using CommonsUtility;
using UnityEngine;

public class SweepCtrl : MonoBehaviour
{
	private float _lastTriggerStayTime;

	private const float _TRIGGER_STAY_INTERVAL = 0.02f;

	private void ChangeCubeSize(Collider other)
	{
		Vector3 localScale = other.transform.localScale;
		other.transform.localScale = ChangeScaleLimiter(localScale);
	}

	private Vector3 ChangeScaleLimiter(Vector3 scale)
	{
		scale.x = CalcScale(scale.x);
		scale.y = CalcScale(scale.y);
		scale.z = CalcScale(scale.z);
		return scale;
	}

	private float CalcScale(float scale)
	{
		if (!(scale <= 0.2f))
		{
			scale -= 0.4f;
			if (scale < 0.2f)
			{
				scale = 0.2f;
			}
		}
		return scale;
	}

	private GarbageCube GetGarbageCube(Collider other)
	{
		GarbageCube garbageCube = other.gameObject.GetComponent<GarbageCube>();
		if (garbageCube == null)
		{
			garbageCube = other.gameObject.AddComponent<GarbageCube>();
		}
		return garbageCube;
	}

	private void CalcScore(Collider other)
	{
		UnitStruct unitStruct = GetGarbageCube(other).GetUnitStruct();
		ScoreCtrl.UpdateAndDisplayScore(ScoreCtrl.GetSliceGarbageScore(other), unitStruct.ScoreType);
	}

	internal void SweepGarbage(Collider other)
	{
		if (other.tag == GameEnum.TagType.Garbage.ToString() || other.tag == GameEnum.TagType.Ash.ToString())
		{
			float num = ScoreCtrl.CalcGarbageMagnification(other.transform.localScale);
			CalcScore(other);
			if (num <= 0.6f)
			{
				GameObjectTreat.DestroyAll(other.gameObject);
			}
			else
			{
				ChangeCubeSize(other);
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		float time = Time.time;
		if (!(time - _lastTriggerStayTime < 0.02f))
		{
			_lastTriggerStayTime = time;
			SweepGarbage(other);
		}
	}

	private void Awake()
	{
	}
}

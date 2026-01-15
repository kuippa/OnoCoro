// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PowerCubeCtrl
using CommonsUtility;
using UnityEngine;

public class PowerCubeCtrl : MonoBehaviour
{
	private PowerCube _powerCube;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
		{
			UnitStruct unitStruct = _powerCube.UnitStruct;
			int baseScore = unitStruct.BaseScore;
			if (ScoreCtrl.IsScorePositiveInt(baseScore, unitStruct.ScoreType))
			{
				ScoreCtrl.UpdateAndDisplayScore(baseScore, unitStruct.ScoreType);
				GameObjectTreat.DestroyAll(base.gameObject);
			}
		}
	}

	private void Awake()
	{
		_powerCube = base.gameObject.AddComponent<PowerCube>();
	}
}

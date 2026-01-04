// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// TowerSweeper
using System.Collections.Generic;
using System.Linq;
using CommonsUtility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class TowerSweeper : MonoBehaviour
{
	private GameObject _MyDeck;

	private NavMeshAgent _NavMeshAgent;

	private GameObject _targetGarbage;

	private List<GameObject> _AimGarbageLists = new List<GameObject>();

	private List<GameObject> _IgnoreGarbageLists = new List<GameObject>();

	private float _lastTriggerStayTime;

	private const float _TRIGGER_STAY_INTERVAL = 0.02f;

	private const float _TARGET_DEL_DISTANCE = 1.2f;

	private double _time;

	public GameObject _Active_Dock;

	public GameObject _Sleep_Dock;

	private bool _chargeMode;

	private const float _BATTERY_ORG_SIZE = 0.5f;

	internal float _LOOP_TIME = 0.6f;

	internal float _FULL_BATTERY = 100f;

	internal float _HP = 100f;

	internal float _DECREASE_BATTERY = 1f;

	internal float _CHARGE_BATTERY = 5f;

	private const float _BATTERY_DISTANCE = 1.8f;

	private bool _isDelete;

	public void CreateSweeperUnit(Vector3 setPoint)
	{
		base.tag = GameEnum.TagType.TowerSweeper.ToString();
		int num = GameObjectTreat.IndexObjectByTag(base.tag);
		base.name = GameEnum.ModelsType.Sweeper.ToString() + num;
		this.AddComponent<Sweeper>();
		GetComponent<Sweeper>()._item_struct.ItemID = base.name;
		GetComponent<Sweeper>()._unit_struct.UnitID = base.name;
		base.transform.position = setPoint;
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/WorkUnit/TowerDock")) as GameObject;
		setPoint.x = setPoint.x + base.transform.localScale.x / 2f + 0.1f;
		gameObject.transform.position = setPoint;
		gameObject.tag = GameEnum.TagType.TowerDock.ToString();
		gameObject.name = gameObject.tag + num;
		_MyDeck = gameObject;
		if (_NavMeshAgent != null)
		{
			_NavMeshAgent.enabled = true;
		}
		ChangeBatteryDockMode(mode: false);
	}

	private void OnDestroy()
	{
		GameObjectTreat.DestroyAll(base.gameObject);
	}

	private void Awake()
	{
		_NavMeshAgent = NavMeshCtrl.GetNavMeshAgent(base.gameObject);
		ChangeBatteryDockMode(mode: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		float time = Time.time;
		if (!(time - _lastTriggerStayTime <= 0.02f))
		{
			_lastTriggerStayTime = time;
			if (other.tag == GameEnum.TagType.Garbage.ToString() || other.tag == GameEnum.TagType.Ash.ToString())
			{
				GameObject gameObject = other.gameObject;
				GameObjectTreat.DebugColorChange(gameObject, Color.red);
				SetTargetGarbage(gameObject);
			}
		}
	}

	private void SetTargetGarbage(GameObject other)
	{
		if (!_AimGarbageLists.Contains(other))
		{
			_AimGarbageLists.Add(other);
		}
		if (CompareDistance(other))
		{
			GameObjectTreat.DebugColorChange(_targetGarbage, Color.green);
			_targetGarbage = other.gameObject;
		}
		else
		{
			GameObjectTreat.DebugColorChange(_targetGarbage, Color.blue);
		}
	}

	private bool CompareDistance(GameObject compareObject)
	{
		if (_IgnoreGarbageLists.Contains(compareObject))
		{
			return false;
		}
		if (_targetGarbage == null)
		{
			_targetGarbage = compareObject.gameObject;
		}
		if (Vector3.Distance(base.transform.position, _targetGarbage.transform.position) >= Vector3.Distance(base.transform.position, compareObject.transform.position))
		{
			return true;
		}
		return false;
	}

	private void MoveControl()
	{
		if (!CheckBattery())
		{
			return;
		}
		UpdateTargetGarbage();
		if (_targetGarbage == null)
		{
			NavMeshCtrl.LookAround(_NavMeshAgent, base.transform);
			return;
		}
		Vector3 position = _targetGarbage.transform.position;
		NavMeshCtrl.SetAgentSpeed(_NavMeshAgent);
		if (NavMeshCtrl.IsSameDestination(_NavMeshAgent, position))
		{
			return;
		}
		if (!NavMeshCtrl.SetNavMeshDestination(_NavMeshAgent, position, base.transform))
		{
			Vector3 vector = position;
			Debug.Log("SetNavMeshDestination false:" + vector.ToString());
			_targetGarbage = SetTargetGarbageIgnoreLists(_targetGarbage);
			return;
		}
		switch (_NavMeshAgent.pathStatus)
		{
		case NavMeshPathStatus.PathComplete:
			HandlePathComplete();
			break;
		case NavMeshPathStatus.PathPartial:
			if (_NavMeshAgent.remainingDistance <= 1.2f)
			{
				TriggerSweepGarbage();
			}
			break;
		case NavMeshPathStatus.PathInvalid:
			_targetGarbage = SetTargetGarbageIgnoreLists(_targetGarbage);
			break;
		}
	}

	private void HandlePathComplete()
	{
		if (!_NavMeshAgent.hasPath)
		{
			_targetGarbage = SetTargetGarbageIgnoreLists(_targetGarbage);
		}
		else if (_NavMeshAgent.remainingDistance <= 1.2f)
		{
			TriggerSweepGarbage();
		}
	}

	private void TriggerSweepGarbage()
	{
		SweepCtrl component = base.transform.Find("head").gameObject.GetComponent<SweepCtrl>();
		if (component != null)
		{
			component.SweepGarbage(_targetGarbage.GetComponent<Collider>());
		}
	}

	private void UpdateTargetGarbage()
	{
		if (_targetGarbage == null || !IsValidTarget(_targetGarbage))
		{
			_targetGarbage = GetBestTargetFromList();
		}
	}

	private bool IsValidTarget(GameObject target)
	{
		if (target != null)
		{
			return !_IgnoreGarbageLists.Contains(target);
		}
		return false;
	}

	private GameObject GetBestTargetFromList()
	{
		return (from g in _AimGarbageLists.Where(IsValidTarget)
			orderby Vector3.Distance(base.transform.position, g.transform.position)
			select g).FirstOrDefault();
	}

	private bool CheckBattery()
	{
		if (_HP <= 0f || _chargeMode)
		{
			ChargeBattery();
			return false;
		}
		DecreaseHP();
		return true;
	}

	private void ChargeBattery()
	{
		if (_MyDeck == null)
		{
			return;
		}
		_chargeMode = true;
		float num = Vector3.Distance(base.transform.position, _MyDeck.transform.position);
		GameObject targetObject = base.transform.Find("battery_bar").gameObject;
		if (num > 1.8f)
		{
			if (NavMeshCtrl.GetDestination(_NavMeshAgent) != _MyDeck.transform.position)
			{
				NavMeshCtrl.SetDestination(_MyDeck.transform.position, _NavMeshAgent);
			}
			GameObjectTreat.ColorChange(targetObject, Color.red);
			return;
		}
		NavMeshCtrl.ClearDestination(_NavMeshAgent);
		ClearIgnoreGarbageLists();
		_HP += _CHARGE_BATTERY;
		BatteryView();
		ChangeBatteryDockMode(mode: true);
		if (_HP >= _FULL_BATTERY)
		{
			GameObjectTreat.ColorChange(targetObject, Color.green);
			_HP = _FULL_BATTERY;
			_chargeMode = false;
			ChangeBatteryDockMode(mode: false);
		}
	}

	private void ChangeBatteryDockMode(bool mode)
	{
		if (!(_MyDeck == null))
		{
			if (_Active_Dock == null || _Sleep_Dock == null)
			{
				_Active_Dock = _MyDeck.transform.Find("ChargeMode").gameObject;
				_Sleep_Dock = _MyDeck.transform.Find("SleepMode").gameObject;
			}
			if (mode)
			{
				_Active_Dock.SetActive(value: true);
				_Sleep_Dock.SetActive(value: false);
			}
			else
			{
				_Active_Dock.SetActive(value: false);
				_Sleep_Dock.SetActive(value: true);
			}
		}
	}

	private void DebugNavMeshAgent(NavMeshAgent NavMeshAgent)
	{
		Debug.Log("NavMeshAgent:" + NavMeshAgent.name + " hasPath:" + NavMeshAgent.hasPath + " pathStatus:" + NavMeshAgent.pathStatus.ToString() + " remainingDistance:" + NavMeshAgent.remainingDistance + " destination:" + NavMeshAgent.destination.ToString() + " pathPending:" + NavMeshAgent.pathPending + _targetGarbage.transform.position.ToString() + _targetGarbage.name);
	}

	private GameObject SetTargetGarbageIgnoreLists(GameObject targetGarbage)
	{
		_IgnoreGarbageLists.Add(targetGarbage);
		GameObjectTreat.DebugColorChange(targetGarbage, Color.black);
		targetGarbage = null;
		return targetGarbage;
	}

	private void ClearIgnoreGarbageLists()
	{
		foreach (GameObject ignoreGarbageList in _IgnoreGarbageLists)
		{
			GameObjectTreat.DebugColorChange(ignoreGarbageList, Color.yellow);
		}
		_IgnoreGarbageLists.Clear();
	}

	private void DecreaseHP()
	{
		_HP -= _DECREASE_BATTERY;
		BatteryView();
	}

	private void BatteryView()
	{
		GameObject obj = base.transform.Find("battery_bar").gameObject;
		Vector3 localScale = obj.transform.localScale;
		localScale.y = 0.5f * _HP / _FULL_BATTERY;
		obj.transform.localScale = localScale;
	}

	private bool IsPowerState()
	{
		bool num = ScoreCtrl.IsScorePositiveInt(0, "CLK");
		if (!num)
		{
			SignPowerOutageCtrl.GetOrCreateCirclePowerOutage(base.gameObject);
			return num;
		}
		SignPowerOutageCtrl.UnSignPowerOutage(base.gameObject);
		return num;
	}

	internal void StartDeleteUnitProcess()
	{
		_isDelete = true;
	}

	internal void DeleteUnitProcess()
	{
		UnitStruct unitStruct = GetComponent<Sweeper>().GetUnitStruct();
		ScoreCtrl.UpdateAndDisplayScore(unitStruct.DeleteCost, unitStruct.ScoreType);
		GameObjectTreat.DestroyAll(_MyDeck);
		GameObjectTreat.DestroyAll(base.gameObject);
	}

	private void Update()
	{
		_time += Time.deltaTime;
		if (_time > (double)(_LOOP_TIME / GameSpeedCtrl.GetGameSpeed()) && !_isDelete && IsPowerState())
		{
			_time = 0.0;
			MoveControl();
		}
	}
}

// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// EnemyLitter
using System;
using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using UnityEngine.AI;

public class EnemyLitter : MonoBehaviour
{
	private NavMeshAgent _NavMeshAgent;

	private EnemyStatus _myStatus;

	private int _child_count;

	private Vector3[] _myPaths;

	private const float LITTER_CHECK = 1f;

	private const float THROW_ANGLE = 65f;

	private const float MOVING_CHECK = 1.5f;

	private Vector3 _undefine_pos = new Vector3(-99f, -99f, -99f);

	internal static int _idx;

	private Vector3 _throw_direction = new Vector3(0f, 0f, 0f);

	private Vector3 _target_position = new Vector3(0f, 0f, 0f);

	private float _target_radius;

	private int _num_of_monitoring;

	private int _MaxLitters = 20;

	private bool _LittingMode = true;

	private bool _MovingMode = true;

	internal void CreateLitterUnit(Vector3 position)
	{
		int num = GameObjectTreat.IndexObjectByTag(base.tag);
		base.name = base.tag + num;
		_myStatus = GetEnemyStatus();
		_myStatus.SetEnemyName(base.name);
		base.transform.position = position;
	}

	private EnemyStatus GetEnemyStatus()
	{
		if (_myStatus == null)
		{
			_myStatus = new EnemyStatus();
		}
		return _myStatus;
	}

	internal void SetThrowOutDirection(GameObject targetObj = null)
	{
		if (targetObj == null)
		{
			_target_position = new Vector3(0f, 0f, 0f);
			_target_radius = 0f;
			ChangeHeadColor(-1);
		}
		else
		{
			_target_position = targetObj.transform.position;
			_target_radius = targetObj.GetComponent<DustBoxCtrl>().GetRadius();
			ChangeHeadColor(1);
		}
	}

	internal void ChangeHeadColor(int monit)
	{
		_num_of_monitoring += monit;
		Renderer component = base.transform.Find("CapsuleHead").GetComponent<Renderer>();
		if (_num_of_monitoring > 0)
		{
			component.material = MaterialManager.Material_BG_Green;
		}
		else
		{
			component.material = MaterialManager.Material_BG_RED;
		}
	}

	private Vector3 GetThrowOutDirection()
	{
		if (_target_position != new Vector3(0f, 0f, 0f))
		{
			return (_target_position - base.transform.position).normalized;
		}
		return base.transform.forward;
	}

	private float GetTargetDistance()
	{
		float result = 0f;
		if (_target_position != new Vector3(0f, 0f, 0f))
		{
			result = Vector3.Distance(base.transform.position, _target_position);
		}
		return result;
	}

	private float GetThrowOutSpeed()
	{
		if (_target_position != new Vector3(0f, 0f, 0f))
		{
			float targetDistance = GetTargetDistance();
			float num = MathF.PI * 13f / 36f;
			float num2 = 9.8f;
			return Mathf.Sqrt(targetDistance * num2 / Mathf.Sin(2f * num));
		}
		return 4.43f;
	}

	private bool MakeChildLitter()
	{
		if (_child_count >= _MaxLitters)
		{
			_child_count = 0;
			_LittingMode = false;
			return false;
		}
		float targetDistance = GetTargetDistance();
		if ((targetDistance > _target_radius || targetDistance == 0f) && _num_of_monitoring > 0)
		{
			return false;
		}
		GameObject gameObject = GarbageCubeCtrl.SpawnGarbageCube(base.transform.Find("Hand").position, 1);
		if (gameObject == null)
		{
			return false;
		}
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		float throwOutSpeed = GetThrowOutSpeed();
		Vector3 throwOutDirection = GetThrowOutDirection();
		Vector3 linearVelocity = throwOutDirection * throwOutSpeed;
		linearVelocity.y = throwOutSpeed * Mathf.Sin(MathF.PI * 13f / 36f);
		linearVelocity.x = throwOutSpeed * Mathf.Cos(MathF.PI * 13f / 36f) * throwOutDirection.x;
		linearVelocity.z = throwOutSpeed * Mathf.Cos(MathF.PI * 13f / 36f) * throwOutDirection.z;
		component.linearVelocity = linearVelocity;
		_child_count++;
		return true;
	}

	private Vector3 GetRandomSize(float min, float max)
	{
		return new Vector3(rdNum(min, max), rdNum(min, max), rdNum(min, max));
	}

	private IEnumerator LitterDrops()
	{
		while (_LittingMode)
		{
			yield return new WaitForSeconds(1f);
			MakeChildLitter();
		}
	}

	private IEnumerator MoveAgent()
	{
		while (_MovingMode)
		{
			yield return new WaitForSeconds(1.5f);
			if (NavMeshCtrl.HasReachedDestination(_NavMeshAgent) && !SetNextPath(_myPaths))
			{
				_MovingMode = false;
			}
		}
	}

	private void AgentWasGoal()
	{
		GameObjectTreat.DestroyAll(base.gameObject);
	}

	private void AgentJumpToStartPosition()
	{
		base.gameObject.transform.position = _myPaths[0];
	}

	private bool SetNextPath(Vector3[] paths)
	{
		if (paths == null || paths.Length == 0)
		{
			AgentWasGoal();
			return false;
		}
		Vector3 destination = paths[0];
		SetDestination(destination);
		List<Vector3> list = new List<Vector3>(paths);
		list.RemoveAt(0);
		_myPaths = list.ToArray();
		return true;
	}

	internal void ReachDustBox(Vector3 dustBoxPos)
	{
		SetNextPath(_myPaths);
	}

	internal void AddPathAndInterrupt(Vector3 path)
	{
		List<Vector3> list = new List<Vector3>(_myPaths);
		if ((list.Count <= 0 || !(list[0] == path)) && !NavMeshCtrl.IsSameDestination(_NavMeshAgent, path))
		{
			list.Insert(0, path);
			_myPaths = list.ToArray();
			Vector3 vector = path;
			Debug.Log("AddPathAndInterrupt:" + vector.ToString());
			SetNextPath(_myPaths);
		}
	}

	internal void SetPaths(string[] marker_names = null)
	{
		Vector3[] myPaths = _myPaths;
		if (marker_names == null || marker_names.Length == 0)
		{
			myPaths = new Vector3[4]
			{
				GetMarkerPositionByName("path_marker_start"),
				GetMarkerPositionByName("path_marker_01"),
				GetMarkerPositionByName("path_marker_02"),
				GetMarkerPositionByName("path_marker_goal")
			};
		}
		else
		{
			myPaths = new Vector3[marker_names.Length];
			int num = 0;
			foreach (string text in marker_names)
			{
				Vector3 markerPositionByName = GetMarkerPositionByName(text.Trim());
				if (markerPositionByName == _undefine_pos)
				{
					Array.Resize(ref myPaths, myPaths.Length - 1);
					continue;
				}
				myPaths[num] = markerPositionByName;
				num++;
			}
		}
		_myPaths = myPaths;
	}

	private void SetDestination(Vector3 destination)
	{
		NavMeshCtrl.SetAgentSpeed(_NavMeshAgent);
		if (!NavMeshCtrl.IsSameDestination(_NavMeshAgent, destination) && !NavMeshCtrl.SetNavMeshDestination(_NavMeshAgent, destination, base.transform))
		{
			Vector3 vector = destination;
			Debug.Log("SetNavMeshDestination false:" + vector.ToString());
		}
	}

	private Vector3 GetMarkerPositionByName(string markerName)
	{
		GameObject gameObject = GameObject.Find(markerName);
		if (gameObject == null)
		{
			Debug.Log("GetMarkerPositionByName cannot find :" + markerName);
			return _undefine_pos;
		}
		return gameObject.transform.position;
	}

	private static float rdNum(float min, float max)
	{
		return Utility.fRandomRange(min, max);
	}

	private void Awake()
	{
		_NavMeshAgent = NavMeshCtrl.GetNavMeshAgent(base.gameObject);
		_idx++;
	}

	internal void InitUnitSpawn(string[] marker_names = null)
	{
		SetPaths(marker_names);
		AgentJumpToStartPosition();
		SetNextPath(_myPaths);
		StartCoroutine(LitterDrops());
		StartCoroutine(MoveAgent());
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
	}
}

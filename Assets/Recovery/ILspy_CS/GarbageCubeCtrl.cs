// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// GarbageCubeCtrl
using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;

public class GarbageCubeCtrl : MonoBehaviour
{
	private struct SpawnRequest
	{
		public Vector3 Position;

		public int SizeFlag;

		public bool IsSwayingPoint;

		public SpawnRequest(Vector3 pos, int size, bool sway)
		{
			Position = pos;
			SizeFlag = size;
			IsSwayingPoint = sway;
		}
	}

	private const int _MAX_CREATE_PER_FRAME = 200;

	internal const int _SIZE_NORMAL = 0;

	internal const int _SIZE_SMALL = 1;

	internal const int _SIZE_BIG = 2;

	private static GameObject _parent_holder;

	private const string _GARBAGE_CUBE_PARENT_NAME = "GarbageCubes";

	private const float _SPREAD_RADIUS = 2f;

	private const float _GARBAGE_CUBE_SIZE = 0.3f;

	private const float _GARBAGE_CUBE_SIZE_SMALL = 0.08f;

	private const float _GARBAGE_CUBE_SIZE_BIG_MIN = 1.5f;

	internal const float _GARBAGE_CUBE_SIZE_BIG_MAX = 3f;

	private Queue<SpawnRequest> _spawnQueue = new Queue<SpawnRequest>();

	private bool _isProcessingQueue;

	private static Vector3 GetLocalScale(int sizeFlag)
	{
		Vector3 zero = Vector3.zero;
		return sizeFlag switch
		{
			1 => new Vector3(Utility.fRandomRange(0.08f, 0.3f), Utility.fRandomRange(0.08f, 0.3f), Utility.fRandomRange(0.08f, 0.3f)), 
			2 => new Vector3(Utility.fRandomRange(1.5f, 3f), Utility.fRandomRange(1.5f, 3f), Utility.fRandomRange(1.5f, 3f)), 
			_ => new Vector3(0.3f, 0.3f, 0.3f), 
		};
	}

	internal void SpawnGarbageCubeAsync(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
	{
		_spawnQueue.Enqueue(new SpawnRequest(spawnPoint, sizeFlag, isSwayingPoint));
		if (!_isProcessingQueue)
		{
			StartCoroutine(ProcessSpawnQueue());
		}
	}

	private IEnumerator ProcessSpawnQueue()
	{
		if (_isProcessingQueue)
		{
			yield break;
		}
		_isProcessingQueue = true;
		WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		while (_spawnQueue.Count > 0)
		{
			for (int i = 0; i < 200; i++)
			{
				if (_spawnQueue.Count <= 0)
				{
					break;
				}
				SpawnRequest spawnRequest = _spawnQueue.Dequeue();
				SpawnGarbageCube(spawnRequest.Position, spawnRequest.SizeFlag, spawnRequest.IsSwayingPoint);
			}
			yield return waitForEndOfFrame;
		}
		_isProcessingQueue = false;
	}

	private IEnumerator SpawnGarbageCubeCoroutine(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
	{
		SpawnGarbageCube(spawnPoint, sizeFlag, isSwayingPoint);
		yield return null;
	}

	internal static GameObject SpawnGarbageCube(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
	{
		GameObject garbageCubePrefab = PrefabManager.GarbageCubePrefab;
		garbageCubePrefab.transform.localScale = GetLocalScale(sizeFlag);
		Vector3 position = spawnPoint;
		Quaternion rotation = Quaternion.identity;
		if (isSwayingPoint)
		{
			position.x += Utility.fRandomRange(-2f, 2f);
			position.z += Utility.fRandomRange(-2f, 2f);
			rotation = Quaternion.Euler(Utility.fRandomRange(0, 360), Utility.fRandomRange(0, 360), Utility.fRandomRange(0, 360));
		}
		GameObject obj = Object.Instantiate(garbageCubePrefab, position, rotation);
		SetGarbageCubeProperties(obj);
		SetGarbageCubeRb(obj);
		return obj;
	}

	private static void SetGarbageCubeProperties(GameObject unit)
	{
		unit.tag = GameEnum.TagType.Garbage.ToString();
		int garbageCubeUID = PrefabManager.GarbageCubeUID;
		unit.name = GameEnum.ModelsType.GarbageCube.ToString() + garbageCubeUID;
		GarbageCube garbageCube = unit.GetComponent<GarbageCube>();
		if (garbageCube == null)
		{
			garbageCube = unit.AddComponent<GarbageCube>();
		}
		garbageCube._item_struct.ItemID = unit.name;
		garbageCube._unit_struct.UnitID = unit.name;
		Transform holderParentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_holder, "GarbageCubes");
		unit.transform.SetParent(holderParentTransform);
	}

	private static void SetGarbageCubeRb(GameObject unit)
	{
		Rigidbody rigidbody = unit.GetComponent<Rigidbody>();
		if (rigidbody == null)
		{
			rigidbody = unit.AddComponent<Rigidbody>();
		}
		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		if (unit.GetComponent<Collider>() == null)
		{
			unit.AddComponent<BoxCollider>();
		}
	}

	private void Awake()
	{
		StartCoroutine(ProcessSpawnQueue());
	}
}

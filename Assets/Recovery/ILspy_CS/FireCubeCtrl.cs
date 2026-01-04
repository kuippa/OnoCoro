// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// FireCubeCtrl
using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;

public class FireCubeCtrl : MonoBehaviour
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

	internal const int _SIZE_NORMAL = 0;

	internal const int _SIZE_MEDIUM = 1;

	internal const int _SIZE_BIG = 2;

	private const int _MAX_CREATE_PER_FRAME = 20;

	internal const string _SIZE_NORMAL_NAME = "VFX_Fire_01_Small_Smoke";

	internal const string _SIZE_MEDIUM_NAME = "VFX_Fire_01_Medium_Smoke";

	internal const string _SIZE_BIG_NAME = "VFX_Fire_01_Big_Smoke";

	private const float _SPREAD_RADIUS = 1f;

	private static GameObject _parent_holder;

	private const string _FIRE_CUBE_PARENT_NAME = "FireCubes";

	private Queue<SpawnRequest> _spawnQueue = new Queue<SpawnRequest>();

	private bool _isProcessingQueue;

	internal static void ChangeFireCubeSize(GameObject parentObject, int sizeFlag)
	{
		string text = "";
		text = sizeFlag switch
		{
			0 => "VFX_Fire_01_Small_Smoke", 
			1 => "VFX_Fire_01_Medium_Smoke", 
			2 => "VFX_Fire_01_Big_Smoke", 
			_ => "VFX_Fire_01_Small_Smoke", 
		};
		Transform[] componentsInChildren = parentObject.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "VFX_Fire_01_Small_Smoke" || transform.name == "VFX_Fire_01_Medium_Smoke" || transform.name == "VFX_Fire_01_Big_Smoke")
			{
				if (transform.name == text)
				{
					transform.gameObject.SetActive(value: true);
				}
				else
				{
					transform.gameObject.SetActive(value: false);
				}
			}
		}
		BoxCollider component = parentObject.GetComponent<BoxCollider>();
		if (component != null)
		{
			float num = 1.5f + Mathf.Pow(2f, sizeFlag);
			component.size = new Vector3(num, num, num);
		}
	}

	internal static GameObject SpawnFireCube(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
	{
		GameObject fireCubePrefab = PrefabManager.FireCubePrefab;
		fireCubePrefab.transform.localScale = new Vector3(1f, 1f, 1f);
		Vector3 position = spawnPoint;
		Quaternion identity = Quaternion.identity;
		if (isSwayingPoint)
		{
			position.x += Utility.fRandomRange(-1f, 1f);
			position.z += Utility.fRandomRange(-1f, 1f);
			identity = Quaternion.identity;
		}
		GameObject obj = Object.Instantiate(fireCubePrefab, position, identity);
		SetFireCubeProperties(obj);
		SetFireCubeRb(obj);
		ChangeFireCubeSize(obj, sizeFlag);
		return obj;
	}

	internal void SpawnFireCubeAsync(Vector3 spawnPoint = default(Vector3), int sizeFlag = 0, bool isSwayingPoint = false)
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
			for (int i = 0; i < 20; i++)
			{
				if (_spawnQueue.Count <= 0)
				{
					break;
				}
				SpawnRequest spawnRequest = _spawnQueue.Dequeue();
				SpawnFireCube(spawnRequest.Position, spawnRequest.SizeFlag, spawnRequest.IsSwayingPoint);
			}
			yield return waitForEndOfFrame;
		}
		_isProcessingQueue = false;
	}

	private static void SetFireCubeProperties(GameObject unit)
	{
		unit.tag = GameEnum.TagType.FireCube.ToString();
		int fireCubeUID = PrefabManager.FireCubeUID;
		unit.name = GameEnum.ModelsType.FireCube.ToString() + fireCubeUID;
		FireCube fireCube = unit.GetComponent<FireCube>();
		if (fireCube == null)
		{
			fireCube = unit.AddComponent<FireCube>();
		}
		fireCube._item_struct.ItemID = unit.name;
		fireCube._unit_struct.UnitID = unit.name;
		Transform holderParentTransform = GameObjectTreat.GetHolderParentTransform(ref _parent_holder, "FireCubes");
		unit.transform.SetParent(holderParentTransform);
	}

	private static void SetFireCubeRb(GameObject unit)
	{
		Rigidbody rigidbody = unit.GetComponent<Rigidbody>();
		if (rigidbody == null)
		{
			rigidbody = unit.AddComponent<Rigidbody>();
		}
		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rigidbody.useGravity = true;
		rigidbody.constraints = RigidbodyConstraints.FreezeRotationY;
	}
}

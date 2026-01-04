// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// NarakuCtrl
using CommonsUtility;
using StarterAssets;
using UnityEngine;

public class NarakuCtrl : MonoBehaviour
{
	private const float _POPUP_PLAYER_DISTANCE = 30f;

	private const float _NARAKU_DISTANCE = 50f;

	private static readonly Vector2 _NARAKU_BASIC_SIZE = new Vector2(1500f, 15f);

	private Vector3 _dem_center_pos = Vector3.zero;

	private GameObject _dem;

	private WaterSurfaceCtrl _waterSurface;

	private GameObject _eventSystem;

	internal void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == GameEnum.TagType.Player.ToString())
		{
			Vector3 closestPointOnBounds = GetClosestPointOnBounds(other);
			string text = other.gameObject.name;
			Vector3 vector = closestPointOnBounds;
			Debug.Log("OnTriggerEnter Player " + text + " " + vector.ToString());
			closestPointOnBounds.y += 30f;
			other.gameObject.GetComponent<InputController>().CharacterMoveToPosition(closestPointOnBounds);
		}
		else if (!(other.gameObject.tag == GameEnum.TagType.Naraku.ToString()) && !(other.gameObject.tag == GameEnum.TagType.Ground.ToString()))
		{
			if (other.gameObject.tag == GameEnum.TagType.FireCube.ToString() || other.gameObject.tag == GameEnum.TagType.Ash.ToString())
			{
				GameObjectTreat.DestroyAll(other.gameObject);
			}
			else if (other.gameObject.tag == GameEnum.TagType.RainDrop.ToString())
			{
				_eventSystem = GameObjectTreat.GetEventSystem(_eventSystem);
				_waterSurface = GameObjectTreat.GetOrAddComponent<WaterSurfaceCtrl>(_eventSystem);
				_waterSurface.RainDropIntoNaraku(other.gameObject);
				GameObjectTreat.DestroyAll(other.gameObject);
			}
			else if (other.gameObject.tag == GameEnum.TagType.Water.ToString())
			{
				GameObjectTreat.DestroyAll(other.gameObject);
			}
			else
			{
				SetRigidbodyVelocity(other);
				Vector3 closestPointOnBounds2 = GetClosestPointOnBounds(other);
				other.gameObject.transform.position = closestPointOnBounds2;
			}
		}
	}

	private void SetRigidbodyVelocity(Collider other)
	{
		Rigidbody component = other.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.linearVelocity = Vector3.zero;
			component.angularVelocity = Vector3.zero;
			if (GameConfig._APP_GAME_MODE == "debug")
			{
				component.linearDamping = 2f;
			}
		}
	}

	private Vector3 GetClosestPointOnBounds(Collider other)
	{
		return DemCtrl.GetClosestPointOnBounds(other);
	}

	private void InitWindow()
	{
		GameObject obj = base.gameObject;
		if (obj == null)
		{
			Debug.Log("InitWindow naraku is null");
		}
		_dem = DemCtrl.GetDemObject();
		ChangeMaterialUVToPlanar(_dem);
		RectTransform component = obj.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		component.sizeDelta = _NARAKU_BASIC_SIZE;
		base.transform.parent.position = _dem_center_pos;
		int currentNarakuIndex = GetCurrentNarakuIndex();
		float demHeight = DemCtrl.GetDemHeight(_dem);
		vector = new Vector3(_dem_center_pos.x, _dem_center_pos.y - demHeight - 50f * (float)currentNarakuIndex, _dem_center_pos.z);
		component.anchoredPosition = vector;
	}

	private void ChangeMaterialUVToPlanar(GameObject targetObject)
	{
		if (targetObject == null)
		{
			Debug.Log("ChangeMaterialUVToPlanar targetObject is null");
			return;
		}
		Renderer component = targetObject.GetComponent<Renderer>();
		if (component == null)
		{
			Debug.Log("No Renderer component found on " + targetObject.name);
			return;
		}
		Material material = component.material;
		if (material == null)
		{
			Debug.Log("No material found on " + targetObject.name);
			return;
		}
		float num = 4f;
		if (material.GetFloat("_UVBase") != num)
		{
			material.SetFloat("_UVBase", 4f);
		}
	}

	private void SetDemUV(GameObject dem)
	{
		_ = _dem == null;
	}

	private int GetCurrentNarakuIndex()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Naraku.ToString());
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == base.gameObject)
			{
				base.gameObject.name = "Naraku_" + i;
				return i + 1;
			}
		}
		return 1;
	}

	private void Awake()
	{
		InitWindow();
	}

	private void Update()
	{
	}
}

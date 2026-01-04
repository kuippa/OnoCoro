// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PlateauCubeMaker
using System;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;

public class PlateauCubeMaker : MonoBehaviour
{
	private const float _CENTER_Y_OFFSET = 5f;

	private const float _BUFFER_Y_OFFSET = 0.5f;

	private const int _MAX_GARBAGE = 200;

	private const int _BURNING_BOOST = 10;

	private static int GetAngleSpacing(int rebuildCost)
	{
		int num = 1;
		if (rebuildCost < 100)
		{
			return 93;
		}
		if (rebuildCost < 200)
		{
			return 43;
		}
		return 23;
	}

	internal void BreakUpBuildingCube(GameObject targetObj, int rebuildCost)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 1;
		Renderer component = targetObj.GetComponent<Renderer>();
		Vector3 center = component.bounds.center;
		Vector3 extents = component.bounds.extents;
		rebuildCost *= 10;
		num3 = GetAngleSpacing(rebuildCost);
		while (num < rebuildCost)
		{
			num += CreateGarbageRoundByAngle(center, extents, num3, num2);
			num2++;
			if (num2 > 200)
			{
				break;
			}
		}
	}

	private int CreateGarbageRoundByAngle(Vector3 center, Vector3 extents, int step, int i)
	{
		float radius = GetRadius(extents);
		float num = (float)step * (MathF.PI / 180f);
		float num2 = radius * Mathf.Cos(num * (float)i);
		float num3 = radius * Mathf.Sin(num * (float)i);
		float num4 = Mathf.PerlinNoise(num2 * 0.1f, num3 * 0.1f) * 0.5f;
		num2 += num4;
		num3 += num4;
		Vector3 pos = new Vector3(center.x + num2, center.y + 0.5f, center.z + num3);
		return CreateGarbageCubeSmall(pos);
	}

	private int CreateGarbageCubeNormal(Vector3 pos)
	{
		int num = 0;
		Collider component = GarbageCubeCtrl.SpawnGarbageCube(pos).GetComponent<Collider>();
		if (component != null)
		{
			num += ScoreCtrl.GetTotalGarbageScore(component);
		}
		return num;
	}

	private int CreateGarbageCubeSmall(Vector3 pos)
	{
		GameObject gameManagerObject = GameObjectTreat.GetGameManagerObject();
		GarbageCubeCtrl garbageCubeCtrl = gameManagerObject.GetComponent<GarbageCubeCtrl>();
		if (garbageCubeCtrl == null)
		{
			garbageCubeCtrl = gameManagerObject.AddComponent<GarbageCubeCtrl>();
		}
		garbageCubeCtrl.SpawnGarbageCubeAsync(pos, 1, isSwayingPoint: true);
		return GarbageCube.GetBaseScore();
	}

	internal void DispCubeMarker(GameObject gameObject, Dictionary<string, string> dictInfo)
	{
		float mesuredHeight = GetMesuredHeight(dictInfo);
		GetMeshrenderInfo(gameObject, mesuredHeight);
	}

	private float GetMesuredHeight(Dictionary<string, string> dictInfo)
	{
		float result = 0f;
		if (dictInfo.ContainsKey("bldg:measuredheight"))
		{
			result = float.Parse(dictInfo["bldg:measuredheight"]);
		}
		return result;
	}

	private void GetMeshrenderInfo(GameObject targetObj, float height = 5f)
	{
		if (targetObj.GetComponent<MeshFilter>() != null)
		{
			Renderer component = targetObj.GetComponent<Renderer>();
			SetCubeMark(component, height);
			CreateCubeRoundByArc(component.bounds.center, component.bounds.extents, 4);
		}
		else
		{
			Debug.Log("MeshFilter not found" + targetObj.name);
		}
	}

	private void SetCubeMark(Renderer renderer, float height)
	{
		Vector3 center = renderer.bounds.center;
		SetCubeAtCenter(center, Color.blue, height);
		SetCubeAtCorner(renderer);
	}

	private void SetCubeAtCorner(Renderer renderer)
	{
		Vector3 center = renderer.bounds.center;
		Vector3 extents = renderer.bounds.extents;
		extents = center + extents;
		extents.y = center.y;
		Vector3 extents2 = renderer.bounds.extents;
		extents2 = center - extents2;
		extents2.y = center.y;
		Vector3 extents3 = renderer.bounds.extents;
		extents3.x = center.x + extents3.x;
		extents3.z = center.z - extents3.z;
		extents3.y = center.y;
		Vector3 extents4 = renderer.bounds.extents;
		extents4.x = center.x - extents4.x;
		extents4.z = center.z + extents4.z;
		extents4.y = center.y;
		SetCube(extents, Color.black);
		SetCube(extents2, Color.cyan);
		SetCube(extents3, Color.magenta);
		SetCube(extents4, Color.yellow);
	}

	private void SetCubeAtCenter(Vector3 center, Color color, float height)
	{
		center.y += height * 0.5f + 0.5f;
		SetCube(center, color);
	}

	private float GetRadius(Vector3 extents)
	{
		float x = extents.x;
		float z = extents.z;
		return Mathf.Sqrt(x * x + z * z);
	}

	private void CreateCubeRoundByArc(Vector3 center, Vector3 extents, int interval)
	{
		float radius = GetRadius(extents);
		int step = Mathf.FloorToInt(MathF.PI * 2f * radius / (float)interval);
		CreateCubeRoundByAngle(center, extents, step);
	}

	private void CreateCubeRoundByAngle(Vector3 center, Vector3 extents, int step)
	{
		float radius = GetRadius(extents);
		float num = 360f / (float)step * (MathF.PI / 180f);
		for (int i = 0; i < step; i++)
		{
			float num2 = radius * Mathf.Cos(num * (float)i);
			float num3 = radius * Mathf.Sin(num * (float)i);
			Vector3 setPosition = new Vector3(center.x + num2, center.y + 0.5f, center.z + num3);
			SetCube(setPosition, Color.white);
		}
	}

	private void SetMaterialColor(GameObject targetObj, Color color)
	{
		Renderer component = targetObj.GetComponent<Renderer>();
		if (!(component == null) && component.materials.Length <= 1)
		{
			component.material.color = color;
		}
	}

	private void SetCube(Vector3 setPosition, Color color)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.transform.position = setPosition;
		SetMaterialColor(gameObject, color);
		gameObject.AddComponent<Rigidbody>();
		gameObject.GetComponent<Rigidbody>().useGravity = true;
		gameObject.tag = GameEnum.TagType.Garbage.ToString();
	}
}

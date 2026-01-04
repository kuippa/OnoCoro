// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// CommonsUtility.GameObjectTreat
using CommonsUtility;
using UnityEngine;

internal class GameObjectTreat : MonoBehaviour
{
	public static GameObject[] FindObjectsByTag(string tag)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag(tag);
		if (array.Length != 0)
		{
			return array;
		}
		return null;
	}

	internal static int IndexObjectByTag(string tag)
	{
		GameObject[] array = FindObjectsByTag(tag);
		if (array != null && array.Length != 0)
		{
			return array.Length;
		}
		return 0;
	}

	internal static void DestroyAll(GameObject target)
	{
		DestroyChild(target);
		DestroyEx(target);
	}

	private static void DestroyEx(GameObject target)
	{
		if (!(target == null))
		{
			Object.Destroy(target);
			target = null;
		}
	}

	private static void DestroyChild(GameObject target)
	{
		if (target == null)
		{
			return;
		}
		Transform componentInChildren = target.GetComponentInChildren<Transform>();
		if (componentInChildren.childCount == 0)
		{
			return;
		}
		foreach (Transform item in componentInChildren)
		{
			DestroyAll(item.gameObject);
		}
	}

	internal static void DestroyMaterialsAll(Material[] materials)
	{
		if (materials != null)
		{
			for (int i = 0; i < materials.Length; i++)
			{
				Object.Destroy(materials[i]);
			}
		}
	}

	internal static void ColorChange(GameObject targetObject, Color setColor)
	{
		if (!(targetObject == null))
		{
			Renderer component = targetObject.GetComponent<Renderer>();
			if (!(component == null))
			{
				component.material.color = setColor;
			}
		}
	}

	private static Color GetColor(GameObject targetObject)
	{
		if (targetObject == null)
		{
			return Color.black;
		}
		Renderer component = targetObject.GetComponent<Renderer>();
		if (component == null)
		{
			return Color.black;
		}
		if (component.material == null)
		{
			return Color.black;
		}
		_ = component.material.color;
		return component.material.color;
	}

	internal static void DebugColorChange(GameObject targetObject, Color setColor)
	{
		if (!(GameConfig._APP_GAME_MODE != "debug") && !(GetColor(targetObject) == Color.black))
		{
			ColorChange(targetObject, setColor);
		}
	}

	internal static void DebugScriptList()
	{
	}

	internal static GameObject GetGameManagerObject()
	{
		GameObject gameObject = GameObject.Find("GameManager");
		if (gameObject == null)
		{
			gameObject = new GameObject("GameManager");
		}
		return gameObject;
	}

	internal static GameObject GetEventSystem(GameObject gameObject = null)
	{
		if (gameObject != null)
		{
			return gameObject;
		}
		GameObject gameObject2 = GameObject.Find("EventSystem");
		if (gameObject2 == null)
		{
			gameObject2 = new GameObject("EventSystem");
		}
		return gameObject2;
	}

	internal static string GetGameObjectPath(GameObject obj)
	{
		string text = obj.name;
		Transform parent = obj.transform.parent;
		while (parent != null)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}

	internal static string GetAppVersion()
	{
		return Application.version;
	}

	internal static string GetAppBuildDate()
	{
		return Utility.GetAppVersion();
	}

	internal static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
	{
		T val = gameObject.GetComponent<T>();
		if (val == null)
		{
			val = gameObject.AddComponent<T>();
		}
		return val;
	}

	internal static Transform GetHolderParentTransform(ref GameObject parent, string parentName)
	{
		string tag_name = GameEnum.TagType.Holder.ToString();
		return GetParentTransform(ref parent, parentName, tag_name);
	}

	internal static Transform GetParentTransform(ref GameObject parent, string parentName, string tag_name = "")
	{
		if (parent != null)
		{
			return parent.transform;
		}
		parent = GameObject.Find(parentName);
		if (parent == null)
		{
			parent = new GameObject(parentName);
		}
		if (tag_name != "")
		{
			parent.tag = tag_name;
		}
		return parent.transform;
	}

	internal static GameObject GetOrNewGameObject(GameObject gameObject, string objName)
	{
		if (gameObject != null)
		{
			return gameObject;
		}
		gameObject = GameObject.Find(objName);
		if (gameObject == null)
		{
			gameObject = new GameObject(objName);
		}
		return gameObject;
	}
}

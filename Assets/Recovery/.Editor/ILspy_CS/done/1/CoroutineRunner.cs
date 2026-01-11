// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// CoroutineRunner
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
	private static CoroutineRunner _instance;

	public static CoroutineRunner Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject obj = new GameObject("CoroutineRunner");
				_instance = obj.AddComponent<CoroutineRunner>();
				Object.DontDestroyOnLoad(obj);
			}
			return _instance;
		}
	}
}

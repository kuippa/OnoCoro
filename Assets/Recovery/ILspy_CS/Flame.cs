// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Flame
using UnityEngine;

public class Flame : MonoBehaviour
{
	private float _time;

	private float _interval = 0.01f;

	internal void GarbageToFire()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Garbage");
		if (array.Length == 0)
		{
			return;
		}
		for (int num = array.Length - 1; num < array.Length; num--)
		{
			Debug.Log("GarbageToFire" + num);
			Rigidbody component = array[num].GetComponent<Rigidbody>();
			if (component != null)
			{
				Debug.Log("GarbageToFire" + num + " " + component.linearVelocity.ToString() + component.angularVelocity.ToString());
				SetBonFire(array[num].transform.position);
				Object.Destroy(array[num]);
				break;
			}
		}
	}

	private void SetBonFire(Vector3 setPoint)
	{
		GameObject original = Resources.Load<GameObject>("Prefabs/Bonfire");
		setPoint.y += 0.5f;
		GameObject obj = Object.Instantiate(original, setPoint, Quaternion.identity);
		obj.AddComponent<Rigidbody>();
		obj.GetComponent<Rigidbody>().useGravity = true;
	}

	private void Update()
	{
		_time += Time.deltaTime;
		if (_time > _interval)
		{
			_time = 0f;
			GarbageToFire();
		}
	}
}

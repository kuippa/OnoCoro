// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MaterialManager
using UnityEngine;

public class MaterialManager
{
	private static Material _material_BG_Green;

	private static Material _material_BG_RED;

	internal static Material Material_BG_Green
	{
		get
		{
			if (_material_BG_Green == null)
			{
				_material_BG_Green = Resources.Load<Material>("Materials/BG_Green");
			}
			return _material_BG_Green;
		}
	}

	internal static Material Material_BG_RED
	{
		get
		{
			if (_material_BG_RED == null)
			{
				_material_BG_RED = Resources.Load<Material>("Materials/BG_RED");
			}
			return _material_BG_RED;
		}
	}
}

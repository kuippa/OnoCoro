// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// GameEnum
public class GameEnum
{
	internal enum TagType
	{
		Garbage,
		PowerCube,
		EnemyLitters,
		TowerSweeper,
		TowerDock,
		Player,
		Ground,
		Naraku,
		Untagged,
		RainDrop,
		Puddle,
		StopPlate,
		FireCube,
		Ash,
		Water,
		WaterTurret,
		DustBox,
		SentryGuard,
		Holder,
		PathBloom
	}

	internal enum ModelsType
	{
		GarbageCube,
		GarbageCubeBox,
		GarbageCubeBig,
		Litter,
		Sweeper,
		PowerCube,
		StopPlate,
		FireCube,
		WaterTurret,
		DustBox,
		SentryGuard
	}

	internal enum LayerType
	{
		Ground,
		AreaIgnoreRaycast
	}

	internal enum UnitType
	{
		Player
	}

	internal enum UIType
	{
		SpawnMarker
	}
}

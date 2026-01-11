// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// EnemyStatus
public class EnemyStatus
{
	internal enum EnemyStatusType
	{
		Name,
		ActivityLv,
		Hp,
		None,
		Normal,
		Poison,
		Burn,
		Freeze,
		Paralysis
	}

	internal string Name;

	internal int Hp;

	internal int Mp;

	internal int Attack;

	internal int Defense;

	internal int Speed;

	internal EnemyStatusType GetEnemyStatusType()
	{
		return EnemyStatusType.None;
	}

	internal string GetEnemyStatusTypeString()
	{
		return "None";
	}

	internal string GetEnemyName()
	{
		return Name;
	}

	internal string SetEnemyName(string name)
	{
		Name = name;
		return Name;
	}
}

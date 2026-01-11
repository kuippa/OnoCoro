// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// ItemStruct
using System;

[Serializable]
public struct ItemStruct
{
	public string Name { get; set; }

	public string ItemID { get; set; }

	public string ToolTip { get; set; }

	public string Info { get; set; }

	public int CreateCost { get; set; }

	public string CostType { get; set; }

	public float CostTime { get; set; }

	public int Stack { get; set; }

	public string ItemIconPath { get; set; }

	public string ItemImagePath { get; set; }

	public int HolderIndex { get; set; }

	public ItemStruct(string name, string ItemID, string ToolTip, string Info, int CreateCost, string CostType, float CostTime, int Stack, string ItemIconPath, string ItemImagePath, int HolderIndex)
	{
		Name = name;
		this.ItemID = ItemID;
		this.ToolTip = ToolTip;
		this.Info = Info;
		this.CreateCost = CreateCost;
		this.CostType = CostType;
		this.CostTime = CostTime;
		this.Stack = Stack;
		this.ItemIconPath = ItemIconPath;
		this.ItemImagePath = ItemImagePath;
		this.HolderIndex = HolderIndex;
	}

	public void AddStack(int stack)
	{
		Stack += stack;
	}
}

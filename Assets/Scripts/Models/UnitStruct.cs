using System.Collections;
using System.Collections.Generic;

public struct UnitStruct
{
    public string Name { get; set;}
    public string UnitID { get; set;}
    public int Lv { get; set;}
    public string Info { get; set;}
    public int UpdateCost { get; set;}
    public int DeleteCost { get; set;}

    public UnitStruct(string name, string UnitID, int Lv, string Info, int UpdateCost, int DeleteCost)
    {
        this.Name = name;
        this.UnitID = UnitID;
        this.Lv = Lv;
        this.Info = Info;
        this.UpdateCost = UpdateCost;
        this.DeleteCost = DeleteCost;
    }
}
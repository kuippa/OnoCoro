using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct UnitStruct
{
    public string Name { get; set;}
    public string UnitID { get; set;}
    public int Lv { get; set;}
    public string Info { get; set;}
    public int UpdateCost { get; set;}
    public int DeleteCost { get; set;}

    // public int HP { get; set;}
    public int BaseScore { get; set;}
    public string ScoreType { get; set;}


    public UnitStruct(string Name, string UnitID, int Lv, string Info, int UpdateCost, int DeleteCost, int BaseScore, string ScoreType)
    {
        this.Name = Name;
        this.UnitID = UnitID;
        this.Lv = Lv;
        this.Info = Info;
        this.UpdateCost = UpdateCost;
        this.DeleteCost = DeleteCost;
        // this.HP = HP;
        this.BaseScore = BaseScore;
        this.ScoreType = ScoreType;

    }
}
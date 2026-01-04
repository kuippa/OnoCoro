using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct CharacterStruct
{
    public int HP { get; set;}
    public int BaseScore { get; set;}
    public string ScoreType { get; set;}
    // public float rotateangle { get; set;}
    // public float rotetespeed { get; set;}

    public CharacterStruct(int HP, int BaseScore, string ScoreType)
    {
        this.HP = HP;
        this.BaseScore = BaseScore;
        this.ScoreType = ScoreType;
    }


}
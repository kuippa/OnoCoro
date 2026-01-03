using System.Collections;
using System.Collections.Generic;

// 不使用の予定

[System.Serializable]
public struct CharacterStruct
{
    public int HP { get; set;}
    public int BaseScore { get; set;}
    public string ScoreType { get; set;}

    public CharacterStruct(int HP, int BaseScore, string ScoreType)
    {
        this.HP = HP;
        this.BaseScore = BaseScore;
        this.ScoreType = ScoreType;
    }


}
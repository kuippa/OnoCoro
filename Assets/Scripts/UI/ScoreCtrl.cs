using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.PlayerLoop;

namespace CommonsUtility
{
    public class ScoreCtrl : MonoBehaviour
    {
        public static ScoreCtrl instance = null;
        private static int _intScore = 0;
        private static int _intScoreCLK = 0;
        private const string _SCORE_FIELD1 = "txtScore1";  // BIT
        private const string _SCORE_FIELD2 = "txtScore2";  // CLK


        // private const int _BASE_SCORE = 2000; // 基礎得点
        // private static Dictionary<string, int> _dicScore = new Dictionary<string, int>()
        // {
        //     {GameEnum.EnemyType.Garbage.ToString(), _BASE_SCORE},
        //     {GameEnum.EnemyType.EnemyLitters.ToString(), _BASE_SCORE},
        // };

        // myStatus.SetEnemyName("Litter");
        // unit.tag = "Garbage";

        private static void DisplayScore(int intScore, string score_field , string score_scale)
        {
            GameObject score_board = GameObject.Find(score_field);
            if (score_board == null)
            {
                Debug.Log("score_board is null");
                return;
            }
            string strScore = string.Format("{0:#,0}", intScore) + score_scale;
            score_board.GetComponent<TextMeshProUGUI>().SetText(strScore);
        }

        internal static void InitScore(int intScore = 0, string score_type = "BIT")
        {
            if (score_type == GlobalConst.SHORT_SCORE1_SCALE)
            {
                _intScore = intScore;
                DisplayScore(intScore, _SCORE_FIELD1, GlobalConst.SHORT_SCORE1_SCALE);
            }
            else if (score_type == GlobalConst.SHORT_SCORE2_SCALE)
            {
                _intScoreCLK = intScore;
                DisplayScore(intScore, _SCORE_FIELD2, GlobalConst.SHORT_SCORE2_SCALE);
            }
        }

        internal static bool IsScorePositiveInt(int intScore, string score_type = "BIT")
        {
            if (score_type == GlobalConst.SHORT_SCORE1_SCALE)
            {
                if (_intScore + intScore < 0)
                {
                    return false;
                }
            }
            else if (score_type == GlobalConst.SHORT_SCORE2_SCALE)
            {
                if (_intScoreCLK + intScore < 0)
                {
                    return false;
                }
            }
            return true;
        }

        internal static void CalcScore(int intScore, string score_type = "BIT")
        {
            if (score_type == GlobalConst.SHORT_SCORE1_SCALE)
            {
                _intScore += intScore;
                DisplayScore(_intScore, _SCORE_FIELD1, GlobalConst.SHORT_SCORE1_SCALE);
            }
            else if (score_type == GlobalConst.SHORT_SCORE2_SCALE)
            {
                _intScoreCLK += intScore;
                DisplayScore(_intScoreCLK, _SCORE_FIELD2, GlobalConst.SHORT_SCORE2_SCALE);
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }        
    }
}
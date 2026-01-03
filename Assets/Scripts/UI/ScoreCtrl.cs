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
        //     {GameEnum.TagType.Garbage.ToString(), _BASE_SCORE},
        //     {GameEnum.TagType.EnemyLitters.ToString(), _BASE_SCORE},
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

        internal static float CalcGarbageMagnification(Vector3 ScaleMagnification)
        {
            float ret = 1f;
            // ret = ScaleMagnification.x * ScaleMagnification.y * ScaleMagnification.z;
            ret = ScaleMagnification.x + ScaleMagnification.y + ScaleMagnification.z;
            return ret;
        }


        internal static int GetSliceGarbageScore(Collider target)
        {
            int score = 0;
            if (target.tag != GameEnum.TagType.Garbage.ToString() )
            {
                return score;
            }

            GarbageCube garbageCube = target.gameObject.GetComponent<GarbageCube>();
            UnitStruct UnitStruct =  garbageCube.GetUnitStruct();
            score = UnitStruct.BaseScore;
            return score;
        }


        internal static int GetTotalGarbageScore(Collider target)
        {
            int score = 0;
            if (target.tag != GameEnum.TagType.Garbage.ToString() )
            {
                return score;
            }

            GarbageCube garbageCube = target.gameObject.GetComponent<GarbageCube>();
            UnitStruct UnitStruct =  garbageCube.GetUnitStruct();
            int base_score = UnitStruct.BaseScore;

            float scoreMag = CalcGarbageMagnification(target.transform.localScale);
            // score = (int)(score * scoreMag);
            if (scoreMag <= GlobalConst.GARBAGE_MINIMUM_SIZE * 3)
            {
                Debug.Log("scoreMag <= GlobalConst.GARBAGE_MINIMUM_SIZE * 3");
                score = base_score;
            }
            else
            {
                // Debug.Log("scoreMag :" + scoreMag);
                int[] slice_cnts = new int[3];
                slice_cnts[0] = GetCountSliceGarbage(target.transform.localScale.x);
                slice_cnts[1] = GetCountSliceGarbage(target.transform.localScale.y);
                slice_cnts[2] = GetCountSliceGarbage(target.transform.localScale.z);
                int slice_cnt = Mathf.Max(slice_cnts);
                score = slice_cnt * base_score;
            }
            return score;
        }

        private static int GetCountSliceGarbage(float size)
        {
            float slice_size = GlobalConst.GARBAGE_BASE_SLICE_SIZE;
            size -= GlobalConst.GARBAGE_MINIMUM_SIZE;
            int slice_cnt = Mathf.CeilToInt(size/slice_size);
            slice_cnt += 1; // GARBAGE_MINIMUM_SIZE 分の最後のスライス
            return slice_cnt;
        }

        private static void ShowScoreEventLog(int intScore, string score_type)
        {
            EventLogCtrl.Instance.ShowEventLog("Score:" + intScore + score_type);
            // EventLogCtrl eventLogCtrl = GameObject.Find("UIEventLog").GetComponent<EventLogCtrl>();
            // if (eventLogCtrl != null)
            // {
            //     eventLogCtrl.ShowEventLog("Score:" + intScore + score_type);
            // }
        }

        // UpdateAndDisplayScore 
        internal static void UpdateAndDisplayScore(int intScore, string score_type = "BIT")
        {
            if (score_type == GlobalConst.SHORT_SCORE1_SCALE)
            {
                _intScore += intScore;
                DisplayScore(_intScore, _SCORE_FIELD1, GlobalConst.SHORT_SCORE1_SCALE);
                ShowScoreEventLog(intScore, score_type);
            }
            else if (score_type == GlobalConst.SHORT_SCORE2_SCALE)
            {
                _intScoreCLK += intScore;
                DisplayScore(_intScoreCLK, _SCORE_FIELD2, GlobalConst.SHORT_SCORE2_SCALE);
                ShowScoreEventLog(intScore, score_type);
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
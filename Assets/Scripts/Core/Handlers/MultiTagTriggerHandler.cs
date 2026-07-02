using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 複数のタグを監視するトリガーハンドラー（enum ベース）
    /// TriggerHandler を拡張し、複数タグに対応しています
    /// 
    /// 使用例：
    /// var handler = GetComponent&lt;MultiTagTriggerHandler&gt;();
    /// handler.SetTargetTags(GameEnum.TagType.Garbage, GameEnum.TagType.Water);
    /// </summary>
    internal abstract class MultiTagTriggerHandler : TriggerHandler
    {
        private HashSet<GameEnum.TagType> _targetTags = new HashSet<GameEnum.TagType>();

        /// <summary>複数タグ方式のため、基底の単一タグ必須チェック（誤警告）を無効化</summary>
        protected override bool RequiresSingleTargetTag => false;

        /// <summary>
        /// 監視対象タグを設定します（複数指定可）
        /// </summary>
        /// <param name="tags">監視対象のタグ enum</param>
        public void SetTargetTags(params GameEnum.TagType[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                Debug.LogWarning($"[{nameof(MultiTagTriggerHandler)}] Target tags cannot be null or empty");
                return;
            }

            _targetTags.Clear();
            foreach (GameEnum.TagType tag in tags)
            {
                _targetTags.Add(tag);
            }
        }

        /// <summary>
        /// タグを追加します
        /// </summary>
        /// <param name="tag">追加するタグ enum</param>
        public void AddTargetTag(GameEnum.TagType tag)
        {
            _targetTags.Add(tag);
        }

        /// <summary>
        /// タグを削除します
        /// </summary>
        /// <param name="tag">削除するタグ enum</param>
        public void RemoveTargetTag(GameEnum.TagType tag)
        {
            _targetTags.Remove(tag);
        }

        /// <summary>
        /// 監視対象として登録されているタグを確認
        /// </summary>
        /// <param name="tag">確認するタグ enum</param>
        /// <returns>監視対象の場合 true</returns>
        public bool IsTargetTag(GameEnum.TagType tag)
        {
            return _targetTags.Contains(tag);
        }

        /// <summary>
        /// 対象オブジェクトが進入した時に呼ばれます
        /// 複数タグのうち、進入したオブジェクトのタグを detectedTag で受け取ります
        /// </summary>
        /// <param name="other">進入したコライダー</param>
        /// <param name="detectedTag">検出されたタグ enum</param>
        protected abstract void OnTargetEnter(Collider other, GameEnum.TagType detectedTag);

        /// <summary>
        /// 対象オブジェクトが離脱した時に呼ばれます
        /// </summary>
        /// <param name="other">離脱したコライダー</param>
        /// <param name="detectedTag">検出されたタグ enum</param>
        protected abstract void OnTargetExit(Collider other, GameEnum.TagType detectedTag);

        /// <summary>
        /// TriggerHandler の OnTargetEnter をオーバーライド
        /// 複数タグをチェックして、マッチしたタグで OnTargetEnter(Collider, GameEnum.TagType) を呼び出します
        /// </summary>
        protected override void OnTargetEnter()
        {
            // MultiTagTriggerHandler では使用しない
        }

        /// <summary>
        /// TriggerHandler の OnTargetExit をオーバーライド
        /// 複数タグをチェックして、マッチしたタグで OnTargetExit(Collider, GameEnum.TagType) を呼び出します
        /// </summary>
        protected override void OnTargetExit()
        {
            // MultiTagTriggerHandler では使用しない
        }

        /// <summary>
        /// TriggerHandler の OnTriggerEnter をオーバーライド
        /// 複数タグをチェックして、マッチしたタグで OnTargetEnter を呼び出します
        /// </summary>
        protected override void OnTriggerEnter(Collider other)
        {
            if (other == null || _targetTags.Count == 0)
            {
                return;
            }

            string otherTagString = other.gameObject.tag;
            
            // 文字列タグを enum に変換
            if (System.Enum.TryParse<GameEnum.TagType>(otherTagString, out GameEnum.TagType otherTagEnum))
            {
                if (_targetTags.Contains(otherTagEnum))
                {
                    OnTargetEnter(other, otherTagEnum);
                }
            }
        }

        /// <summary>
        /// TriggerHandler の OnTriggerExit をオーバーライド
        /// 複数タグをチェックして、マッチしたタグで OnTargetExit を呼び出します
        /// </summary>
        protected override void OnTriggerExit(Collider other)
        {
            if (other == null || _targetTags.Count == 0)
            {
                return;
            }

            string otherTagString = other.gameObject.tag;
            
            // 文字列タグを enum に変換
            if (System.Enum.TryParse<GameEnum.TagType>(otherTagString, out GameEnum.TagType otherTagEnum))
            {
                if (_targetTags.Contains(otherTagEnum))
                {
                    OnTargetExit(other, otherTagEnum);
                }
            }
        }
    }
}

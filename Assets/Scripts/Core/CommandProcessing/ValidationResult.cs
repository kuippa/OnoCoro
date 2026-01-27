using System.Collections.Generic;

namespace CommonsUtility
{
    /// <summary>
    /// YAML バリデーション結果を保持
    /// </summary>
    internal class ValidationResult
    {
        public List<string> Errors { get; private set; } = new List<string>();
        public List<string> Warnings { get; private set; } = new List<string>();
        
        public bool IsValid => Errors.Count == 0;
        
        public void AddError(string message)
        {
            Errors.Add(message);
        }
        
        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }
    }
}

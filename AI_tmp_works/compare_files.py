import os
import re
from pathlib import Path

def normalize_code(content):
    """コードを正規化して意味的な比較を可能にする"""
    # using文を削除
    content = re.sub(r'(?m)^using\s+[^;]+;[\r\n]*', '', content)
    
    # ILSpyのヘッダーコメントを削除
    content = re.sub(r'// Assembly-CSharp[^\n]*\n', '', content)
    content = re.sub(r'// [A-Za-z0-9_.]+\n', '', content)
    
    # 行コメントを削除
    content = re.sub(r'//[^\r\n]*', '', content)
    
    # ブロックコメントを削除
    content = re.sub(r'/\*.*?\*/', '', content, flags=re.DOTALL)
    
    # 空行を削除
    content = re.sub(r'(?m)^\s*[\r\n]+', '', content)
    
    # 連続する空白を1つに
    content = re.sub(r'\s+', ' ', content)
    
    # base.transform を transform に統一
    content = content.replace('base.transform', 'transform')
    content = content.replace('base.gameObject', 'gameObject')
    content = content.replace('this.gameObject', 'gameObject')
    content = content.replace('this.transform', 'transform')
    
    # Object.Instantiate を Instantiate に統一
    content = content.replace('Object.Instantiate', 'Instantiate')
    content = content.replace('Object.FindFirstObjectByType', 'FindFirstObjectByType')
    content = content.replace('Object.FindObjectOfType', 'FindObjectOfType')
    content = content.replace('Object.Destroy', 'Destroy')
    content = content.replace('Object.DontDestroyOnLoad', 'DontDestroyOnLoad')
    
    # ILSpyの変数名の違いを吸収
    content = re.sub(r'\bitem\d*\b', 'item', content)  # item, item2, item3 -> item
    content = re.sub(r'\bnum\d*\b', 'num', content)    # num, num2, num3 -> num
    content = re.sub(r'\bresult\d*\b', 'result', content)
    content = re.sub(r'\bflag\d*\b', 'flag', content)
    
    # 前後の空白を削除
    content = content.strip()
    
    return content

def find_corresponding_file(ilspy_file_name, scripts_dir):
    """ILSpyファイルに対応するプロジェクトファイルを探す"""
    # 名前空間プレフィックスを除去して検索
    base_name = ilspy_file_name
    prefixes = ['CommonsUtility.', 'StarterAssets.', 'AppCamera.']
    
    for prefix in prefixes:
        if ilspy_file_name.startswith(prefix):
            base_name = ilspy_file_name[len(prefix):]
            break
    
    # scriptsディレクトリ以下を再帰的に検索
    for root, dirs, files in os.walk(scripts_dir):
        if base_name in files:
            return os.path.join(root, base_name)
    
    return None

def compare_files(ilspy_path, scripts_path):
    """2つのファイルを比較して、機能的に同じかどうかを判定"""
    try:
        with open(ilspy_path, 'r', encoding='utf-8') as f:
            ilspy_content = f.read()
        
        with open(scripts_path, 'r', encoding='utf-8') as f:
            scripts_content = f.read()
        
        normalized_ilspy = normalize_code(ilspy_content)
        normalized_scripts = normalize_code(scripts_content)
        
        return normalized_ilspy == normalized_scripts
    except Exception as e:
        print(f"Error comparing files: {e}")
        return False

def main():
    ilspy_dir = r'g:\unity\OnoCoro2026\Assets\Recovery\.Editor\ILspy_CS'
    scripts_dir = r'g:\unity\OnoCoro2026\Assets\Scripts'
    
    # 除外するファイル
    exclude_files = {'-Module-.cs', '___References.cs', 'UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs'}
    
    results = {
        'no_change': [],
        'changed': [],
        'not_found': []
    }
    
    # ILspy_CSフォルダ内のすべての.csファイルを処理
    for file_name in sorted(os.listdir(ilspy_dir)):
        if not file_name.endswith('.cs') or file_name in exclude_files:
            continue
        
        ilspy_path = os.path.join(ilspy_dir, file_name)
        
        # 対応するファイルを探す
        scripts_path = find_corresponding_file(file_name, scripts_dir)
        
        if scripts_path is None:
            results['not_found'].append(file_name)
            print(f"NotFound: {file_name}")
            continue
        
        # ファイルを比較
        if compare_files(ilspy_path, scripts_path):
            results['no_change'].append(file_name)
            print(f"NoChange: {file_name}")
        else:
            results['changed'].append(file_name)
            print(f"Changed: {file_name}")
    
    # サマリーを出力
    print("\n" + "="*60)
    print("SUMMARY")
    print("="*60)
    print(f"No Change: {len(results['no_change'])} files")
    print(f"Changed: {len(results['changed'])} files")
    print(f"Not Found: {len(results['not_found'])} files")
    
    # nochange_passに移動すべきファイルのリストを出力
    print("\n" + "="*60)
    print("FILES TO MOVE TO nochange_pass:")
    print("="*60)
    for file_name in results['no_change']:
        ilspy_path = os.path.join(ilspy_dir, file_name)
        print(ilspy_path)
    
    # 結果をファイルに保存
    output_path = r'g:\unity\OnoCoro2026\comparison_results.txt'
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write("NO CHANGE FILES:\n")
        f.write("="*60 + "\n")
        for file_name in results['no_change']:
            f.write(f"{file_name}\n")
        
        f.write("\nCHANGED FILES:\n")
        f.write("="*60 + "\n")
        for file_name in results['changed']:
            f.write(f"{file_name}\n")
        
        f.write("\nNOT FOUND FILES:\n")
        f.write("="*60 + "\n")
        for file_name in results['not_found']:
            f.write(f"{file_name}\n")
    
    print(f"\nResults saved to: {output_path}")

if __name__ == '__main__':
    main()

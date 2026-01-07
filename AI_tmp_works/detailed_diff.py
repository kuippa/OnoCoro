import os
import difflib
from pathlib import Path

def get_detailed_diff(file1_path, file2_path):
    """2つのファイルの詳細な差分を取得"""
    try:
        with open(file1_path, 'r', encoding='utf-8') as f:
            file1_lines = f.readlines()
        with open(file2_path, 'r', encoding='utf-8') as f:
            file2_lines = f.readlines()
        
        diff = difflib.unified_diff(
            file1_lines,
            file2_lines,
            fromfile=os.path.basename(file1_path),
            tofile=os.path.basename(file2_path),
            lineterm=''
        )
        
        return list(diff)
    except Exception as e:
        return [f"Error: {e}"]

def find_corresponding_file(ilspy_file_name, scripts_dir):
    """ILSpyファイルに対応するプロジェクトファイルを探す"""
    base_name = ilspy_file_name
    prefixes = ['CommonsUtility.', 'StarterAssets.', 'AppCamera.']
    
    for prefix in prefixes:
        if ilspy_file_name.startswith(prefix):
            base_name = ilspy_file_name[len(prefix):]
            break
    
    for root, dirs, files in os.walk(scripts_dir):
        if base_name in files:
            return os.path.join(root, base_name)
    
    return None

def analyze_diff(diff_lines):
    """差分を分析して、変更の性質を判定"""
    if not diff_lines:
        return "identical", []
    
    significant_changes = []
    minor_changes = []
    
    for line in diff_lines:
        if line.startswith('---') or line.startswith('+++') or line.startswith('@@'):
            continue
        
        # 変更行を分析
        if line.startswith('-') or line.startswith('+'):
            stripped = line[1:].strip()
            
            # using文の違い
            if stripped.startswith('using '):
                minor_changes.append(('using', line))
            # コメントのみの違い
            elif stripped.startswith('//'):
                minor_changes.append(('comment', line))
            # 空行
            elif not stripped:
                minor_changes.append(('whitespace', line))
            # ILSpyヘッダー
            elif 'Assembly-CSharp' in stripped or stripped.startswith('//'):
                minor_changes.append(('header', line))
            # 実質的なコードの変更
            else:
                significant_changes.append(line)
    
    if not significant_changes:
        return "minor_only", minor_changes
    elif len(significant_changes) < 10:
        return "small_change", significant_changes
    else:
        return "significant_change", significant_changes

def main():
    ilspy_dir = r'g:\unity\OnoCoro2026\Assets\Recovery\.Editor\ILspy_CS'
    scripts_dir = r'g:\unity\OnoCoro2026\Assets\Scripts'
    output_dir = r'g:\unity\OnoCoro2026\diff_analysis'
    
    os.makedirs(output_dir, exist_ok=True)
    
    exclude_files = {'-Module-.cs', '___References.cs', 'UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs'}
    
    results = {
        'identical': [],
        'minor_only': [],
        'small_change': [],
        'significant_change': [],
        'not_found': []
    }
    
    # 全ファイルを詳細分析
    count = 0
    max_files = 999
    
    for file_name in sorted(os.listdir(ilspy_dir)):
        if not file_name.endswith('.cs') or file_name in exclude_files:
            continue
        
        if count >= max_files:
            break
        
        ilspy_path = os.path.join(ilspy_dir, file_name)
        scripts_path = find_corresponding_file(file_name, scripts_dir)
        
        if scripts_path is None:
            results['not_found'].append(file_name)
            print(f"NotFound: {file_name}")
            continue
        
        # 差分を取得
        diff_lines = get_detailed_diff(ilspy_path, scripts_path)
        change_type, changes = analyze_diff(diff_lines)
        
        results[change_type].append(file_name)
        
        # 差分をファイルに保存
        diff_file = os.path.join(output_dir, f"{file_name}.diff.txt")
        with open(diff_file, 'w', encoding='utf-8') as f:
            f.write(f"Change Type: {change_type}\n")
            f.write(f"ILSpy: {ilspy_path}\n")
            f.write(f"Scripts: {scripts_path}\n")
            f.write("="*80 + "\n\n")
            f.writelines(diff_lines)
            f.write("\n\n" + "="*80 + "\n")
            f.write(f"Significant Changes: {len(changes)}\n")
            for change in changes[:20]:  # 最初の20行のみ
                f.write(change + "\n")
        
        print(f"{change_type}: {file_name}")
        count += 1
    
    # サマリーを出力
    print("\n" + "="*60)
    print("DETAILED ANALYSIS SUMMARY")
    print("="*60)
    print(f"Identical: {len(results['identical'])} files")
    print(f"Minor Only (comments/whitespace): {len(results['minor_only'])} files")
    print(f"Small Changes: {len(results['small_change'])} files")
    print(f"Significant Changes: {len(results['significant_change'])} files")
    print(f"Not Found: {len(results['not_found'])} files")
    
    # 結果をファイルに保存
    summary_path = os.path.join(output_dir, 'summary.txt')
    with open(summary_path, 'w', encoding='utf-8') as f:
        f.write("DETAILED ANALYSIS SUMMARY\n")
        f.write("="*60 + "\n\n")
        
        for category in ['identical', 'minor_only', 'small_change', 'significant_change', 'not_found']:
            f.write(f"\n{category.upper()}:\n")
            f.write("-"*60 + "\n")
            for file_name in results[category]:
                f.write(f"{file_name}\n")
    
    print(f"\nDiff analysis saved to: {output_dir}")
    print(f"Summary saved to: {summary_path}")

if __name__ == '__main__':
    main()

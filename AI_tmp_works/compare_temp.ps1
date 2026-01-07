$ilspyDir = "g:\unity\OnoCoro2026\Assets\Recovery\.Editor\ILspy_CS"
$scriptsDir = "g:\unity\OnoCoro2026\Assets\Scripts"

function Normalize-Code {
    param([string]$content)
    
    # using文を削除
    $content = $content -replace '(?m)^using\s+[^;]+;[\r\n]*', ''
    
    # コメントを削除（行コメントとブロックコメント）
    $content = $content -replace '//[^\r\n]*', ''
    $content = $content -replace '(?s)/\*.*?\*/', ''
    
    # 空行を削除
    $content = $content -replace '(?m)^\s*[\r\n]+', ''
    
    # 連続する空白を1つに
    $content = $content -replace '\s+', ' '
    
    # base.transform を transform に統一
    $content = $content -replace 'base\.transform', 'transform'
    
    # 前後の空白を削除
    $content = $content.Trim()
    
    return $content
}

$results = @{
    NoChange = @()
    Changed = @()
    NotFound = @()
}

$filesToCheck = @(
    "ClickCtrl.cs",
    "BuildingBreak.cs",
    "Burning.cs",
    "CircularIndicator.cs",
    "GameConfig.cs",
    "GameCtrl.cs",
    "LangConst.cs",
    "LangCtrl.cs",
    "ScoreCtrl.cs",
    "DebugInfoCtrl.cs",
    "DemCtrl.cs",
    "Earthquake.cs"
)

foreach ($fileName in $filesToCheck) {
    $ilspyFile = $null
    
    if ($fileName -like "*.cs") {
        $ilspyFile = Get-ChildItem -Path $ilspyDir -Filter $fileName -File -ErrorAction SilentlyContinue |
            Select-Object -First 1
        
        if (-not $ilspyFile) {
            $ilspyFile = Get-ChildItem -Path $ilspyDir -Filter "CommonsUtility.$fileName" -File -ErrorAction SilentlyContinue |
                Select-Object -First 1
        }
        
        if (-not $ilspyFile) {
            $ilspyFile = Get-ChildItem -Path $ilspyDir -Filter "StarterAssets.$fileName" -File -ErrorAction SilentlyContinue |
                Select-Object -First 1
        }
        
        if (-not $ilspyFile) {
            $ilspyFile = Get-ChildItem -Path $ilspyDir -Filter "AppCamera.$fileName" -File -ErrorAction SilentlyContinue |
                Select-Object -First 1
        }
    }
    
    if (-not $ilspyFile) {
        $results.NotFound += $fileName
        continue
    }
    
    $scriptsFile = Get-ChildItem -Path $scriptsDir -Filter $fileName -Recurse -File -ErrorAction SilentlyContinue |
        Select-Object -First 1
    
    if (-not $scriptsFile) {
        $results.NotFound += $fileName
        continue
    }
    
    $ilspyContent = Get-Content -Path $ilspyFile.FullName -Raw -Encoding UTF8
    $scriptsContent = Get-Content -Path $scriptsFile.FullName -Raw -Encoding UTF8
    
    $normalizedIlspy = Normalize-Code $ilspyContent
    $normalizedScripts = Normalize-Code $scriptsContent
    
    if ($normalizedIlspy -eq $normalizedScripts) {
        $results.NoChange += $ilspyFile.Name
        Write-Host "NoChange: $($ilspyFile.Name)" -ForegroundColor Green
    } else {
        $results.Changed += $ilspyFile.Name
        Write-Host "Changed: $($ilspyFile.Name)" -ForegroundColor Yellow
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "NoChange: $($results.NoChange.Count)" -ForegroundColor Green
Write-Host "Changed: $($results.Changed.Count)" -ForegroundColor Yellow
Write-Host "NotFound: $($results.NotFound.Count)" -ForegroundColor Red

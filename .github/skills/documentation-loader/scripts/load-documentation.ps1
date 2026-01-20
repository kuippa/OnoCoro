#Requires -Version 5.1
<#
.SYNOPSIS
Load and cache OnoCoro mandatory documentation for AI agents.

.DESCRIPTION
Automatically loads AGENTS.md, coding-standards.md, and related documentation.
Supports auto-load on session start and manual trigger via /readmd command.

.PARAMETER autoRegister
Register this script to run on session initialization (one-time setup).

.PARAMETER disableAutoLoad
Disable automatic documentation loading on session start.

.PARAMETER verbose
Display detailed loading information.

.EXAMPLE
# Enable auto-load (one-time setup)
.github/skills/documentation-loader/scripts/load-documentation.ps1 -autoRegister

# Manual load (use in Copilot Chat as /readmd)
.github/skills/documentation-loader/scripts/load-documentation.ps1

# Disable auto-load
.github/skills/documentation-loader/scripts/load-documentation.ps1 -disableAutoLoad

.NOTES
This script is part of the OnoCoro documentation-loader Skill.
Mandatory documents are defined in references/required-documents.json
#>

param(
    [switch]$autoRegister,
    [switch]$disableAutoLoad,
    [switch]$verbose
)

# Get project root
$projectRoot = (Get-Location).Path
$docsToLoad = @(
    "AGENTS.md",
    "docs/coding-standards.md",
    "docs/recovery-workflow.md",
    ".github/instructions/unity-csharp-recovery.instructions.md",
    ".github/instructions/prefab-asset-management.instructions.md",
    ".github/instructions/plateau-sdk-geospatial.instructions.md"
)

# Session context cache file
$sessionCacheDir = ".github"
$sessionCacheFile = Join-Path $sessionCacheDir ".session-context.json"

# Ensure cache directory exists
if (-not (Test-Path $sessionCacheDir)) {
    New-Item -ItemType Directory -Path $sessionCacheDir -Force | Out-Null
}

function Get-DocumentHash {
    param([string]$FilePath)
    if (Test-Path $FilePath) {
        $fileContent = Get-Content $FilePath -Raw
        $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes($fileContent))
        return ([System.Convert]::ToBase64String($hash)).Substring(0, 16)
    }
    return $null
}

function Get-LoadedDocumentation {
    param([switch]$verbose)
    
    $output = @()
    $output += "📚 **OnoCoro Documentation Context Loaded**`n"
    $output += "**Loaded Files**:`n"
    
    $loadedCount = 0
    $totalSize = 0
    
    foreach ($doc in $docsToLoad) {
        $fullPath = Join-Path $projectRoot $doc
        
        if (Test-Path $fullPath) {
            $fileInfo = Get-Item $fullPath
            $lineCount = @(Get-Content $fullPath).Count
            if ($lineCount -lt 1) { $lineCount = 1 }
            $totalSize += $fileInfo.Length
            
            # Display file info
            $output += "✅ ``$doc`` ($lineCount lines)`n"
            $loadedCount++
            
            if ($verbose) {
                $output += "   Path: $fullPath`n"
            }
        }
        else {
            $output += "⚠️ ``$doc`` (NOT FOUND)`n"
        }
    }
    
    $output += "`n**Context Summary**:`n"
    $output += "- **Recovery Phase**: Defensive programming, null checks required`n"
    $output += "- **Coding Standards**: No magic numbers, required braces, early return pattern`n"
    $output += "- **PLATEAU SDK**: CityGML processing, coordinate transformation`n"
    $output += "- **PrefabManager**: Centralized asset management`n"
    $output += "`n**Status**: ✅ Ready for development ($loadedCount documents loaded, $([Math]::Round($totalSize / 1KB, 1)) KB total)`n"
    
    return $output -join ""
}

function Update-SessionCache {
    if ($env:COPILOT_SESSION_ID) {
        $sessionIdValue = $env:COPILOT_SESSION_ID
    }
    else {
        $sessionIdValue = "manual-session"
    }
    
    $cacheData = @{
        timestamp = (Get-Date -Format "o")
        session_id = $sessionIdValue
        loaded_documents = @{}
        context_status = "ready"
    }
    
    foreach ($doc in $docsToLoad) {
        $fullPath = Join-Path $projectRoot $doc
        $cacheData.loaded_documents[$doc] = @{
            path = $doc
            exists = (Test-Path $fullPath)
            hash = Get-DocumentHash $fullPath
            loaded = (Test-Path $fullPath)
        }
    }
    
    $cacheData | ConvertTo-Json | Set-Content $sessionCacheFile -Encoding UTF8
}

function Register-AutoLoad {
    Write-Host "✅ Documentation loader registered" -ForegroundColor Green
    Write-Host "Auto-load enabled. Documentation will load on session start." -ForegroundColor Cyan
    
    # Create marker file to indicate auto-load is enabled
    $autoLoadMarker = Join-Path $sessionCacheDir ".auto-load-enabled"
    "" | Set-Content $autoLoadMarker
    
    return "Auto-load registered. Documentation will load on next session."
}

function Disable-AutoLoad {
    $autoLoadMarker = Join-Path $sessionCacheDir ".auto-load-enabled"
    
    if (Test-Path $autoLoadMarker) {
        Remove-Item $autoLoadMarker -Force
        Write-Host "⛔ Auto-load disabled" -ForegroundColor Yellow
        return "Auto-load disabled. Run with -autoRegister to re-enable."
    }
    else {
        Write-Host "ℹ️ Auto-load was not enabled" -ForegroundColor Cyan
        return "Auto-load was not enabled."
    }
}

function Test-AutoLoadRequired {
    $autoLoadMarker = Join-Path $sessionCacheDir ".auto-load-enabled"
    $sessionCacheExists = Test-Path $sessionCacheFile
    
    if ((Test-Path $autoLoadMarker) -and -not $sessionCacheExists) {
        return $true
    }
    return $false
}

# Main execution
if ($autoRegister) {
    Register-AutoLoad
}
elseif ($disableAutoLoad) {
    Disable-AutoLoad
}
elseif ((Test-AutoLoadRequired) -or $true) {
    # Load documentation and update cache
    $result = Get-LoadedDocumentation -verbose:$verbose
    Update-SessionCache
    Write-Output $result
}

# OnoCoro Required Documents

This file defines which documentation files are loaded by the **documentation-loader** Skill.

## Mandatory Documents (Always Loaded)

These documents are loaded for every session:

```json
{
  "mandatory": [
    {
      "path": "AGENTS.md",
      "description": "AI Agent guidelines and coding standards summary",
      "priority": "CRITICAL",
      "tags": ["AI", "guidelines", "recovery"]
    },
    {
      "path": "docs/coding-standards.md",
      "description": "C# implementation standards and best practices",
      "priority": "CRITICAL",
      "tags": ["coding", "C#", "standards"]
    },
    {
      "path": "docs/recovery-workflow.md",
      "description": "Recovery phase merge rules and guidelines",
      "priority": "CRITICAL",
      "tags": ["recovery", "git", "workflow"]
    }
  ]
}
```

## Conditional Documents (Loaded on Context)

These documents are loaded when specific contexts are active:

```json
{
  "conditional": [
    {
      "path": ".github/instructions/unity-csharp-recovery.instructions.md",
      "trigger": "C# code modification",
      "description": "Recovery phase C# development guidelines",
      "priority": "HIGH"
    },
    {
      "path": ".github/instructions/prefab-asset-management.instructions.md",
      "trigger": "Asset/prefab management",
      "description": "PrefabManager usage and caching patterns",
      "priority": "HIGH"
    },
    {
      "path": ".github/instructions/plateau-sdk-geospatial.instructions.md",
      "trigger": "PLATEAU SDK integration",
      "description": "PLATEAU SDK, CityGML processing, coordinate transformation",
      "priority": "HIGH"
    },
    {
      "path": "docs/architecture.md",
      "trigger": "System design or refactoring",
      "description": "System architecture and component relationships",
      "priority": "MEDIUM"
    },
    {
      "path": "docs/introduction.md",
      "trigger": "Policy or goal verification",
      "description": "Project purpose, goals, and non-goals",
      "priority": "LOW"
    }
  ]
}
```

## Documents NOT Loaded (Human-Facing)

These documents are NOT loaded by this Skill because they target human developers:

```json
{
  "excluded": [
    {
      "path": ".github/instructions.md",
      "reason": "Development operations guide (human focus)"
    },
    {
      "path": ".github/copilot/README.md",
      "reason": "Copilot setup guide (not needed at runtime)"
    }
  ]
}
```

## Adding New Required Documents

To add a new document to the mandatory load:

1. **Add to SKILL.md** - Update the "Mandatory Documents" table
2. **Update load-documentation.ps1** - Add path to `$docsToLoad` array
3. **Test** - Run `.github/skills/documentation-loader/scripts/load-documentation.ps1` and verify output

Example:

```powershell
# In load-documentation.ps1
$docsToLoad = @(
    "AGENTS.md",
    "docs/coding-standards.md",
    # ... existing items ...
    "path/to/new/document.md"  # Add new document here
)
```

## Current Load Count

- **Mandatory**: 3 documents
- **Conditional**: 3 documents
- **Total**: 6 documents (~2,000 lines, ~100 KB)

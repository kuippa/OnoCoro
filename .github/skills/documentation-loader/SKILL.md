---
name: documentation-loader
description: Automatically load and cache OnoCoro mandatory documentation (AGENTS.md, coding-standards.md, etc.) for AI agents. Supports manual triggers via /readmd command and auto-load on session start. Prevents context gaps when sessions reset or new agents are spawned.
compatibility: Works with GitHub Copilot and AI Agents
---

# Documentation Loader Skill

Automatically load and maintain OnoCoro mandatory documentation context for AI Agents and GitHub Copilot sessions.

## Purpose

The **documentation-loader** Skill solves a critical problem in AI-assisted development:

**Problem**: When Copilot Chat sessions reset, or when new AI agents are spawned, essential project guidelines (AGENTS.md, coding-standards.md, etc.) are lost from context. This causes:
- Inconsistent code quality
- Recovery phase defensive programming patterns forgotten
- PLATEAU SDK integration guidelines lost
- PrefabManager caching strategies not applied

**Solution**: This Skill:
1. **Auto-loads** mandatory documentation on session start
2. **Provides `/readmd` command** for manual re-loading
3. **Maintains session context** with a structured summary
4. **Caches document state** to detect changes

## When to Use

### Automatic Triggers

✅ **Auto-load on**:
- New Copilot Chat session
- AI Agent spawn event
- Manual `/readmd` command invocation

### Manual Triggers

✅ **Use `/readmd` when**:
- Session context is unclear
- Documentation has been updated
- Starting a new major feature
- Recovery phase code review begins
- PLATEAU SDK integration work starts

## Key Features

| Feature | Description |
|---------|-------------|
| **Mandatory Docs** | Loads AGENTS.md, coding-standards.md, recovery-workflow.md, instructions/*.md |
| **Auto-Load** | Triggers on session initialization (PowerShell script) |
| **Manual Command** | `/readmd` triggers context reload in Copilot Chat |
| **Session Cache** | Tracks loaded documents to avoid re-loading unchanged files |
| **Structured Output** | Provides concise markdown summary for easy reference |
| **AI-Focused** | Loads AI Agent guidelines (not human developer guides) |

## Setup Instructions

### Step 1: Enable Auto-Load Script (One-time)

Run this PowerShell command in the project root to set up auto-load:

```powershell
# Register the auto-load script
.github/skills/documentation-loader/scripts/load-documentation.ps1 -autoRegister

# Output: "Auto-load registered. Documentation will load on next session."
```

### Step 2: Use `/readmd` Command in Copilot Chat

In any Copilot Chat session, trigger manual load:

```
/readmd
```

**Output**:
```
📚 OnoCoro Documentation Context Loaded

Loaded Files:
✅ AGENTS.md (252 lines) - AI Agent guidelines
✅ docs/coding-standards.md (450+ lines) - C# implementation standards
✅ docs/recovery-workflow.md (300+ lines) - Recovery merge rules
✅ .github/instructions/unity-csharp-recovery.instructions.md (375 lines)
✅ .github/instructions/prefab-asset-management.instructions.md (396 lines)
✅ .github/instructions/plateau-sdk-geospatial.instructions.md (556 lines)

Context Summary:
- Recovery Phase: Defensive programming, null checks required
- Coding Standards: No magic numbers, required braces, early return pattern
- PLATEAU SDK: CityGML processing, coordinate transformation
- PrefabManager: Centralized asset management via PrefabManager

Session Status: Ready for development
```

## Document Loading Strategy

### Mandatory Documents (Always Load)

| Document | Purpose | AI Relevance |
|----------|---------|--------------|
| `AGENTS.md` | Project-wide AI rules | ⭐⭐⭐ CRITICAL |
| `docs/coding-standards.md` | C# implementation standards | ⭐⭐⭐ CRITICAL |
| `docs/recovery-workflow.md` | Recovery phase merge rules | ⭐⭐⭐ CRITICAL |

### Conditional Documents (Load on Context)

| Document | Trigger | AI Relevance |
|----------|---------|--------------|
| `.github/instructions/unity-csharp-recovery.instructions.md` | C# code work | ⭐⭐ High |
| `.github/instructions/prefab-asset-management.instructions.md` | Asset management | ⭐⭐ High |
| `.github/instructions/plateau-sdk-geospatial.instructions.md` | GIS/PLATEAU work | ⭐⭐ High |
| `docs/architecture.md` | Design/refactoring | ⭐ Medium |
| `docs/introduction.md` | Policy/goals | ⭐ Low |

### Documents NOT Loaded (Human-Facing)

- `.github/instructions.md` — Development operations guide (human focus)
- `.github/copilot/README.md` — Setup guide (not needed at runtime)

## Usage Examples

### Example 1: Auto-Load on Session Start

```powershell
# Run once to enable auto-load
.github/skills/documentation-loader/scripts/load-documentation.ps1 -autoRegister

# Now, on every new session, documentation loads automatically
```

**Output**:
```
✅ Documentation loader registered
Auto-load enabled. Run .github/skills/documentation-loader/scripts/load-documentation.ps1 on session start
```

### Example 2: Manual Trigger in Copilot Chat

```
/readmd
```

**In Response**:
> 📚 **OnoCoro Documentation Context Loaded**
> 
> Session initialized with mandatory documentation.
> - Recovery Phase guidelines active
> - Coding standards: AGENTS.md § Coding Standards
> - Asset management: PrefabManager patterns required
> 
> Ready for development. Ask questions about specific patterns or file modifications.

### Example 3: Conditional Load During Development

```
I'm starting PLATEAU SDK integration. Can you load the PLATEAU documentation?

[User Input]
/readmd

[System Response]
✅ Loaded:
- AGENTS.md (general rules)
- docs/coding-standards.md (C# standards)
- .github/instructions/plateau-sdk-geospatial.instructions.md (PLATEAU patterns)

You're ready to work on PLATEAU SDK integration. Key points:
- Coordinate transformation: WGS84 ↔ Unity
- Null safety: All GetComponent() calls must check null
- Memory optimization: Progressive loading for large datasets
```

## Implementation Details

### Auto-Load Mechanism

**File**: `scripts/load-documentation.ps1`

```powershell
# When called with -autoRegister:
# 1. Reads AGENTS.md to identify mandatory documents
# 2. Creates a session context cache file
# 3. Registers task to run on session initialization

# When called manually:
# 1. Checks if documentation has changed
# 2. Outputs concise markdown summary
# 3. Updates session cache

# Caching Strategy:
# - Track file hash of each document
# - Only re-read if hash changes
# - Store summary in .github/.session-context (gitignored)
```

### Command Integration

**In Copilot Chat**:

```
/readmd
```

Maps to:

```
"Please run: .github/skills/documentation-loader/scripts/load-documentation.ps1
and output the result as markdown summary"
```

### Session Context File

**Location**: `.github/.session-context` (gitignored)

```json
{
  "timestamp": "2026-01-20T10:30:00Z",
  "session_id": "copilot-session-xyz",
  "loaded_documents": {
    "AGENTS.md": {
      "path": "AGENTS.md",
      "hash": "abc123def456",
      "line_count": 252,
      "loaded": true
    },
    "docs/coding-standards.md": {
      "path": "docs/coding-standards.md",
      "hash": "xyz789uvw012",
      "line_count": 450,
      "loaded": true
    }
  },
  "context_status": "ready"
}
```

## Related Documentation

- [AGENTS.md](../../AGENTS.md) - Project AI guidelines
- [.github/copilot/README.md](../copilot/README.md) - Copilot customization
- [docs/coding-standards.md](../../docs/coding-standards.md) - C# standards
- [docs/recovery-workflow.md](../../docs/recovery-workflow.md) - Recovery rules

## Tips & Troubleshooting

### Q: Documentation isn't loading automatically

**A**: 
1. Check if auto-register was run:
   ```powershell
   # Re-register
   .github/skills/documentation-loader/scripts/load-documentation.ps1 -autoRegister
   ```

2. Verify script location:
   ```powershell
   Test-Path ".github/skills/documentation-loader/scripts/load-documentation.ps1"
   ```

### Q: Want to disable auto-load temporarily?

**A**:
```powershell
# Disable auto-load
.github/skills/documentation-loader/scripts/load-documentation.ps1 -disableAutoLoad

# Output: "Auto-load disabled. Run with -autoRegister to re-enable."
```

### Q: Documentation loaded but context still unclear?

**A**: Try explicit re-load:
```
/readmd
```

This forces a fresh read and cache refresh.

### Q: Can I customize which documents load?

**A**: Yes, edit `references/required-documents.json` to add/remove documents. See [required-documents.md](references/required-documents.md) for details.

## Checklist

When using this skill:

- [ ] Session started (auto-load should trigger or `/readmd` executed)
- [ ] AGENTS.md context confirmed
- [ ] Coding standards understood (AGENTS.md § Coding Standards)
- [ ] Recovery phase rules active (defensive programming)
- [ ] Ready to begin development

---

## Related Skills

- **[microsoft-docs](../microsoft-docs/SKILL.md)** — For API research
- **[microsoft-code-reference](../microsoft-code-reference/SKILL.md)** — For code validation
- **[make-skill-template](../make-skill-template/SKILL.md)** — For creating new skills

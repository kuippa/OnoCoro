# Recovery Workflow Guide

This document provides detailed guidance for merging and managing code during the project recovery process from 2-year-old backups.

---

## Overview

This project is being recovered from SSD failure backups. Merge operations must follow strict rules to maintain code quality and minimize risk during the recovery phase.

---

## Primary Rule: Functional Improvement Only

**MANDATORY**: Do NOT modify code if there's no functional improvement.

Minor refactoring (using removal, formatting, code style adjustments) without functional change = **SKIP MERGE**.

```csharp
// ❌ NG: Skip this refactoring (no functional change)
// Before: using System.Collections;
// After: (removed)

// ✅ OK: Merge only if functional improvement
// Example: Adding null checks, important bug fix, constant definition
```

**When in doubt**: Skip the merge and ask the user for guidance.

---

## Recovery-Specific Rules

### 1. Preserve Variable Initialization

**REQUIRED**: Keep explicit initialization, even if default

```csharp
// ✅ OK - Preserve initialization
public static GameTimerCtrl instance = null;
public float _time = 0.0f;

// ❌ NG - Remove initialization
public static GameTimerCtrl instance;
public float _time;
```

**Rationale**: Explicit initialization makes intent clear and aids in recovery process verification.

### 2. Use `this` for MonoBehaviour Member Access

**MANDATORY**: Always use `this` for MonoBehaviour members, never `base`

```csharp
// ✅ OK - Explicit this.gameObject reference
_text = this.gameObject.GetComponent<TextMeshProUGUI>();
Button btn = this.gameObject.transform.Find("Panel/Button").GetComponent<Button>();
this.gameObject.SetActive(false);

// ❌ NG - Never use base
_text = base.gameObject.GetComponent<TextMeshProUGUI>();

// ❌ NG - Never omit this
_text = gameObject.GetComponent<TextMeshProUGUI>();

// ❌ NG - Use this.gameObject.transform, not this.transform
Button btn = this.transform.Find("Panel/Button").GetComponent<Button>();
```

**Rationale**: 
- `this` clarifies that we're accessing instance members
- `this.gameObject.transform` shows the explicit path through GameObject
- Improves clarity during code review and recovery verification

### 3. Preserve All Comments

**REQUIRED**: Do not delete existing comments (including commented-out code)

**Rationale**: Comments and commented-out code may contain important context from the original development, especially during recovery.

### 4. Merge Decision Matrix

**SKIP merge if**:
- Code structure is essentially identical
- Diff is minor refactoring with minimal benefit
- Existing code works well and change carries risk
- Changes are only for code style/formatting

**ALWAYS merge if**:
- New methods or features are added
- Important bug fixes (null checks, logic corrections)
- Performance improvements with measurable impact
- TODO implementations
- Critical safety improvements

**When uncertain**: Contact the user before proceeding.

---

## Recovery Phase Merge Checklist

Before merging code from recovery sources:

- [ ] **Functional improvement confirmed** - Change provides measurable functional benefit
- [ ] **Initialization preserved** - Default initializations are kept
- [ ] **this usage correct** - No base usage, proper this.gameObject references
- [ ] **Comments intact** - No existing comments removed
- [ ] **No style-only changes** - Formatting changes aren't the primary modification
- [ ] **Risk assessment** - Change risk is acceptable given the recovery phase

---

## Examples

### ✅ Example 1: Safe Merge (Bug Fix + Null Check)

```csharp
// BEFORE (from backup)
private void Initialize()
{
    Transform child = transform.Find("child");
    child.SetActive(true);  // ⚠️ Potential null reference
}

// AFTER (recovery merge)
private void Initialize()
{
    Transform child = this.gameObject.transform.Find("child");
    if (child == null)
    {
        Debug.LogWarning("Child not found");
        return;
    }
    
    child.gameObject.SetActive(true);
}
```

**Decision**: ✅ MERGE - Adds null check (functional improvement)

### ❌ Example 2: Skip Merge (Style Only)

```csharp
// BEFORE
using System.Collections;
using UnityEngine;
private int count = 0;

// AFTER (proposal)
using UnityEngine;
private int count;  // Removed unused using and default initialization
```

**Decision**: ❌ SKIP - No functional improvement, only style change

### ✅ Example 3: Safe Merge (Constant Definition)

```csharp
// BEFORE
if (count > 10) { Debug.Log("Too many"); }

// AFTER
private const int MAX_ITEM_COUNT = 10;
if (count > MAX_ITEM_COUNT) { Debug.Log("Too many items"); }
```

**Decision**: ✅ MERGE - Improves code maintainability with constant

---

## Related Documentation

- [AGENTS.md](../AGENTS.md) - AI Agent guidelines
- [docs/coding-standards.md](coding-standards.md) - General coding standards
- [.github/instructions.md](../.github/instructions.md) - Project management guide

---

**Last Updated**: 2026-01-20

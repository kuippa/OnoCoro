---
name: microsoft-code-reference
description: Look up Microsoft API references, find working code samples, and verify SDK code is correct. Use when working with Azure SDKs, .NET libraries, or Microsoft APIs—to find the right method, check parameters, get working examples, or troubleshoot errors. Catches hallucinated methods, wrong signatures, and deprecated patterns by querying official docs. **OnoCoro Customized for Unity/C# Development.**
compatibility: Requires Microsoft Learn MCP Server (https://learn.microsoft.com/api/mcp)
---

# Microsoft Code Reference (OnoCoro Customized)

Look up Microsoft API references, find working code samples, and verify SDK code is correct. This skill is customized for **OnoCoro C# development, Recovery phase code validation, and PLATEAU SDK integration**.

## Tools

| Need | Tool | Example |
|------|------|---------|
| API method/class lookup | `microsoft_docs_search` | `"BlobClient UploadAsync Azure.Storage.Blobs"` |
| Working code sample | `microsoft_code_sample_search` | `query: "upload blob managed identity", language: "csharp"` |
| Full API reference | `microsoft_docs_fetch` | Fetch URL from `microsoft_docs_search` (for overloads, full signatures) |

## Finding Code Samples

Use `microsoft_code_sample_search` to get official, working examples:

```
microsoft_code_sample_search(query: "null checking patterns C#", language: "csharp")
microsoft_code_sample_search(query: "async await patterns Recovery phase", language: "csharp")
microsoft_code_sample_search(query: "resource management C# dispose pattern", language: "csharp")
microsoft_code_sample_search(query: "exception handling Unity C#", language: "csharp")
```

**When to use:**
- Before writing code—find a working pattern to follow
- After errors—compare your code against a known-good sample
- Unsure of initialization/setup—samples show complete context
- During Recovery phase code review—validate patterns against official standards

## API Lookups

```
# Verify method exists (include namespace for precision)
"Transform.Find() UnityEngine.Transform"
"GetComponent<T>() generic method Unity"
"Instantiate() method signature parameters"

# Find class/interface
"DefaultAzureCredential class Azure.Identity"
"IDisposable interface C# dispose pattern"

# Find correct package
"Azure Blob Storage NuGet package"
"Newtonsoft.Json NuGet package"
```

Fetch full page when method has multiple overloads or you need complete parameter details.

## OnoCoro-Specific Patterns

### Recovery Phase Validation

```
# Null checking patterns
microsoft_code_sample_search(query: "null check guard clause C#", language: "csharp")
microsoft_docs_search(query: "C# null-coalescing operator best practices")

# Transform/Component lookup
microsoft_code_sample_search(query: "Transform.Find() null check pattern", language: "csharp")
microsoft_code_sample_search(query: "GetComponent<T>() error handling", language: "csharp")
```

### PrefabManager Pattern

```
microsoft_code_sample_search(query: "resource management singleton pattern C#", language: "csharp")
microsoft_code_sample_search(query: "caching strategy dictionary C#", language: "csharp")
microsoft_docs_search(query: "C# static dictionary performance best practices")
```

### PLATEAU SDK Integration

```
microsoft_docs_search(query: ".NET geospatial coordinate transformation")
microsoft_code_sample_search(query: "mesh generation C# procedural", language: "csharp")
microsoft_docs_search(query: "C# large dataset processing performance")
```

## Error Troubleshooting

Use `microsoft_code_sample_search` to find working code samples and compare with your implementation. For specific errors, use `microsoft_docs_search` and `microsoft_docs_fetch`:

| Error Type | Query |
|------------|-------|
| NullReferenceException | `"null reference exception prevention C# patterns"` |
| Method not found | `"[ClassName] methods [Namespace]"` |
| Type not found | `"[TypeName] namespace import required"` |
| Wrong signature | `"[ClassName] [MethodName] overloads"` → fetch full page |
| Component missing | `"GetComponent<T>() null check pattern Unity"` |
| Resource leak | `"IDisposable pattern C# proper cleanup"` |
| Async deadlock | `"async await best practices C# deadlock prevention"` |

## OnoCoro Recovery Phase Workflow

### Before Writing Code

1. **Search for pattern** — `microsoft_code_sample_search(query: "[pattern]", language: "csharp")`
2. **Review official sample** — Compare with AGENTS.md standards
3. **Implement** with confidence that pattern is validated

### After NullReferenceException

1. **Identify the issue** — What object is null?
2. **Search for pattern** — `microsoft_code_sample_search(query: "null check guard clause", language: "csharp")`
3. **Apply pattern** — Add proper null checks with early return
4. **Reference** [.github/instructions/unity-csharp-recovery.instructions.md](../../instructions/unity-csharp-recovery.instructions.md)

### During Code Review

1. **Check method signature** — `microsoft_docs_search("[ClassName] [MethodName] signature")`
2. **Verify pattern** — `microsoft_code_sample_search("[similar pattern]", language: "csharp")`
3. **Validate** against [AGENTS.md](../../../AGENTS.md) compliance

## When to Verify

Always verify when:
- Method name seems "too convenient" (`UploadFile` vs actual `Upload`)
- Mixing SDK versions (v11 `CloudBlobClient` vs v12 `BlobServiceClient`)
- Package name doesn't follow conventions (`Azure.*` for .NET)
- Using an API for the first time
- In Recovery phase—old code may have deprecated patterns

## Validation Workflow

Before generating code or during Recovery merge, verify it's correct:

1. **Confirm method or package exists** — `microsoft_docs_search(query: "[ClassName] [MethodName] [Namespace]")`
2. **Fetch full details** (for overloads/complex params) — `microsoft_docs_fetch(url: "...")`
3. **Find working sample** — `microsoft_code_sample_search(query: "[task]", language: "csharp")`

For simple lookups, step 1 alone may suffice. For complex API usage, complete all three steps.

## OnoCoro Code Review Examples

### Example 1: Transform.Find() Pattern

```csharp
// Question: Is this null check pattern correct?
Transform absorbCollider = transform.Find("absorbcollider");
if (absorbCollider == null)
{
    Debug.LogWarning("absorbcollider not found");
    return;
}
```

**Verification Steps:**
```
1. microsoft_docs_search(query: "Transform.Find() null check C# Unity")
2. microsoft_code_sample_search(query: "Transform.Find() error handling", language: "csharp")
3. Confirm pattern matches [.github/instructions/unity-csharp-recovery.instructions.md](../../instructions/unity-csharp-recovery.instructions.md)
```

### Example 2: GetComponent() with Validation

```csharp
// Question: Should I validate the component exists?
RainAbsorbCtrl rainAbsorb = absorbCollider.GetComponent<RainAbsorbCtrl>();
if (rainAbsorb == null)
{
    Debug.LogWarning("RainAbsorbCtrl component not found");
    return;
}
```

**Verification Steps:**
```
1. microsoft_code_sample_search(query: "GetComponent<T>() null validation pattern", language: "csharp")
2. microsoft_docs_search(query: "Component lifecycle validation best practices")
3. Cross-reference [AGENTS.md](../../../AGENTS.md) Coding Standards section
```

### Example 3: Resource Management Pattern

```csharp
// Question: Is this the right way to manage prefab caching?
private static Dictionary<PrefabType, GameObject> prefabCache = 
    new Dictionary<PrefabType, GameObject>();
```

**Verification Steps:**
```
1. microsoft_docs_search(query: "C# static dictionary caching patterns performance")
2. microsoft_code_sample_search(query: "singleton pattern static cache C#", language: "csharp")
3. Validate against [.github/instructions/prefab-asset-management.instructions.md](../../instructions/prefab-asset-management.instructions.md)
```

---

**Note**: This skill is optimized for OnoCoro development with emphasis on Recovery phase code validation. For general .NET/Azure documentation, refer to official [learn.microsoft.com](https://learn.microsoft.com).

**Related Documentation**:
- [AGENTS.md](../../../AGENTS.md) - AI Agent guidelines
- [.github/instructions/unity-csharp-recovery.instructions.md](../../instructions/unity-csharp-recovery.instructions.md) - Recovery phase C# standards
- [docs/coding-standards.md](../../../docs/coding-standards.md) - Detailed C# standards
- [docs/recovery-workflow.md](../../../docs/recovery-workflow.md) - Recovery merge rules

**Last Updated**: 2026-01-20

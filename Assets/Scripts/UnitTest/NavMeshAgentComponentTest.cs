using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMeshAgent.AddComponent<NavMeshAgent>() の詳細診断クラス
/// 複数のパターンで AddComponent 成功/失敗を検証
/// </summary>
public class NavMeshAgentComponentTest : MonoBehaviour
{
    [SerializeField]
    private Vector3[] _testPositions = new Vector3[]
    {
        new Vector3(5f, 0f, 5f),      // path_marker_start
        new Vector3(8f, 0f, 8f),      // path_marker_03
        new Vector3(11f, 0f, 2f),     // path_marker_04
        new Vector3(15f, 0f, 5f)      // path_marker_goal
    };

    private const float _SEARCH_DISTANCE = 50f;
    private const string _LOG_PREFIX = "[NavMeshAgentComponentTest]";

    private void Start()
    {
        Debug.Log($"{_LOG_PREFIX} 詳細診断テスト開始");
        TestNavMeshAgentCreation();
    }

    private void TestNavMeshAgentCreation()
    {
        for (int i = 0; i < _testPositions.Length; i++)
        {
            Vector3 originalPosition = _testPositions[i];
            Debug.Log($"{_LOG_PREFIX} ===== テスト #{i + 1}: ({originalPosition.x}, {originalPosition.y}, {originalPosition.z}) =====");

            // ステップ1: NavMesh.SamplePosition でサンプリング
            Vector3 sampledPosition = SampleNavMeshPosition(originalPosition);
            if (sampledPosition == Vector3.zero)
            {
                continue;
            }

            // ステップ2: 複数のパターンでテスト
            TestPattern_EmptyGameObject(i, sampledPosition);
            TestPattern_WithBoxCollider(i, sampledPosition);
            TestPattern_WithCreatePrimitive(i, sampledPosition);
            TestPattern_WithCreatePrimitive_NoCollider(i, sampledPosition);

            Debug.Log($"{_LOG_PREFIX}");
        }

        Debug.Log($"{_LOG_PREFIX} 詳細診断テスト完了");
    }

    private Vector3 SampleNavMeshPosition(Vector3 position)
    {
        Debug.Log($"{_LOG_PREFIX}   NavMesh.SamplePosition()");
        Debug.Log($"{_LOG_PREFIX}     元: ({position.x}, {position.y}, {position.z})");

        if (NavMesh.SamplePosition(position, out var hit, _SEARCH_DISTANCE, NavMesh.AllAreas))
        {
            Debug.Log($"{_LOG_PREFIX}     ✓ サンプル位置: ({hit.position.x:F2}, {hit.position.y:F2}, {hit.position.z:F2}) [距離: {hit.distance:F2}f]");
            return hit.position;
        }
        else
        {
            Debug.LogError($"{_LOG_PREFIX}     ✗ サンプリング失敗");
            return Vector3.zero;
        }
    }

    /// <summary>パターン1: 空の GameObject</summary>
    private void TestPattern_EmptyGameObject(int index, Vector3 position)
    {
        Debug.Log($"{_LOG_PREFIX}   [パターン1] 空の GameObject");
        GameObject testObj = new GameObject($"Test_Empty_{index}");
        testObj.transform.position = position;
        TryAddNavMeshAgent(testObj, "空");
        Destroy(testObj);
    }

    /// <summary>パターン2: BoxCollider 付き GameObject</summary>
    private void TestPattern_WithBoxCollider(int index, Vector3 position)
    {
        Debug.Log($"{_LOG_PREFIX}   [パターン2] BoxCollider 付き GameObject");
        GameObject testObj = new GameObject($"Test_BoxCollider_{index}");
        testObj.transform.position = position;
        testObj.AddComponent<BoxCollider>();
        TryAddNavMeshAgent(testObj, "BoxCollider有");
        Destroy(testObj);
    }

    /// <summary>パターン3: CreatePrimitive (Collider 有)・修正前</summary>
    private void TestPattern_WithCreatePrimitive(int index, Vector3 position)
    {
        Debug.Log($"{_LOG_PREFIX}   [パターン3] CreatePrimitive(Cube) + Collider 無効化");
        GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testObj.name = $"Test_Primitive_{index}";
        testObj.transform.position = position;
        MeshRenderer renderer = testObj.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.enabled = false;
        
        BoxCollider collider = testObj.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.enabled = false;
        }
        
        TryAddNavMeshAgent(testObj, "Primitive+Collider無効");
        Destroy(testObj);
    }

    /// <summary>パターン4: CreatePrimitive (Collider 削除)・修正後</summary>
    private void TestPattern_WithCreatePrimitive_NoCollider(int index, Vector3 position)
    {
        Debug.Log($"{_LOG_PREFIX}   [パターン4] CreatePrimitive(Cube) + Collider 削除");
        GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testObj.name = $"Test_Primitive_NoCollider_{index}";
        testObj.transform.position = position;
        MeshRenderer renderer = testObj.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.enabled = false;
        
        BoxCollider collider = testObj.GetComponent<BoxCollider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }
        
        TryAddNavMeshAgent(testObj, "Primitive+Collider削除");
        Destroy(testObj);
    }

    private void TryAddNavMeshAgent(GameObject testObj, string patternName)
    {
        Debug.Log($"{_LOG_PREFIX}     AddComponent<NavMeshAgent>() [{patternName}]");
        Debug.Log($"{_LOG_PREFIX}       位置: {testObj.transform.position}");

        try
        {
            // 追加前の状態確認
            BoxCollider boxCol = testObj.GetComponent<BoxCollider>();
            Debug.Log($"{_LOG_PREFIX}       追加前 - BoxCollider: {(boxCol != null ? "有" : "無")}");

            NavMeshAgent agent = testObj.AddComponent<NavMeshAgent>();
            
            if (agent != null)
            {
                Debug.Log($"{_LOG_PREFIX}       ✓ コンポーネント追加成功");
                Debug.Log($"{_LOG_PREFIX}         isOnNavMesh: {agent.isOnNavMesh}");
                Debug.Log($"{_LOG_PREFIX}         isActiveAndEnabled: {agent.isActiveAndEnabled}");
                
                // NavMesh上にないなら位置をサンプリングして修正してみる
                if (!agent.isOnNavMesh)
                {
                    Debug.Log($"{_LOG_PREFIX}         isOnNavMesh=False → Warp で修正試行");
                    if (NavMesh.SamplePosition(testObj.transform.position, out var hit, 50f, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);
                        Debug.Log($"{_LOG_PREFIX}         Warp後 isOnNavMesh: {agent.isOnNavMesh}");
                    }
                }
            }
            else
            {
                Debug.LogError($"{_LOG_PREFIX}       ✗ null が返された");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{_LOG_PREFIX}       ✗ 例外: {ex.Message}");
        }
    }
}

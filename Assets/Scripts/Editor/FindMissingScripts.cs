using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FindMissingScripts : MonoBehaviour
{
    [MenuItem("Tools/Debug/Find Missing Scripts In Scene")]
    static void Find()
    {
        int goCount = 0;
        int missingCount = 0;
        List<string> results = new List<string>();

        // 비활성 포함 모든 오브젝트 검색
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (GameObject go in allObjects)
        {
            goCount++;
            Component[] comps = go.GetComponents<Component>();

            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    missingCount++;
                    string path = GetFullPath(go);
                    results.Add($"[{missingCount}] Missing Script in: {path}");
                }
            }
        }

        if (missingCount == 0)
        {
            Debug.Log($" Missing Script 없음. (총 {goCount}개 오브젝트 검사됨)");
        }
        else
        {
            Debug.LogWarning($" Missing Script {missingCount}개 발견!\n");

            foreach (string line in results)
                Debug.Log(line);
        }
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform;

        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
}

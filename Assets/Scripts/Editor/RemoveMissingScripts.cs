using UnityEditor;
using UnityEngine;

public class RemoveMissingScripts : MonoBehaviour
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Scene")]
    static void RemoveInScene()
    {
        var all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int goCount = 0, compCount = 0, removedCount = 0;
        foreach (var go in all)
        {
            goCount++;
            var comps = go.GetComponents<Component>();
            var so = new SerializedObject(go);
            var prop = so.FindProperty("m_Component");
            int r = 0;
            for (int i = 0; i < comps.Length; i++)
            {
                compCount++;
                if (comps[i] == null) { prop.DeleteArrayElementAtIndex(i - r); removedCount++; r++; }
            }
            so.ApplyModifiedProperties();
        }
        Debug.Log($"[Cleanup] GameObjects: {goCount}, Components: {compCount}, Removed: {removedCount}");
    }
}

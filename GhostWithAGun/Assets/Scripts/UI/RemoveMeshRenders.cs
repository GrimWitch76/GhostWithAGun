using UnityEditor;
using UnityEngine;


public class RemoveMeshRenderers : MonoBehaviour
{
    [MenuItem("Tools/Remove MeshRenderers From Children")]
    private static void RemoveMeshRenderersFromChildren()
    {
        // Loop through all selected GameObjects in the hierarchy
        foreach (GameObject go in Selection.gameObjects)
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            int count = 0;

            foreach (MeshRenderer r in renderers)
            {
                Undo.DestroyObjectImmediate(r);
                count++;
            }

            Debug.Log($"Removed {count} MeshRenderer(s) from {go.name} and its children.");
        }
    }
}
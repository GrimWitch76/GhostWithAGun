using UnityEngine;
using System.Collections.Generic;

public class SoundGizmoDrawer : MonoBehaviour
{
    private class GizmoData
    {
        public Vector3 Position;
        public float Radius;
        public float ExpireTime;
    }

    private static List<GizmoData> gizmos = new List<GizmoData>();

    public static void DrawSound(Vector3 position, float radius, float duration = 2f)
    {
        gizmos.Add(new GizmoData
        {
            Position = position,
            Radius = radius,
            ExpireTime = Time.time + duration
        });
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        // Remove expired gizmos
        gizmos.RemoveAll(g => g.ExpireTime < Time.time);

        // Draw active ones
        foreach (var g in gizmos)
        {
            Gizmos.DrawWireSphere(g.Position, g.Radius);
        }
    }
}

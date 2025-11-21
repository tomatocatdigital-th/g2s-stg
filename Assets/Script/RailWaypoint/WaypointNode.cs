using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    [Header("Next nodes (สาขา)")]
    public WaypointNode[] nextWaypoints;

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public bool onlyWhenSelected = false;
    public float nodeRadius = 0.18f;
    public Color nodeColor = new Color(1f, 1f, 0f, 0.95f);
    public Color lineColor = new Color(0f, 1f, 1f, 0.95f);
    public float arrowSize = 0.35f;
    public float arrowBack = 0.8f;

    void OnDrawGizmos() { if (drawGizmos && !onlyWhenSelected) Draw(); }
    void OnDrawGizmosSelected() { if (drawGizmos && onlyWhenSelected) Draw(); }

    void Draw()
    {
        Vector3 a = transform.position;
        Gizmos.color = nodeColor;
        Gizmos.DrawWireSphere(a, nodeRadius);

        if (nextWaypoints == null) return;
        Gizmos.color = lineColor;

        foreach (var next in nextWaypoints)
        {
            if (!next) continue;
            Vector3 b = next.transform.position;
            Gizmos.DrawLine(a, b);

            Vector3 dir = b - a;
            float len = dir.magnitude;
            if (len < 1e-4f) continue;

            dir /= len;
            Vector3 tip = b;
            Vector3 basePos = b - dir * arrowBack;
            Vector3 right = Quaternion.Euler(0, +45, 0) * (-dir);
            Vector3 left = Quaternion.Euler(0, -45, 0) * (-dir);
            Gizmos.DrawLine(tip, basePos + right * arrowSize);
            Gizmos.DrawLine(tip, basePos + left * arrowSize);
        }
    }
}

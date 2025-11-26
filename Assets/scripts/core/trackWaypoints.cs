using System.Collections.Generic;
using UnityEngine;

public class trackWaypoints : MonoBehaviour {

    [Header("this will only show the points visually , nothing else")]

    public bool drawOnlyOnSelected = false;
    public bool closeLoop = false;
    public bool showArrowHeads = true;
    public bool reversepath = false;

    public Color linecolor;
    [Range(0, 1)] public float SphereRadius;
    public List<Transform> nodes = new List<Transform>();
    [Range(0, 10)] public float arrowHeadLength = 0.25f; // Length of the arrow head lines
    [Range(10, 100)] public float arrowHeadAngle = 20.0f; // Angle of the arrow head lines=

    private void OnDrawGizmosSelected() {
        if (drawOnlyOnSelected) gizmosLogic();
    }

    private void OnDrawGizmos() {
        if (!drawOnlyOnSelected) gizmosLogic();

    }

    /// <summary>
    /// Calculates a point on a Catmull-Rom spline.
    /// t is the time value, where 0 <= t <= 1.
    /// </summary>
    public static Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
        // T^2
        float t2 = t * t;
        // T^3
        float t3 = t2 * t;

        // Blending functions for Catmull-Rom:
        // (0.5 * (-t3 + 2t2 - t)) * p0
        Vector3 a = 0.5f * (2f * p1);
        Vector3 b = 0.5f * (-p0 + p2) * t;
        Vector3 c = 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2;
        Vector3 d = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3) * t3;

        return a + b + c + d;
    }


    // Add this variable to your class fields
    [Range(10, 200)] public int splineSegmentsPerNode = 50;

    // ... inside gizmosLogic()
    void gizmosLogic() {
        Gizmos.color = linecolor;

        // ... (Your code to populate 'nodes' list remains the same)

        Transform[] path = GetComponentsInChildren<Transform>();

        nodes = new List<Transform>();
        for (int i = 1; i < path.Length; i++) {
            nodes.Add(path[i]);
        }

        if (reversepath) {
            nodes.Reverse();
        }

        // --- Start Spline Logic ---

        // Need at least 2 nodes to draw a line, and 4 for a proper Catmull-Rom segment
        if (nodes.Count < 2) {
            // Still draw the spheres for visibility
            for (int i = 0; i < nodes.Count; i++) {
                Gizmos.DrawWireSphere(nodes[i].position, SphereRadius);
            }
            return;
        }

        Vector3 previousPosition = nodes[0].position;

        for (int i = 0; i < nodes.Count; i++) {
            // Draw spheres at the actual waypoints
            Gizmos.DrawWireSphere(nodes[i].position, SphereRadius);

            // Define the four control points (P0, P1, P2, P3) for the segment P1-P2
            Vector3 p0 = nodes[i == 0 ? (closeLoop ? nodes.Count - 1 : 0) : i - 1].position;
            Vector3 p1 = nodes[i].position;
            Vector3 p2 = nodes[i == nodes.Count - 1 ? (closeLoop ? 0 : nodes.Count - 1) : i + 1].position;
            Vector3 p3 = nodes[i >= nodes.Count - 2 ? (closeLoop ? (i + 2) % nodes.Count : nodes.Count - 1) : i + 2].position;

            // If the path is not a loop, handle the start and end caps by doubling the first/last point
            if (!closeLoop) {
                if (i == 0) p0 = p1; // segment P1-P2, P1 is the first node
                if (i == nodes.Count - 2) p3 = p2; // segment P(N-2)-P(N-1), P(N-1) is the last node
                if (i == nodes.Count - 1) continue; // Skip the last node, as it's only P2 in the last segment
            } else {
                if (i == nodes.Count - 1) { // When drawing the last segment back to the start
                    p2 = nodes[0].position;
                    p3 = nodes[1].position;
                }
            }

            // If it's the very last segment and not a loop, stop here
            if (!closeLoop && i == nodes.Count - 1) continue;

            // --- Draw the Spline Segment ---

            previousPosition = p1; // Start the segment drawing from P1

            for (int j = 1; j <= splineSegmentsPerNode; j++) {
                float t = (float)j / splineSegmentsPerNode;

                // Get the next position on the curve
                Vector3 currentPosition = GetCatmullRomPosition(t, p0, p1, p2, p3);

                // Draw the tiny line segment
                Gizmos.DrawLine(previousPosition, currentPosition);

                // Only draw arrow heads on the actual node positions, or maybe the end of the segment
                // For smoother paths, drawing arrows only at the node positions (p2) is cleaner

                // --- Arrow Head Logic (Optional) ---
                if (showArrowHeads && j == splineSegmentsPerNode && currentPosition == p2) {
                    Vector3 direction = (currentPosition - previousPosition).normalized;
                    if (direction != Vector3.zero) {
                        // Recalculate right/left vectors using Quaternion.LookRotation
                        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
                        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
                        Gizmos.DrawLine(currentPosition, currentPosition + right * arrowHeadLength);
                        Gizmos.DrawLine(currentPosition, currentPosition + left * arrowHeadLength);
                    }
                }
                // --- End Arrow Head Logic ---

                previousPosition = currentPosition;
            }
        }
    }
}
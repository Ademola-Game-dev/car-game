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


    void gizmosLogic() {
        Gizmos.color = linecolor;

        Transform[] path = GetComponentsInChildren<Transform>();

        nodes = new List<Transform>();
        for (int i = 1; i < path.Length; i++) {
            nodes.Add(path[i]);
        }

        if (reversepath) {
            nodes.Reverse();
        }

        for (int i = 0; i < nodes.Count; i++) {
            Vector3 currentWaypoint = nodes[i].position;
            Vector3 previousWaypoint;

            if (i != 0) {
                previousWaypoint = nodes[i - 1].position;
            } else {
                if (closeLoop) {
                    previousWaypoint = nodes[nodes.Count - 1].position;
                } else {
                    previousWaypoint = nodes[0].position;
                }
            }

            Gizmos.DrawLine(previousWaypoint, currentWaypoint);
            Gizmos.DrawWireSphere(currentWaypoint, SphereRadius);

            // Draw arrow head
            if(!showArrowHeads) continue;
            Vector3 direction = (currentWaypoint - previousWaypoint).normalized;
            if (direction != Vector3.zero) // Avoid errors if previous and current are the same
            {
                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
                Gizmos.DrawLine(currentWaypoint, currentWaypoint + right * arrowHeadLength);
                Gizmos.DrawLine(currentWaypoint, currentWaypoint + left * arrowHeadLength);
            }
        }
    }


}
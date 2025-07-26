using UnityEngine;

public class scriptableRaccer : MonoBehaviour {

    public trackWaypoints track;


    public int currentnode = 0;
    public float speed = 1;

    public float minDistance = 0.1f;

    [Header("next node angle in degrees")]
    public float nextnodeangle;


    void FixedUpdate() {
        if (track.nodes.Count == 0) return;

        transform.LookAt(track.nodes[currentnode].position);

        transform.position += transform.forward * Time.deltaTime * speed;


        if (Vector3.Distance(transform.position, track.nodes[currentnode].position) < minDistance) {
            currentnode++;
            if (currentnode >= track.nodes.Count) {
                currentnode = 0;
            }
        }

        if (currentnode <= track.nodes.Count - 2) {

            nextnodeangle = Vector3.Angle(transform.position, track.nodes[currentnode+1].position );

        }


    }


}

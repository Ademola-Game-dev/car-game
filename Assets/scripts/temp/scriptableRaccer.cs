using UnityEngine;

public class scriptableRaccer : MonoBehaviour {

    public trackWaypoints track;

    public int currentnode = 0;
    [Range(1, 100)] public float speedModifier = 2;
    [Range(0, 400)] public float cornerSpeedModifier = 2;


    public float minDistance = 0.1f;

    [Header("next node angle in degrees")]
    public float nextnodeangle;

    public float steerAffect = 0;


    void Start() {
        transform.position = track.nodes[0].position;
        //currentnode = 0;
    }


    void FixedUpdate() {
        if (track.nodes.Count == 0) return;

        transform.LookAt(track.nodes[currentnode].position);

        transform.position += transform.forward * Time.deltaTime * (speedModifier - steerAffect);


        if (Vector3.Distance(transform.position, track.nodes[currentnode].position) < minDistance) {
            currentnode++;
            if (currentnode >= track.nodes.Count) {
                currentnode = 0;
            }
        }

        if (currentnode <= track.nodes.Count - 2) {
            //Vector3.Angle(A - B, C - B)
            nextnodeangle = Vector3.Angle(transform.position - track.nodes[currentnode].position, track.nodes[currentnode + 1].position - track.nodes[currentnode].position);
            steerAffect = (Mathf.Clamp(Mathf.Abs(180 - nextnodeangle), 0, 180) / 180) * (Mathf.Clamp(cornerSpeedModifier, 0, speedModifier));
        }


    }


}

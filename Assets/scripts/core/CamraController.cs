using Unity.Mathematics;
using UnityEngine;

public class CamraController : MonoBehaviour {

    //ref
    private CarStats stats;


    public Transform targetTransform;
    public Vector2 offset;
    public quaternion offsetRotation;
    public Vector3 lookOffset;
    public float smoothSpeed = 0.125f;

    [Header("the lower this is the closet the will be")]
    [Range(2, 8)] public float maxDistanceToTarget = 6f;

    // private
    private float distanceToTarget;
    public float lerpModifier = 1;

    void Start() {
        stats = targetTransform.GetComponent<CarStats>();
    }

    void FixedUpdate() {
        if (targetTransform == null) return;

        transform.position = Vector3.Lerp(transform.position, targetTransform.TransformPoint(new Vector3(0, offset.y, offset.x)), Time.deltaTime * smoothSpeed * lerpModifier);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetTransform.rotation * offsetRotation, Time.deltaTime * smoothSpeed );

        if (stats) {
            transform.LookAt(targetTransform.position + stats.lookAtPoint);
        } else {
            transform.LookAt(targetTransform.position + lookOffset);
        }

        // this will be used to make the camera not stay behing too far from the target
        distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
        lerpModifier = Mathf.Clamp(distanceToTarget / maxDistanceToTarget, 1, 20);

        // todo : add screen shake when going too fast

    }

    public void switchPositions(Vector2 newOffset) {
        offset = newOffset;
    }

}

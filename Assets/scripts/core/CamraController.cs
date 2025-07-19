using Unity.Mathematics;
using UnityEngine;

public class CamraController : MonoBehaviour {

    public Transform targetTransform;
    public Vector2 offset;
    public quaternion offsetRotation;
    public float smoothSpeed = 0.125f;


    void Start() {

    }

    void FixedUpdate() {
        transform.position = Vector3.Lerp(transform.position, targetTransform.TransformPoint(new Vector3(0, offset.y, offset.x)), Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetTransform.rotation * offsetRotation, Time.deltaTime * smoothSpeed);

    }

    public void switchPositions(Vector2 newOffset) {
        offset = newOffset;
    }

}
